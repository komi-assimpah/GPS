class MapComponent extends HTMLElement {
  constructor() {
    super();
    this.attachShadow({ mode: "open" });
  }


  connectedCallback() {
    const template = document.createElement("template");
    template.innerHTML = `
      <link rel="stylesheet" href="../../leaflet-routing-machine-3.2.12/dist/leaflet-routing-machine.css">
      <link rel="stylesheet" href="../../leaflet/leaflet.css">
      <style>
        #map {
          width: 100%;
          height: 100vh;
        }
      </style>
      <div id="map"></div>
    `;

    this.shadowRoot.appendChild(template.content.cloneNode(true));
  }


}

customElements.define("map-component", MapComponent);