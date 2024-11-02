class LeafletMap extends HTMLElement {
    static get observedAttributes() {
        return ["start", "end"];
    }

    constructor() {
        super();
        this.attachShadow({ mode: "open" });
        this.start = [43.695, 7.267]; // Coordonnées initiales de départ
        this.end = null;

        this.originMarker = null; // Marqueur d'origine unique
        this.destMarker = null; // Marqueur de destination unique
        this.routeControl = null; // Contrôle de routage unique
        this.animationInterval = 30; // Intervalle de mise à jour pour l'animation (en ms)

        this.scriptsLoaded = false; // Ajout d'un indicateur pour vérifier si les scripts sont chargés
    }

    connectedCallback() {
        const template = document.createElement("template");
        template.innerHTML = `
          <link rel="stylesheet" href="../../leaflet-routing-machine-3.2.12/dist/leaflet-routing-machine.css">
          <link rel="stylesheet" href="../../leaflet-1.8.0/leaflet.css">
          <div id="map" style="width: 100%; height: 100vh;"></div>
      `;
        this.shadowRoot.appendChild(template.content.cloneNode(true));

        // Charger les scripts Leaflet de manière asynchrone avant d'initialiser la carte
        this.loadScript("../../leaflet-1.8.0/leaflet.js")
            .then(() =>
                this.loadScript(
                    "../../leaflet-routing-machine-3.2.12/dist/leaflet-routing-machine.js"
                )
            )
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

        // Initialisation de la carte
        //this.map = L.map(mapElement).setView([43.695, 7.267], 5);
        this.map = L.map(mapElement).setView(this.start, 5);

        L.tileLayer("http://{s}.tile.osm.org/{z}/{x}/{y}.png", {
            attribution:
                'Leaflet &copy; <a href="http://openstreetmap.org">OpenStreetMap</a> contributors',
            maxZoom: 18,
        }).addTo(this.map);

        // Configuration de l'icône de taxi
        const taxiIcon = L.icon({
            iconUrl: "../../assets/img/riding.png",
            iconSize: [50, 50],
        });

        // Ajout du marqueur de taxi initial
        this.originMarker = L.marker(this.start, { icon: taxiIcon }).addTo(
            this.map
        );

        // Écoute des clics pour mettre à jour la destination
        this.map.on("click", (event) => {
            const { lat, lng } = event.latlng;
            this.setAttribute("end", JSON.stringify([lat, lng])); // Met à jour l'attribut `end`
        });
    }

    attributeChangedCallback(name, oldValue, newValue) {
        const value = JSON.parse(newValue);
        if (name === "start") {
            this.start = value;
            this.updateRoute();
            console.log("start long ", value[0]);
            console.log("console lag", value[1]);

            this.updateOriginMarker(); // Mettre à jour le marqueur
        } else if (name === "end") {
            this.end = value;
            this.updateRoute();
        }
    }

    updateOriginMarker() {
        if (!this.start || !this.map) return;
        // Si le marqueur existe déjà, mettez à jour sa position
        if (this.originMarker) {
            this.originMarker.setLatLng(L.latLng(...this.start)); // Utilise `originMarker` ici
        } else {
            // Sinon, créez un nouveau marqueur avec l'icône de taxi et ajoutez-le à la carte
            const taxiIcon = L.icon({
                iconUrl: "../../assets/img/riding.png",
                iconSize: [50, 50],
            });
            this.originMarker = L.marker(L.latLng(...this.start), {
                icon: taxiIcon,
            }).addTo(this.map);
        }
    }

    updateRoute() {
        if (!this.start || !this.end || !this.scriptsLoaded) return;

        // Ajoute un nouveau marqueur de destination
        this.destMarker = L.marker(this.end).addTo(this.map);

        // Initialise ou met à jour le contrôle de routage
        if (this.routeControl) {
            this.routeControl.setWaypoints([
                L.latLng(...this.start),
                L.latLng(...this.end),
            ]);
        } else {
            this.routeControl = L.Routing.control({
                waypoints: [L.latLng(...this.start), L.latLng(...this.end)],
                addWaypoints: false,
            })
                .on("routesfound", (routeEvent) => this.animateRoute(routeEvent))
                .addTo(this.map);
        }
    }

    animateRoute(routeEvent) {
        const routes = routeEvent.routes[0].coordinates;
        let index = 0;
  
        if (this.animation) clearInterval(this.animation);
  
        this.animation = setInterval(() => {
            if (index < routes.length) {
                this.originMarker.setLatLng([routes[index].lat, routes[index].lng]);
                index++;
            } else {
                clearInterval(this.animation); // Stop animation once route is completed
            }
        }, this.animationInterval);
    }
}

customElements.define("leaflet-map", LeafletMap);
