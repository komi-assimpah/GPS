class PopupMessage extends HTMLElement {
  constructor() {
    super();
  }

  connectedCallback() {
    this.attachShadow({ mode: "open" });

    const wrapper = document.createElement("div");
    wrapper.classList.add("popup");
    wrapper.textContent = this.getAttribute("text") || "Default message";

    const bgColor = this.getAttribute("color") || "#333";
    wrapper.style.backgroundColor = bgColor;

    const duration = this.getAttribute("duration") || 3000;

    const style = document.createElement("style");
    style.textContent = `
        .popup {
          position: fixed;
          // bottom: 20px;
          // right: 20px;
          padding: 10px 20px;
          color: white;
          border-radius: 5px;
          opacity: 0;
          animation: fadein 0.5s forwards, fadeout 0.5s ${duration}ms forwards;
        }
        @keyframes fadein {
          from { opacity: 0; }
          to { opacity: 1; }
        }
        @keyframes fadeout {
          from { opacity: 1; }
          to { opacity: 0; }
        }
      `;

    this.shadowRoot.append(style, wrapper);

    setTimeout(() => {
      this.remove();
    }, parseInt(duration) + 1000);
  }
}

customElements.define("popup-message", PopupMessage);
