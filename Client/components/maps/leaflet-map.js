class LeafletMap extends HTMLElement {
    static get observedAttributes() {
        return ["start", "end"];
    }

    static DEFAULT_START = [43.69676878749648, 7.2787016118204555];
    static DEFAULT_ZOOM = 14; //pls petit = plus zoomé
    static API_URL = "http://localhost:8733/Design_Time_Addresses/RoutingServer/Service1/suggestJourney";
    static TILES_URL = "http://{s}.tile.osm.org/{z}/{x}/{y}.png";
    static TILE_ATTRIBUTION = 'Leaflet &copy; <a href="http://openstreetmap.org">OpenStreetMap</a> contributors';
    static ICON_SIZE = [40, 40];
    static ANIMATION_INTERVAL = 300;    // plus petit = plus rapide
    static TRANSITION_TIME = 30;        // Transition entre itinéraires en secondes(walking -> cycling)

    static ICONS = {
        start: "../../assets/icons/pin.png",
        end: "../../assets/icons/location-pin.png",
        walking: "../../assets/icons/walking.png",
        cycling: "../../assets/icons/cycling.png",
        default: "../../assets/icons/location.png"
    };

    constructor() {
        super();
        this.attachShadow({ mode: "open" });

        this.start = LeafletMap.DEFAULT_START;
        this.end = null;
        this.originMarker = null;
        this.destMarker = null;
        this.travelerMarker = null;
        this.scriptsLoaded = false;
        this.clientId = `client-${Math.random().toString(36).substring(2, 9)}`;
        this.itinerariesQueue = [];

        this.initTemplate();
        this.getCurrentPosition();
    }

    connectedCallback() {
        this.loadScript("../../leaflet-1.8.0/leaflet.js")
            .then(() => {
                this.scriptsLoaded = true;
                this.initializeMap();
                this.notifyDepartureSelected();
            })
            .catch((error) => {
                console.error("Erreur lors de l'initialisation de la carte:", error);
            });
    }

    attributeChangedCallback(name, oldValue, newValue) {
        const value = JSON.parse(newValue);
        if (name === "start") {
            this.start = value;
            this.updateStartPoint();
        } else if (name === "end") {
            this.end = value;
            this.updateEndPoint();
        }
    }



    // ------------------------------------------------------
    // Récupération de la position actuelle de l'utilisateur
    // -------------------------------------------------------
    getCurrentPosition() {
        if ("geolocation" in navigator) {
            navigator.geolocation.getCurrentPosition(
                (position) => {
                    this.start = [position.coords.latitude, position.coords.longitude];
                    if (this.map) {
                        this.map.setView(this.start, LeafletMap.DEFAULT_ZOOM);
                        this.updateStartPoint();
                        this.notifyDepartureSelected();
                    }
                },
                (error) => {
                    console.warn("Erreur de géolocalisation:", error);
                    // Garde la position par défaut en cas d'erreur
                },
                {
                    enableHighAccuracy: true,
                    timeout: 5000,
                    maximumAge: 0
                }
            );
        }
    }

    // ---------------------------------------
    // Initialisation du template et du style
    // ---------------------------------------
    initTemplate() {
        const template = document.createElement("template");
        template.innerHTML = `
          <link rel="stylesheet" href="../../leaflet-1.8.0/leaflet.css">
           <style>
            .instructions {
                position: absolute;
                bottom: 15px;
                left: 50%;
                transform: translateX(-50%);
                background-color: #78B9BA;
                color: black;
                padding: 15px 20px;
                border-radius: 8px;
                font-size: 22px;
                min-width: 300px; 
                width: 80%; 
                max-width: 800px;
                text-align: center;
                z-index: 1000;
                box-shadow: 0 2px 4px rgba(0,0,0,0.2); /* Ajout d'une ombre pour meilleure visibilité */
            }
            .instructions.hidden {
                display: none;
            }
            #remaining-time {
                position: absolute; 
                top: 10px; 
                right: 10px; 
                background-color: #21324B; 
                color: #fff; 
                padding: 15px 15px; 
                border-radius: 8px; 
                font-size: 14px; 
                z-index: 1000;
            }
            #remaining-time.hidden {
                display: none;
            }
        </style>
        
        <div id="map" style="width: 100%; height: 100vh;"></div>
        <div class="instructions hidden" id="instructions"></div>
        <div class="hidden" id="remaining-time">Remaining time : 0 min</div>
        `;
        this.shadowRoot.appendChild(template.content.cloneNode(true));
    }

    // -----------------------------
    // Chargement des scripts externes
    // -----------------------------
    loadScript(src) {
        return new Promise((resolve, reject) => {
            const script = document.createElement("script");
            script.src = src;
            script.onload = () => resolve();
            script.onerror = () => reject(new Error(`Échec du chargement du script: ${src}`));
            this.shadowRoot.appendChild(script);
        });
    }

    // -------------------------------------------
    // Initialisation et configuration de la carte
    // -------------------------------------------
    initializeMap() {
        if (!this.scriptsLoaded) return;
        const mapElement = this.shadowRoot.getElementById("map");
        this.map = L.map(mapElement).setView(this.start, LeafletMap.DEFAULT_ZOOM);

        L.tileLayer(LeafletMap.TILES_URL, {
            attribution: LeafletMap.TILE_ATTRIBUTION,
            maxZoom: 18,
        }).addTo(this.map);

        this.addStartMarker();
        this.setupMapClickHandler();
        this.subscribeToQueue();
    }

    setupMapClickHandler() {
        this.map.on("click", async (event) => {
            const { lat, lng } = event.latlng;
            this.end = [lat, lng];
            this.addEndMarker();
            this.setAttribute("end", JSON.stringify([lat, lng]));

            const address = await this.fetchAddressFromCoordinates(lat, lng);
            this.dispatchEvent(new CustomEvent("destination-selected", {
                detail: { address, coordinates: [lat, lng] },
                bubbles: true,
                composed: true
            }));
        });
    }

    // -----------------------------------------
    // Mise à jour des points de départ / arrivée
    // -----------------------------------------
    updateStartPoint() {
        this.addStartMarker();
        this.updateRoute();
        if (this.map) this.map.setView(this.start, LeafletMap.DEFAULT_ZOOM);
    }

    updateEndPoint() {
        this.addEndMarker();
        this.updateRoute();
        if (this.map) this.map.setView(this.end, LeafletMap.DEFAULT_ZOOM);
    }

    // ---------------------------
    // Gestion des marqueurs
    // ---------------------------
    createMarkerIcon(iconUrl) {
        return L.icon({ iconUrl, iconSize: LeafletMap.ICON_SIZE });
    }

    addStartMarker() {
        if (this.originMarker) this.map.removeLayer(this.originMarker);
        const startIcon = this.createMarkerIcon(LeafletMap.ICONS.start);
        this.originMarker = L.marker(this.start, { icon: startIcon }).addTo(this.map);
    }

    addEndMarker() {
        if (this.destMarker) this.map.removeLayer(this.destMarker);
        const endIcon = this.createMarkerIcon(LeafletMap.ICONS.end);
        this.destMarker = L.marker(this.end, { icon: endIcon }).addTo(this.map);
    }

    // ---------------------------
    // Récupération et affichage de l'itinéraire
    // ---------------------------
    updateRoute() {
        if (!this.start || !this.end) {
            console.error("Erreur : les coordonnées de départ ou d'arrivée sont manquantes.");
            return;
        }

        const url = `${LeafletMap.API_URL}?startLat=${this.start[0]}&startLng=${this.start[1]}&endLat=${this.end[0]}&endLng=${this.end[1]}&clientId=${this.clientId}`;
        console.log("Requête Itinéraire:", url);

        fetch(url)
            .then((response) => {
                if (!response.ok) throw new Error(`Erreur HTTP : ${response.status}`);
                return response.json();
            })
            .then((data) => this.handleRouteData(data))
            .catch((error) => {
                console.error("Erreur lors de la récupération de l'itinéraire :", error);
            });
    }

    handleRouteData(data) {
        if (!data.suggestJourneyResult ||
            data.suggestJourneyResult.length === 0 ||
            !data.suggestJourneyResult[0].Value ||
            !data.suggestJourneyResult[0].Value.instructions ||
            data.suggestJourneyResult[0].Value.instructions.length === 0) {
            console.error("Aucun itinéraire trouvé ou données invalides.");
            return;
        }
        const route = data.suggestJourneyResult[0].Value;
        console.log("Itinéraire récupéré :", route);
        if (this.destMarker) this.destMarker.addTo(this.map);
    }

    // -------------------------------
    // Gestion de la file d'itinéraires
    // -------------------------------
    subscribeToQueue() {
        const client = new StompJs.Client({
            brokerURL: 'ws://localhost:61614',
            connectHeaders: { login: 'user', passcode: 'password' },
            debug: (str) => console.log(str),
            reconnectDelay: 5000,
            heartbeatIncoming: 4000,
            heartbeatOutgoing: 4000,
        });

        client.onConnect = (frame) => {
            console.log('Connecté à ActiveMQ:', frame);
            client.subscribe(`/queue/ItinerarySuggested-${this.clientId}`, (message) => {
                if (message.body) {
                    try {
                        const data = JSON.parse(message.body);
                        console.log('[ActiveMQ] Message reçu:', data);
                        this.handleAllItineraries(data);
                    } catch (error) {
                        console.error('[ActiveMQ] Erreur de traitement du message:', error);
                    }
                }
            });
        };

        client.onStompError = (frame) => {
            console.error('[ActiveMQ] Erreur du broker:', frame.headers['message']);
            console.error('[ActiveMQ] Détails:', frame.body);
        };

        client.activate();
    }

    /**
     * Traite tous les itinéraires reçus, 
     * les affiche, calcule les durées, et lance les animations séquentielles.
     */
    async handleAllItineraries(itinerariesData) {
        // if(!itinerariesData){
        //     const instructionsDiv = this.shadowRoot.getElementById("instructions");
        //     instructionsDiv.classList.remove("hidden");
        //     instructionsDiv.textContent = "Aucun itinéraire trouvé pour cette destination.";
            
        //     // Mettre à jour l'affichage du temps
        //     const timeElement = this.shadowRoot.getElementById("remaining-time");
        //     timeElement.textContent = "Temps total : - min";
    
        //     // Attendre quelques secondes puis réinitialiser
        //     setTimeout(() => {
        //         this.resetMap();
        //     }, 4000);
            
        //     return;
        // }


        this.itinerariesQueue = [];
        let totalDuration = 0;
        const durationsInfo = [];

        // Préparation des itinéraires
        for (const key in itinerariesData) {
            if (!itinerariesData.hasOwnProperty(key)) continue;

            const itinerary = itinerariesData[key];
            if (itinerary.instructions && itinerary.instructions.length > 0) {
                if (totalDuration > 0) totalDuration += LeafletMap.TRANSITION_TIME;

                const duration = itinerary.instructions.reduce((sum, step) => sum + step.duration, 0);
                durationsInfo.push({ type: key, startTime: totalDuration, endTime: totalDuration + duration });

                totalDuration += duration;
                this.itinerariesQueue.push({
                    type: key,
                    itinerary,
                    startTime: durationsInfo[durationsInfo.length - 1].startTime,
                    duration
                });
            }
        }
        
        
        document.querySelector('aside').classList.add('hidden');
        document.body.classList.add('fullmap');

        const remainingTimeElement = this.shadowRoot.getElementById("remaining-time");
        remainingTimeElement.classList.remove("hidden");

        this.updateRemainingTime(totalDuration);
        this.displayAllRoutes();
        await this.animateAllItineraries(totalDuration);

        // Arrivé à destination
        const instructionsDiv = this.shadowRoot.getElementById("instructions");
        instructionsDiv.textContent = "Destination reached !!";
        this.updateRemainingTime(0);


        setTimeout(() => {
            this.resetMap();
        }, 4000);
    }

    /**
     * Affiche tous les itinéraires (tous types confondus) sur la carte.
     */
    displayAllRoutes() {
        if (!this.routeLayers) this.routeLayers = [];
        for (let { type, itinerary } of this.itinerariesQueue) {
            const coordinates = itinerary.instructions.map(step => [step.position.lat, step.position.lng]);
            this.displayRoute(coordinates, type);
        }

        const allBounds = this.routeLayers.reduce(
            (bounds, layer) => bounds.extend(layer.getBounds()),
            L.latLngBounds()
        );
        this.map.fitBounds(allBounds);
    }

    /**
     * Affiche un itinéraire individuel.
     */
    displayRoute(coordinates, type) {
        console.log(`[displayRoute] Tracé de l'itinéraire du type '${type}'`);
        const color = type.includes("cycling") ? "blue" : "red";
        const latLngs = coordinates.map(coord => L.latLng(coord[0], coord[1]));
        const routeLayer = L.polyline(latLngs, { color }).addTo(this.map);
        this.routeLayers.push(routeLayer);
    }

    /**
     * Anime tous les itinéraires de manière séquentielle
     */
    async animateAllItineraries(totalDuration) {
        const startTime = Date.now();

        for (const queueItem of this.itinerariesQueue) {
            const steps = queueItem.itinerary.instructions;
            const coordinates = steps.map(step => [step.position.lat, step.position.lng]);

            await this.animateSingleItinerary(
                queueItem.type,
                coordinates,
                steps,
                startTime,
                totalDuration,
                queueItem.startTime,
                queueItem.duration
            );
        }
    }

    /**
     * Anime un itinéraire unique du début à la fin.
     */
    animateSingleItinerary(type, coordinates, steps, startTime, totalDuration, segmentStartTime, segmentDuration) {
        return new Promise((resolve) => {
            const iconUrl = type.includes("walking") ? LeafletMap.ICONS.walking :
                            type.includes("cycling") ? LeafletMap.ICONS.cycling :
                            LeafletMap.ICONS.default;

            if (this.travelerMarker) this.map.removeLayer(this.travelerMarker);
            this.travelerMarker = L.marker(coordinates[0], { icon: this.createMarkerIcon(iconUrl) }).addTo(this.map);

            const instructionsDiv = this.shadowRoot.getElementById("instructions");
            instructionsDiv.classList.remove("hidden");

            let index = 0;
            let stepIndex = 0;
            const stepDuration = segmentDuration / coordinates.length; // Pour un déplacement plus fluide

            const intervalId = setInterval(() => {
                if (index < coordinates.length) {
                    const [lat, lng] = coordinates[index];
                    this.travelerMarker.setLatLng([lat, lng]);
                    this.map.setView([lat, lng], this.map.getZoom(), { animate: true });

                    // Affichage de l'instruction correspondante
                    if (stepIndex < steps.length && index >= stepIndex) {
                        instructionsDiv.textContent = `Step ${stepIndex + 1}: ${steps[stepIndex].text}`;
                        stepIndex++;
                    }

                    // Mise à jour du temps restant
                    const elapsedTime = (Date.now() - startTime) / 1000;
                    const currentSegmentProgress = index * stepDuration;
                    const totalElapsedTime = segmentStartTime + currentSegmentProgress;
                    const remainingTime = Math.max(0, totalDuration - totalElapsedTime);
                    this.updateRemainingTime(remainingTime);

                    index++;
                } else {
                    clearInterval(intervalId);
                    resolve();
                }
            }, LeafletMap.ANIMATION_INTERVAL);
        });
    }

    // ---------------------------
    // Mise à jour du temps restant
    // ---------------------------
    updateRemainingTime(totalRemainingSeconds) {
        const timeElement = this.shadowRoot.getElementById("remaining-time");

        if (totalRemainingSeconds >= 3600) {
            const hours = Math.floor(totalRemainingSeconds / 3600);
            const minutes = Math.ceil((totalRemainingSeconds % 3600) / 60);
            console.log("Temps restant :", hours, "h", minutes, "min");
            timeElement.textContent = `Remaining time : ${hours}h ${minutes}min`;
        } else {
            const minutes = Math.ceil(totalRemainingSeconds / 60);
            console.log("Temps restant :", minutes, "min");
            timeElement.textContent = `Remaining time : ${minutes}min`;
        }

    }

    // -----------------------------------------
    // Récupération d'une adresse depuis lat/lon
    // -----------------------------------------
    async fetchAddressFromCoordinates(lat, lon) {
        const url = `https://api-adresse.data.gouv.fr/reverse/?lon=${lon}&lat=${lat}`;
        try {
            const response = await fetch(url);
            const data = await response.json();

            if (data.features && data.features.length > 0) {
                return data.features[0].properties.label;
            } else {
                console.warn("Aucune adresse trouvée pour ces coordonnées.");
                return "Adresse inconnue";
            }
        } catch (error) {
            console.error("Erreur lors de la récupération de l'adresse :", error);
            return "Erreur lors de la récupération de l'adresse";
        }
    }

    /**
     * Notifie l'élément parent que la position de départ est définie
     */
    async notifyDepartureSelected() {
        const departureAddress = await this.fetchAddressFromCoordinates(this.start[0], this.start[1]);
        this.dispatchEvent(new CustomEvent("departure-selected", {
            detail: {
                address: departureAddress,
                coordinates: [this.start[0], this.start[1]]
            },
            bubbles: true,
            composed: true
        }));
        console.log("Adresse de départ récupérée :", departureAddress);
    }

    /**
     * Réinitialise la carte et les marqueurs une fois que l'itinéraire est terminé.
     */
    resetMap() {
        [this.originMarker, this.destMarker, this.travelerMarker].forEach(marker => {
            if (marker) this.map.removeLayer(marker);
        });

        if (this.routeLayers) {
            this.routeLayers.forEach(layer => this.map.removeLayer(layer));
            this.routeLayers = [];
        }

        this.start = null;
        this.end = null;

        // Émission d'un événement pour réinitialiser les barres de recherche
        this.dispatchEvent(new CustomEvent("reset-search-bars", {
            detail: {},
            bubbles: true,
            composed: true
        }));

        // Réinitialiser l'affichage du temps restant
        const timeElement = this.shadowRoot.getElementById("remaining-time");
        timeElement.classList.add("hidden");
        timeElement.textContent = "Remaining time : 0 min";

        instructionsDiv.classList.add("hidden");
        if (instructionsDiv) instructionsDiv.classList.add("hidden");
        if (timeElement) timeElement.classList.add("hidden");


        document.querySelector('aside').classList.remove('hidden');
        document.body.classList.remove('fullmap');
    }
}

customElements.define("leaflet-map", LeafletMap);
