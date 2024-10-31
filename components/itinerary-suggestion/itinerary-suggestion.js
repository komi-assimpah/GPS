class ItinerarySuggestion extends HTMLElement {
  constructor() {
    super();
    this.attachShadow({ mode: "open" });
  }

  connectedCallback() {
    const wrapper = document.createElement("div");
    wrapper.classList.add("itinerary-suggestion");

    const iconWrapper = document.createElement("div");
    iconWrapper.classList.add("icon-wrapper");

    // Dossier par défaut pour les icons
    const iconsFolder = "../../assets/icons/";

    const icons = this.getAttribute("icons").split(",");
    icons.forEach((iconSrc, index) => {
      const icon = document.createElement("img");
      icon.src = `${iconsFolder}${iconSrc.trim()}`; // Utilise le dossier par défaut pour charger les icons

      icon.classList.add("icon");
      iconWrapper.appendChild(icon);

      if (index < icons.length - 1) {
        const separator = document.createElement("span");
        separator.textContent = " => ";
        iconWrapper.appendChild(separator);
      }
    });

    const text = document.createElement("span");
    text.textContent = this.getAttribute("text") || "No itinerary info";

    const distance = document.createElement("strong");
    distance.textContent = this.getAttribute("distance") || "0 Km";

    const style = document.createElement("style");
    style.textContent = `
        .itinerary-suggestion {
            display: flex; 
            justify-content: space-between;
            padding: 10px;
            border: 1px solid #ddd;
            border-radius: 5px;
            background-color: #fff;
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
        span {
          color: #333;
        }
        strong {
          font-size: 1.2em;
          color: #333;
        }
      `;

    this.shadowRoot.append(style, wrapper);
    wrapper.append(iconWrapper, text, distance);
  }
}

customElements.define("itinerary-suggestion", ItinerarySuggestion);
