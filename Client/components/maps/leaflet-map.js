class LeafletMap extends HTMLElement {
    static get observedAttributes() {
        return ["start", "end"];
    }

    constructor() {
        super();
        this.attachShadow({ mode: "open" });
        this.start = [43.69676878749648, 7.2787016118204555]; // Coordonnées de départ par defaut
        this.end = null;

        this.originMarker = null; 
        this.destMarker = null;
        this.travelerMarker = null; 
        this.animationInterval = 30; // Intervalle de mise à jour pour l'animation (en ms)

        this.scriptsLoaded = false;
        this.clientId = `client-${Math.random().toString(36).substring(2, 9)}`;

        this.itinerariesQueue = []; // Pour stocker les itinéraires avant animation


    }

    async connectedCallback() {
        const template = document.createElement("template");
        template.innerHTML = `
          <link rel="stylesheet" href="../../leaflet-1.8.0/leaflet.css">
           <style>
            .instructions {
                position: absolute;
                bottom: 15px;
                left: 70%;
                transform: translateX(-50%);
                background-color: #78B9BA;
                color: black;
                padding: 10px;
                border-radius: 8px;
                font-size: 18px;
                max-width: 100%;
                text-align: center;
                z-index: 1000;
            }
            .instructions.hidden {
                display: none;
            }
        </style>
        <div id="map" style="width: 100%; height: 100vh;"></div>
        <div class="instructions hidden" id="instructions"></div>
      `;
        this.shadowRoot.appendChild(template.content.cloneNode(true));

        try {
            // Charger d'abord la carte
            await this.loadScript("../../leaflet-1.8.0/leaflet.js");
            this.scriptsLoaded = true;
            this.loadMap();
    
            // Ensuite récupérer l'adresse et émettre l'événement
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
        } catch (error) {
            console.error("Erreur lors de l'initialisation de la carte:", error);
        }
    }

    loadScript(src) {
        return new Promise((resolve, reject) => {
            const script = document.createElement("script");
            script.src = src;
            script.onload = () => resolve();
            script.onerror = () => reject(new Error(`Échec du chargement du script: ${src}`));
            this.shadowRoot.appendChild(script);
        });
    }

    loadMap() {
        if (!this.scriptsLoaded) return;

        const mapElement = this.shadowRoot.getElementById("map");
        this.map = L.map(mapElement).setView(this.start, 16);

        L.tileLayer("http://{s}.tile.osm.org/{z}/{x}/{y}.png", {
            attribution:'Leaflet &copy; <a href="http://openstreetmap.org">OpenStreetMap</a> contributors',
            maxZoom: 18,
        }).addTo(this.map);



        this.addStartMarker();

        this.subscribeToQueue();

        //when an address on the map is clicked destiionnation coordinates are updated
        this.map.on("click", async (event) => {
            const { lat, lng } = event.latlng;
            console.log("click long", lng);
            console.log("click lat", lat);

            this.end = [lat, lng];
            this.addEndMarker();
            
            this.setAttribute("end", JSON.stringify([lat, lng])); 

            const address = await this.fetchAddressFromCoordinates(lat, lng);

            // Émettre un événement pour informer la `search-bar` de destination
            this.dispatchEvent(new CustomEvent("destination-selected", {
                detail: {
                    address: address,
                    coordinates: [lat, lng]
                },
                bubbles: true,
                composed: true
            }));
        });
    }

    attributeChangedCallback(name, oldValue, newValue) {
        const value = JSON.parse(newValue);
        if (name === "start") {
            this.start = value;
            this.addStartMarker();
            this.updateRoute();
            if (this.map) {
                this.map.setView(this.start, 16); // 16 est le niveau de zoom
            }
        } else if (name === "end") {
            this.end = value;
            this.addEndMarker();
            this.updateRoute();
            if (this.map) {
                this.map.setView(this.end, 16); // 16 est le niveau de zoom
            }
        }
    }

    addStartMarker() {
        const startIcon = L.icon({
            iconUrl: "../../assets/icons/pin.png",
            iconSize: [40, 40],
        });

        if (this.originMarker) {
            this.map.removeLayer(this.originMarker);
        }
        this.originMarker = L.marker(this.start, { icon: startIcon }).addTo(this.map);
    }

    addEndMarker() {
        const endIcon = L.icon({
            iconUrl: "../../assets/icons/location-pin.png",
            iconSize: [40, 40],
        });

        if (this.destMarker) {
            this.map.removeLayer(this.destMarker);
        }
        this.destMarker = L.marker(this.end, { icon: endIcon }).addTo(this.map);
    }

    updateOriginMarker() {
        if (!this.start || !this.map) return;
        if (this.originMarker) {
            this.originMarker.setLatLng(L.latLng(...this.start));
        } else {
            this.addStartMarker();
        }
    }

    updateRoute() {
        if (!this.start || !this.end){
            console.error("Erreur : les coordonnées de départ ou d'arrivée sont manquantes.");
            return;
        }

        const url = `http://localhost:8733/Design_Time_Addresses/RoutingServer/Service1/suggestJourney?startLat=${this.start[0]}&startLng=${this.start[1]}&endLat=${this.end[0]}&endLng=${this.end[1]}&clientId=${this.clientId}`;
    
        console.log("Départ :", this.start);
        console.log("Arrivée :", this.end);
        console.log("clientId :", this.clientId);
        console.log("Requête envoyée à :", url);

        fetch(url)
            .then((response) => {
                if (!response.ok) {
                    console.error("Erreur HTTP :", response.status);
                    throw new Error(`Erreur HTTP : ${response.status}`);
                }
                console.log("response eeh", response);
                return response.json();
            })
            .then((data) => {
                console.log("response data eeh", data);
                if (
                    !data.suggestJourneyResult ||
                    data.suggestJourneyResult.length === 0 ||
                    !data.suggestJourneyResult[0].Value ||
                    !data.suggestJourneyResult[0].Value.instructions ||
                    data.suggestJourneyResult[0].Value.instructions.length === 0
                ) {
                    throw new Error("Aucun itinéraire trouvé ou données invalides.");
                }

                const route = data.suggestJourneyResult[0].Value;
                console.log("Données de l'itinéraire récupérées :", route);
    

    
                if (this.destMarker) {
                    this.destMarker.addTo(this.map);
                }
            })
            .catch((error) => {
                console.error("Erreur lors de la récupération de l'itinéraire :", error);
            });
    }

    subscribeToQueue() {
        const client = new StompJs.Client({
            brokerURL: 'ws://localhost:61614',
            connectHeaders: {
                login: 'user',
                passcode: 'password',
            },
            debug: function (str) {
                console.log(str);
            },
            reconnectDelay: 5000,
            heartbeatIncoming: 4000,
            heartbeatOutgoing: 4000,
        });

        client.onConnect = (frame) => {
            console.log('Connected to ActiveMQ:', frame);

            client.subscribe(`/queue/ItinerarySuggested-${this.clientId}`, (message) => {
                if (message.body) {
                    try {
                        const data = JSON.parse(message.body);
                        console.log('[ActiveMQ] Message received:', data);
                        this.handleAllItineraries(data);

                    } catch (error) {
                        console.error('[ActiveMQ] Error processing message:', error);
                    }
                }
            });
        };
        client.onStompError = (frame) => {
            console.error('[ActiveMQ] Broker error:', frame.headers['message']);
            console.error('[ActiveMQ] Details:', frame.body);
        };
        client.activate();
    }

    // Gestion de l'animation des itinéraires, 
    //on affiche d'abord les itinéraires puis
    // on les anime un par un
    async handleAllItineraries(itinerariesData) {
        this.itinerariesQueue = [];
    
        for (const key in itinerariesData) {
            if (itinerariesData.hasOwnProperty(key)) {
                const itinerary = itinerariesData[key];
                if (itinerary.instructions && itinerary.instructions.length > 0) {
                    this.itinerariesQueue.push({ type: key, itinerary });
                }
            }
        }
    
        // Afficher les itinéraires
        for (let i = 0; i < this.itinerariesQueue.length; i++) {
            const { type, itinerary } = this.itinerariesQueue[i];
            const coordinates = itinerary.instructions.map(step => [step.position.lat, step.position.lng]);
            this.displayRoute(coordinates, type);
        }
    
        // Animer les itinéraires
        for (let i = 0; i < this.itinerariesQueue.length; i++) {
            const { type, itinerary } = this.itinerariesQueue[i];
            const coordinates = itinerary.instructions.map(step => [step.position.lat, step.position.lng]);
            await this.animateRouteSequential(type, coordinates, itinerary.instructions);
        }
    
        const instructionsDiv = this.shadowRoot.getElementById("instructions");
        instructionsDiv.textContent = "Vous êtes arrivé à destination !";



        //attendre 5 secondes avant de réinitialiser la carte
        setTimeout(() => {
            this.resetMap();
        }, 4000);
        

    }

    displayRoute(coordinates, type) {
        console.log(`[displayRoute] Drawing route of type '${type}'`);
    
        let colorOfItinerary = "red";
        if (type.includes("cycling")) {
            colorOfItinerary = "blue";
        }

        const latLngs = coordinates.map((coord) => L.latLng(coord[0], coord[1]));

        if (!this.routeLayers) {
            this.routeLayers = [];
        }

        const routeLayer = L.polyline(latLngs, { color: colorOfItinerary }).addTo(this.map);
        this.routeLayers.push(routeLayer);

        // On ajuste la vue pour montrer tous les itinéraires affichés
        const allBounds = this.routeLayers.reduce((bounds, layer) => bounds.extend(layer.getBounds()), L.latLngBounds());
        this.map.fitBounds(allBounds);
    }

    animateRouteSequential(type, coordinates, steps) {
        return new Promise((resolve) => {
            let iconUrl;
            if (type.includes("walking")) {
                iconUrl = "../../assets/icons/walking.png";
            } else if (type.includes("cycling")) {
                iconUrl = "../../assets/icons/cycling.png";
            } else {
                iconUrl = "../../assets/icons/location.png";
            }
    
            const markerIcon = L.icon({
                iconUrl: iconUrl,
                iconSize: [40, 40],
            });
    
            if (this.travelerMarker) {
                this.map.removeLayer(this.travelerMarker);
            }
    
            this.travelerMarker = L.marker(coordinates[0], { icon: markerIcon }).addTo(this.map);
        
            let index = 0;
            let stepIndex = 0;
        
            const instructionsDiv = this.shadowRoot.getElementById("instructions");
            instructionsDiv.classList.remove("hidden");
        
            const intervalId = setInterval(() => {
                if (index < coordinates.length) {
                    const [lat, lng] = coordinates[index];
                    this.travelerMarker.setLatLng([lat, lng]);
        
                    this.map.setView([lat, lng], this.map.getZoom(), { animate: true });


                    if (stepIndex < steps.length && index >= stepIndex) {
                        const step = steps[stepIndex];
                        instructionsDiv.textContent = `Étape ${stepIndex + 1}: ${step.text}`;
                        stepIndex++;
                    }
        
                    index++;
                } else {
                    clearInterval(intervalId);
                    resolve();
                }
            }, this.animationInterval);
        });
    }
    

    displaySteps(steps) {
        const instructionsDiv = this.shadowRoot.getElementById("instructions");

        if(steps.length > 0){
            instructionsDiv.textContent = `Étape 1: ${steps[0].text}`;
            instructionsDiv.classList.remove("hidden");
            instructionsDiv.innerHTML = "";
        } else {
            instructionsDiv.textContent = "Aucune pas de route trouvée";
        }

        console.log(`Distance: ${steps.distance} mètres`);
        console.log(`Durée: ${steps.duration} secondes`);
        steps.forEach((step, index) => {
            instructionsDiv.textContent = `Étape 1: ${steps[0].text}`;
            console.log(`Étape ${index + 1}:`);
            console.log(`Instructions: ${step.text}`);
            console.log('step lat', step.position.lat);
            console.log('step lng', step.position.lng);
        });
    }

    async fetchAddressFromCoordinates(lat, lon) {
        const url = `https://api-adresse.data.gouv.fr/reverse/?lon=${lon}&lat=${lat}`;
        try {
            const response = await fetch(url);
            const data = await response.json();

            if (data.features && data.features.length > 0) {
                const address = data.features[0].properties.label;
                return address;
            } else {
                console.warn("Aucune adresse trouvée pour ces coordonnées.");
                return "Adresse inconnue";
            }
        } catch (error) {
            console.error("Erreur lors de la récupération de l'adresse :", error);
            return "Erreur lors de la récupération de l'adresse";
        }
    }

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


        // Réinitialiser les coordonnées de départ et d'arrivée
        //et contenu dans le front
        this.dispatchEvent(new CustomEvent("reset-search-bars", {
            detail: {},
            bubbles: true,
            composed: true
        }));
    }
    
}

customElements.define("leaflet-map", LeafletMap);
