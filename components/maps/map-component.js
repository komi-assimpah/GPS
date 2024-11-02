class MapComponent extends HTMLElement {
    constructor() {
      super();
      this.attachShadow({ mode: "open" });
    }
  
    connectedCallback() {
      const template = document.createElement("template");
      template.innerHTML = `
        <style>
          #map {
            width: 100%;
            height: 500px; /* Adjust the height as needed */
            border-radius: 8px;
          }
        </style>
        <div id="map"></div>
      `;
      
      this.shadowRoot.appendChild(template.content.cloneNode(true));
  
      // Initialize the map
      this.initializeMap();
    }
  
    initializeMap() {
      // Set up the map
      this.map = L.map(this.shadowRoot.getElementById("map")).setView([43.6163, 7.0715], 13); // Center on a sample location
  
      // Add a tile layer (e.g., OpenStreetMap)
      L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
        attribution: '© OpenStreetMap contributors'
      }).addTo(this.map);
  
      // Example of a marker at the center
      this.addMarker([43.6163, 7.0715], "Starting Point");
    }
  
    // Add a marker to the map
    addMarker(coords, message) {
      L.marker(coords).addTo(this.map).bindPopup(message).openPopup();
    }
  
    // Plot route on the map using a series of coordinates
    plotRoute(routeCoords) {
      if (this.routeLayer) this.routeLayer.remove(); // Remove any existing route
      this.routeLayer = L.polyline(routeCoords, { color: 'blue' }).addTo(this.map);
      this.map.fitBounds(this.routeLayer.getBounds()); // Zoom to fit the route
    }
  }
  
  customElements.define("map-component", MapComponent);
  
