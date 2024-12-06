class LeafletMap extends HTMLElement {
    static get observedAttributes() {
        return ["start", "end"];
    }

    constructor() {
        super();
        this.attachShadow({ mode: "open" });
        this.start = [43.69676878749648, 7.2787016118204555]; // Coordonnées initiales de départ
        this.end = null;

        this.originMarker = null; // Marqueur d'origine unique
        this.destMarker = null; // Marqueur de destination unique
        this.cyclistMarker = null; // Marqueur de cycliste unique
        this.animationInterval = 300; // Intervalle de mise à jour pour l'animation (en ms)

        this.scriptsLoaded = false; // Ajout d'un indicateur pour vérifier si les scripts sont chargés
    }

    connectedCallback() {
        const template = document.createElement("template");
        template.innerHTML = `
          <link rel="stylesheet" href="../../leaflet-1.8.0/leaflet.css">
          <div id="map" style="width: 100%; height: 100vh;"></div>
      `;
        this.shadowRoot.appendChild(template.content.cloneNode(true));

        // Charger les scripts Leaflet de manière asynchrone avant d'initialiser la carte
        this.loadScript("../../leaflet-1.8.0/leaflet.js")
            .then(() => {
                this.scriptsLoaded = true;
                this.loadMap();
            })
            .catch((error) =>
                console.error("Échec du chargement des scripts Leaflet:", error)
            );
    }

    loadScript(src) {
        return new Promise((resolve, reject) => {
            const script = document.createElement("script");
            script.src = src;
            script.onload = () => resolve();
            script.onerror = () =>
                reject(new Error(`Échec du chargement du script: ${src}`));
            this.shadowRoot.appendChild(script);
        });
    }

    loadMap() {
        if (!this.scriptsLoaded) return;

        const mapElement = this.shadowRoot.getElementById("map");

        this.map = L.map(mapElement).setView(this.start, 16);

        L.tileLayer("http://{s}.tile.osm.org/{z}/{x}/{y}.png", {
            attribution:
                'Leaflet &copy; <a href="http://openstreetmap.org">OpenStreetMap</a> contributors',
            maxZoom: 18,
        }).addTo(this.map);

        this.addStartMarker(); // Ajoute le marqueur de départ

        // Abonnez-vous à la queue ActiveMQ
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
            this.setAttribute("end", JSON.stringify(address));

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
        }
    }

    addStartMarker() {
        const startIcon = L.icon({
            iconUrl: "../../assets/icons/pin.png", // Icône pour le départ
            iconSize: [40, 40],
        });

        if (this.originMarker) {
            this.map.removeLayer(this.originMarker);
        }
        this.originMarker = L.marker(this.start, { icon: startIcon }).addTo(this.map);
    }

    addEndMarker() {
        const endIcon = L.icon({
            iconUrl: "../../assets/icons/location-pin.png", // Icône pour la destination
            iconSize: [40, 40],
        });

        if (this.destMarker) {
            this.map.removeLayer(this.destMarker);
        }
        this.destMarker = L.marker(this.end, { icon: endIcon }).addTo(this.map);
    }

    updateOriginMarker() {
        if (!this.start || !this.map) return;
        // Si le marqueur existe déjà, mettez à jour sa position
        if (this.originMarker) {
            this.originMarker.setLatLng(L.latLng(...this.start)); // Utilise `originMarker` ici
        } else {
            // Sinon, créez un nouveau marqueur avec l'icône de taxi et ajoutez-le à la carte
            this.addStartMarker();
        }
    }

    updateRoute() {
        if (!this.start || !this.end){
            console.error("Erreur : les coordonnées de départ ou d'arrivée sont manquantes.");
            return;
        }
        const orsApiKey = "5b3ce3597851110001cf6248863d8fc1bc55493fa434eea86000ea6e";
        const url = `http://localhost:8733/Design_Time_Addresses/RoutingServer/Service1/suggestJourney?startLat=,${this.start[0]}&startLng=${this.start[1]}&endLat=${this.end[0]}&endLng=${this.end[1]}`;

        console.log("this.start", this.start);
        console.log("this.end", this.end);
        

        fetch(url)
            .then((response) => {
                if (!response.ok) {
                    throw new Error(`Erreur HTTP : ${response.status}`);
                    console.log("erreur pooo");
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
    
                // Publiez l'itinéraire dans ActiveMQ
                //this.sendItineraryToQueue(route);
    
                if (this.destMarker) {
                    this.destMarker.addTo(this.map);
                }
            })
            .catch((error) => {
                console.error("Erreur lors de la récupération de l'itinéraire :", error);
            });
    }

    displayRoute(coordinates) {
        console.log("displayRoute");
        const latLngs = coordinates.map((coord) => L.latLng(coord[0], coord[1]));
    
        // Supprime l'itinéraire précédent, s'il existe
        if (this.routeLayer) {
            this.map.removeLayer(this.routeLayer);
        }
    
        // Ajoute l'itinéraire à la carte
        this.routeLayer = L.polyline(latLngs, { color: "blue" }).addTo(this.map);
    
        // Ajuste la vue de la carte pour inclure tout l'itinéraire
        this.map.fitBounds(this.routeLayer.getBounds());
    }



    animateRoute(coordinates, steps) {
        const cyclistIcon = L.icon({
            iconUrl: "../../assets/icons/cycling.png", // Icône pour le cycliste
            iconSize: [40, 40],
        });
    
        // Supprimez l'ancien marqueur, si nécessaire
        if (this.cyclistMarker) {
            this.map.removeLayer(this.cyclistMarker);
        }
    
        // Créez un nouveau marqueur pour le cycliste
        this.cyclistMarker = L.marker(this.start, { icon: cyclistIcon }).addTo(this.map);
    
        let index = 0; // Index des coordonnées
        let stepIndex = 0;
    
        if (this.animation) clearInterval(this.animation);
    
        this.animation = setInterval(() => {
            if (index < coordinates.length) {
                const [lat, lng] = coordinates[index];
                this.cyclistMarker.setLatLng([lat, lng]);
    
                // Vérifiez si on entre dans une nouvelle étape
                if (stepIndex < steps.length && index >= stepIndex) {
                    console.log(`Étape ${stepIndex + 1}:`);
                    console.log(`Instructions: ${steps[stepIndex].text}`);
                    console.log('step lat', steps[stepIndex].position.lat);
                    console.log('step lng', steps[stepIndex].position.lng);
                    stepIndex++;
                }
    
                index++;
            } else {
                clearInterval(this.animation); // Arrête l'animation une fois terminée
            }
        }, this.animationInterval);
    }
        

    // Méthode pour obtenir une adresse à partir de coordonnées (latitude, longitude)
    async fetchAddressFromCoordinates(lat, lon) {
        const url = `https://api-adresse.data.gouv.fr/reverse/?lon=${lon}&lat=${lat}`;

        try {
            const response = await fetch(url);
            const data = await response.json();

            if (data.features && data.features.length > 0) {
                // Récupère l'adresse textuelle
                const address = data.features[0].properties.label;
                // console.log("Adresse trouvée:", address);
                return address; // Renvoie l'adresse
            } else {
                console.warn("Aucune adresse trouvée pour ces coordonnées.");
                return "Adresse inconnue";
            }
        } catch (error) {
            console.error("Erreur lors de la récupération de l'adresse :", error);
            return "Erreur lors de la récupération de l'adresse";
        }
    }

    sendItineraryToQueue(data) {
        const client = new StompJs.Client({
            brokerURL: 'ws://localhost:61614',
            connectHeaders: { login: 'user', passcode: 'password' },
            debug: function (str) {
                console.log('[STOMP Debug]', str);
            },
            reconnectDelay: 5000,
        });
    
        client.onConnect = function (frame) {
            //console.log('[STOMP Connected]', frame);
            console.log('STOMP Connecté à ActiveMQ:');
            try {
                client.publish({
                    destination: '/queue/itinerary',
                    body: JSON.stringify(data),
                });
                console.log('[STOMP Publish] Message envoyé:', data);
            } catch (error) {
                console.error('[STOMP Publish Error]', error);
            }
        };
    
        client.onStompError = function (frame) {
            console.error('[STOMP Error]', frame.headers['message'], frame.body);
        };
    
        client.activate();
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
    
        // Utilisez une fonction fléchée pour conserver le contexte de `this`
        client.onConnect = (frame) => {
            console.log('Connected to ActiveMQ:', frame);
    

            client.subscribe('/queue/ItinerarySuggested', (message) => {
                if (message.body) {
                    try {
                        const data = JSON.parse(message.body);
                        console.log('[ActiveMQ] Message received:', data);

                        // Parcourir les clés du message pour extraire les itinéraires 
                        // (ex: walking, cycling, driving, etc.)
                        for (const key in data) {
                            if (data.hasOwnProperty(key)) {
                                const itinerary = data[key]; // Itinéraire (ex: walking, cycling)
                                console.log(`[ActiveMQ] Processing itinerary for '${key}':`, itinerary);

                                // Vérifiez si l'itinéraire contient des instructions valides
                                if (itinerary.instructions && itinerary.instructions.length > 0) {
                                    this.handleReceivedItinerary(itinerary);
                                } else {
                                    console.warn(`[Itinerary] No valid instructions found for '${key}'.`);
                                }
                            }
                        }
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

    handleReceivedItinerary(itinerary) {
        if (!itinerary || !itinerary.instructions || itinerary.instructions.length === 0) {
            console.warn('[Itinerary] No steps found in the received itinerary.');
            return;
        }
    
        console.log('[Itinerary] Instructions received:', itinerary.instructions);
    
        // Afficher les étapes dans la console
        this.displaySteps(itinerary.instructions);
    
        // Extraire les coordonnées pour tracer la route
        const coordinates = itinerary.instructions.map(step => [step.position.lat, step.position.lng]);
    
        if (coordinates.length > 0) {
            this.displayRoute(coordinates); // Affiche la route sur la carte
            this.animateRoute(coordinates, itinerary.instructions); // Anime le parcours
        } else {
            console.warn('[Itinerary] No coordinates found in the received instructions.');
        }
    }
    
    
        



    displaySteps(steps) {
        console.log(`Distance: ${steps.distance} mètres`);
        console.log(`Durée: ${steps.duration} secondes`);
        steps.forEach((step, index) => {
            console.log(`Étape ${index + 1}:`);
            console.log(`Instructions: ${step.text}`);
            console.log('step lat', step.position.lat);
            console.log('step lng', step.position.lng);

        });
    }
    
}




customElements.define("leaflet-map", LeafletMap);
