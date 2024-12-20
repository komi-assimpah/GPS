class SearchBar extends HTMLElement {
    constructor() {
        super();
        this.attachShadow({ mode: "open" });
        this.callTimeout = null;
    }

    connectedCallback() {
        const placeholder = this.getAttribute("placeholder") || "Enter address";
        const template = document.createElement("template");
        template.innerHTML = `
            <style>
                .search-bar { position: relative; }
                input {
                    width: 100%;
                    padding: 10px;
                    border-radius: 8px;
                    border: 1px solid #ccc;
                    outline: none;
                    transition: border-color 0.3s ease;
                }
                input:focus {
                    border-color: #CE472F;
                    border-width: 2px;
                    box-shadow: 0 0 5px rgba(206, 71, 47, 0.5); 
                }
                .suggestions {
                    position: absolute;
                    top: 100%;
                    width: 100%;
                    background: white;
                    border: 1px solid #ccc;
                    max-height: 150px;
                    overflow-y: auto;
                    z-index: 10;
                    display: none; /* Masquer par défaut */
                }
                .suggestion-item {
                    padding: 10px;
                    cursor: pointer;
                }
                .suggestion-item:hover {
                    background-color: #f0f0f0;
                }
            </style>
            <div class="search-bar">
                <input type="text" placeholder="${placeholder}" id="search-input" />
                <div class="suggestions" id="suggestions"></div>
            </div>
        `;
        this.shadowRoot.appendChild(template.content.cloneNode(true));
        this.inputElement = this.shadowRoot.getElementById("search-input");
        this.suggestionsElement = this.shadowRoot.getElementById("suggestions");

        this.inputElement.addEventListener("input", () => this.handleInput());
        this.suggestionsElement.addEventListener("click", (e) => this.selectSuggestion(e));
    }

    handleInput() {
        if (this.callTimeout) clearTimeout(this.callTimeout);
        this.callTimeout = setTimeout(() => {
            const query = this.inputElement.value;
            if (query.length >= 3) {
                this.fetchSuggestions(query);
            } else {
                this.suggestionsElement.style.display = "none"; // Masquer si moins de 3 caractères
            }
        }, 300);
    }

    // Fonction pour récupérer les suggestions
    async fetchSuggestions(query) {
        try {
            const url = `https://api-adresse.data.gouv.fr/search/?q=${encodeURIComponent(query)}&limit=5`;
            const response = await fetch(url);
            const data = await response.json();
            this.renderSuggestions(data.features);
        } catch (error) {
            console.error("Erreur lors de la récupération des suggestions:", error);
        }
    }

    renderSuggestions(suggestions) {
        this.suggestionsElement.innerHTML = "";
        
        if (suggestions.length > 0) {
            this.suggestionsElement.style.display = "block"; // Afficher les suggestions
            suggestions.forEach((suggestion) => {
                const item = document.createElement("div");
                item.textContent = suggestion.properties.label;
                item.classList.add("suggestion-item");
                item.dataset.latitude = suggestion.geometry.coordinates[1];
                item.dataset.longitude = suggestion.geometry.coordinates[0];
                item.dataset.value = suggestion.properties.label;
                this.suggestionsElement.appendChild(item);
            });
        } else {
            this.suggestionsElement.style.display = "none"; // Masquer si aucune suggestion
        }
    }

    selectSuggestion(event) {
        if (event.target.classList.contains("suggestion-item")) {
            this.inputElement.value = event.target.dataset.value;
            this.suggestionsElement.innerHTML = "";
            this.suggestionsElement.style.display = "none"; // Masquer après sélection
            const latitude = parseFloat(event.target.dataset.latitude);
            const longitude = parseFloat(event.target.dataset.longitude);
            this.dispatchEvent(new CustomEvent("address-selected", {
                detail: {
                    address: this.inputElement.value,
                    coordinates: [latitude, longitude]
                },
                bubbles: true
            }));
        }
    }

    updateInputWithCoordinates(coordinateText) {
        this.inputElement.value = coordinateText;
    }
}

customElements.define("search-bar", SearchBar);
