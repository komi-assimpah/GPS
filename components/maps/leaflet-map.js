class LeafletMap extends HTMLElement {
  static get observedAttributes() {
      return ['start', 'end'];
  }

  constructor() {
      super();
      this.attachShadow({ mode: "open" });

      this.start = [43.695, 7.267]; // Coordonnées initiales de départ
      this.end = null; // Coordonnées initiales de destination

      this.originMarker = null; // Marqueur d'origine unique
      this.destMarker = null; // Marqueur de destination unique

      this.routeControl = null; // Contrôle de routage unique
      this.animationInterval = 30; // Intervalle de mise à jour pour l'animation (en ms)

  }

  connectedCallback() {
      const template = document.createElement("template");
      template.innerHTML = `
          <link rel="stylesheet" href="../../leaflet-routing-machine-3.2.12/dist/leaflet-routing-machine.css">
          <link rel="stylesheet" href="../../leaflet-1.8.0/leaflet.css">
          <style>
              #map {
                  width: 100%;
                  height: 100vh;
              }
          </style>
          <div id="map"></div>
      `;
      this.shadowRoot.appendChild(template.content.cloneNode(true));

      // Chargement dynamique des scripts JavaScript
      this.loadScript("../../leaflet-1.8.0/leaflet.js")
          .then(() => this.loadScript("../../leaflet-routing-machine-3.2.12/dist/leaflet-routing-machine.js"))
          .then(() => this.initializeMap())
          .catch((error) => console.error("Échec du chargement des scripts Leaflet:", error));
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

  initializeMap() {
      const mapElement = this.shadowRoot.getElementById("map");

      // Initialisation de la carte
      this.map = L.map(mapElement).setView(this.start, 5);
      L.tileLayer('http://{s}.tile.osm.org/{z}/{x}/{y}.png', {
          attribution: 'Leaflet &copy; <a href="http://openstreetmap.org">OpenStreetMap</a> contributors',
          maxZoom: 18
      }).addTo(this.map);

      // Configuration de l'icône de taxi
      const taxiIcon = L.icon({
          iconUrl: '../../assets/img/riding.png',
          iconSize: [50, 50]
      });

      // Ajout du marqueur de taxi initial
      this.marker = L.marker(this.start, { icon: taxiIcon }).addTo(this.map);

      // Écoute des clics sur la carte pour définir la destination
      this.map.on('click', (dest) => this.handleMapClick(dest));
  }

  attributeChangedCallback(name, oldValue, newValue) {
      if (name === 'start') {
          this.start = JSON.parse(newValue); // Expects JSON string like "[lat, lng]"
          this.updateRoute();
      } else if (name === 'end') {
          this.end = JSON.parse(newValue);
          this.updateRoute();
      }
  }

  handleMapClick(dest) {
      this.setAttribute("end", JSON.stringify([dest.latlng.lat, dest.latlng.lng]));
  }

  updateRoute() {
      // Supprime le marqueur de destination actuel s'il existe
      if (this.destMarker) {
          this.map.removeLayer(this.destMarker);
      }

      if (!this.end) return; // Si aucune destination n'est définie, arrêtez

      // Ajoute un nouveau marqueur de destination
      this.destMarker = L.marker(this.end).addTo(this.map);

      // Initialise ou met à jour le contrôle de routage
      if (this.routeControl) {
          this.routeControl.setWaypoints([
              L.latLng(...this.start),
              L.latLng(...this.end)
          ]);
      } else {
          this.routeControl = L.Routing.control({
              waypoints: [
                  L.latLng(...this.start),
                  L.latLng(...this.end)
              ],
              addWaypoints: false
          }).on('routesfound', (routeEvent) => this.animateRoute(routeEvent)).addTo(this.map);
      }
  }

  animateRoute(routeEvent) {
      const routes = routeEvent.routes[0].coordinates;
      let index = 0;

      if (this.animation) clearInterval(this.animation);

      this.animation = setInterval(() => {
          if (index < routes.length) {
              this.marker.setLatLng([routes[index].lat, routes[index].lng]);
              index++;
          } else {
              clearInterval(this.animation); // Stop animation once route is completed
          }
      }, this.animationInterval);
  }
}

customElements.define("leaflet-map", LeafletMap);
