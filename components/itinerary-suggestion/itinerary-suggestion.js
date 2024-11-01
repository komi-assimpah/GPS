class ItinerarySuggestion extends HTMLElement {
  constructor() {
    super();
    this.attachShadow({ mode: "open" });
  }

  connectedCallback() {
    const template = document.createElement("template");
    template.innerHTML = `
      <div class="itinerary-suggestion">
        <div class="icon-wrapper"></div>
        <span class="text">${this.getAttribute("text") || "No itinerary info"}</span>
        <div class="distance-time">
          <strong>${this.getAttribute("distance") || "0 Km"}</strong>
          <strong>${this.getAttribute("time") || "0 Min"}</strong>
        </div>
      </div>

      <style>
        .itinerary-suggestion {
          display: flex;
          justify-content: space-between;
          padding: 10px;
          border: 1px solid #ddd;
          border-radius: 5px;
          background-color: #fff;
          cursor: pointer;
        }
        .icon-wrapper {
          display: flex;
          align-items: center;
        }
        .icon {
          width: 24px;
          height: 24px;
          margin-right: 5px;
        }
        .text {
          color: #333;
        }
        .distance-time {
          text-align: right;
          color: #333;
        }
        .distance-time strong {
          font-size: 1.2em;
          display: block;
        }
      </style>
    `;
    this.shadowRoot.appendChild(template.content.cloneNode(true));

    const iconWrapper = this.shadowRoot.querySelector(".icon-wrapper");
    const iconsFolder = "../../assets/icons/";
    const icons = this.getAttribute("icons").split(",");
    icons.forEach((iconSrc, index) => {
      const icon = document.createElement("img");
      icon.src = `${iconsFolder}${iconSrc.trim()}`;
      icon.classList.add("icon");
      iconWrapper.appendChild(icon);

      if (index < icons.length - 1) {
        const separator = document.createElement("span");
        separator.textContent = " -> ";
        iconWrapper.appendChild(separator);
      }
    });

    // Ajoute un écouteur de clic pour déclencher l'événement personnalisé avec l'ID du composant
    this.addEventListener("click", () => {
      const details = {
        steps: [
          { icon: "walker.png", name: "Sophia Antipolis", distance: "1.0 Km", time: "13 Min" },
          { icon: "cycling.png", name: "Saint-Philippe", distance: "6.9 Km", time: "31 Min" },
          { icon: "walker.png", name: "Chaudon", distance: "1.0 Km", time: "15 Min" },
          { icon: "location.png", name: "Musée Picasso", distance: "", time: "" }
        ]
      };
      const id = this.getAttribute("id");
      this.dispatchEvent(new CustomEvent("show-details", { detail: { details, id }, bubbles: true }));
    });
  }
}

customElements.define("itinerary-suggestion", ItinerarySuggestion);
