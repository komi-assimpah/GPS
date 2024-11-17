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
        this.animationInterval = 30; // Intervalle de mise à jour pour l'animation (en ms)

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
        if (!this.scriptsLoaded) return; // Assurez-vous que les scripts sont chargés

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

        // Écoute des clics pour mettre à jour la destination
        this.map.on("click", async (event) => {
            const { lat, lng } = event.latlng;
            console.log("click long", lng);
            console.log("click lat", lat);
            this.setAttribute("end", JSON.stringify([lat, lng])); // Met à jour l'attribut `end`

            const address = await this.fetchAddressFromCoordinates(lat, lng);
            this.setAttribute("end", JSON.stringify(address)); // Met à jour l'attribut `end`

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
        if (!this.start && !this.end){
            console.log("erreur");
            return;
        }
        const apiKey = "5b3ce3597851110001cf6248863d8fc1bc55493fa434eea86000ea6e"; // Remplacez par votre clé OpenRouteService
        const url = `https://api.openrouteservice.org/v2/directions/cycling-regular?api_key=${apiKey}&start=${this.start[1]},${this.start[0]}&end=${this.end[1]},${this.end[0]}`;

        fetch(url)
            .then((response) => {
                if (!response.ok) {
                    throw new Error(`Erreur HTTP : ${response.status}`);
                }
                return response.json();
            })
            .then((data) => {
                if (!data.features || data.features.length === 0) {
                    throw new Error("Aucun itinéraire trouvé.");
                }
                console.log("data : ", data);


                const route = data.features[0];
                console.log("Données de l'itinéraire récupérées :", route);

                // Publiez l'itinéraire dans ActiveMQ
                this.sendItineraryToQueue(route);

                // this.displayRoute(route.geometry.coordinates);
                // this.animateRoute(route.geometry.coordinates);

            })
            .catch((error) => {
                console.error("Erreur lors de la récupération de l'itinéraire :", error);
            });
    }

    displayRoute(coordinates) {
        console.log("displayRoute");
        const latLngs = coordinates.map((coord) => L.latLng(coord[1], coord[0]));
    
        // Supprime l'itinéraire précédent, s'il existe
        if (this.routeLayer) {
            this.map.removeLayer(this.routeLayer);
        }
    
        // Ajoute l'itinéraire à la carte
        this.routeLayer = L.polyline(latLngs, { color: "blue" }).addTo(this.map);
    
        // Ajuste la vue de la carte pour inclure tout l'itinéraire
        this.map.fitBounds(this.routeLayer.getBounds());
    }

    
    animateRoute(coordinates) {
        const cyclistIcon = L.icon({
            iconUrl: "../../assets/icons/cycling.png", // Icône pour le cycliste
            iconSize: [40, 40],
        });

        if (this.cyclistMarker) {
            this.map.removeLayer(this.cyclistMarker);
        }
        this.cyclistMarker = L.marker(this.start, { icon: cyclistIcon }).addTo(this.map);
        
        let index = 0;
        if (this.animation) clearInterval(this.animation);
        this.animation = setInterval(() => {
            if (index < coordinates.length) {
                const [lng, lat] = coordinates[index];
                this.cyclistMarker.setLatLng([lat, lng]);
                index++;
            } else {
                clearInterval(this.animation); // Stop animation once route is completed
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
    
            client.subscribe('/queue/itinerary', (message) => {
                if (message.body) {
                    const itinerary = JSON.parse(message.body);
                    console.log('Itinéraire reçu depuis ActiveMQ:', itinerary);
    
                    // Appeler les méthodes de classe avec `this`
                    this.displayRoute(itinerary.geometry.coordinates);
                    this.animateRoute(itinerary.geometry.coordinates);
                }
            });
        };
    
        client.onStompError = (frame) => {
            console.error('Broker error:', frame.headers['message']);
            console.error('Details:', frame.body);
        };
    
        client.activate();
    }
    
    
    
















    
}




customElements.define("leaflet-map", LeafletMap);
