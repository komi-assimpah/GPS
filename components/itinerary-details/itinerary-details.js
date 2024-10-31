class ItineraryStepList extends HTMLElement {
    constructor() {
      super();
      this.attachShadow({ mode: "open" });
      this.shadowRoot.appendChild(this.getTemplate().content.cloneNode(true));
    }
  
    getTemplate() {
      const template = document.createElement("template");
      template.innerHTML = `
        <div class="itinerary-steps">
          <div id="steps-container"></div>
          <button class="select-button">Sélectionner</button>
        </div>
        <style>
          .itinerary-steps {
            display: flex;
            flex-direction: column;
            align-items: center;
            padding: 15px;
            border: 1px solid #ddd;
            border-radius: 8px;
            background-color: #fff;
          }
          .step {
            display: flex;
            align-items: center;
            width: 100%;
            padding: 5px 0;
          }
          .icon {
            width: 24px;
            height: 24px;
            margin-right: 10px;
          }
          .details {
            display: flex;
            justify-content: space-between;
            width: 100%;
          }
          .name {
            flex-grow: 1;
            margin-right: 5px;
            color: #333;
          }
          .distance-time {
            text-align: right;
            color: #333;
          }
          .distance-time strong {
            display: block;
            font-size: 1em;
          }
          .separator {
            text-align: center;
            color: #ccc;
            margin: 8px 0;
          }
          .select-button {
            margin-top: 15px;
            padding: 10px 20px;
            background-color: #78B9BA;
            color: #fff;
            font-size: 1em;
            font-weight: bold;
            border: none;
            border-radius: 20px;
            cursor: pointer;
          }
          .select-button:hover {
            background-color: #66a3a3;
          }
        </style>
      `;
      return template;
    }
  
    connectedCallback() {}
  
    updateSteps(steps) {
      const stepsContainer = this.shadowRoot.querySelector("#steps-container");
      stepsContainer.innerHTML = "";
  
      steps.forEach((step, index) => {
        const stepWrapper = document.createElement("div");
        stepWrapper.classList.add("step");
  
        const icon = document.createElement("img");
        icon.src = `../../assets/icons/${step.icon}`;
        icon.classList.add("icon");
  
        const details = document.createElement("div");
        details.classList.add("details");
  
        const name = document.createElement("span");
        name.textContent = step.name;
        name.classList.add("name");
  
        const distanceTime = document.createElement("div");
        distanceTime.classList.add("distance-time");
  
        const distance = document.createElement("strong");
        distance.textContent = step.distance;
  
        const time = document.createElement("strong");
        time.textContent = step.time;
  
        distanceTime.append(distance, document.createElement("br"), time);
        details.append(name, distanceTime);
        stepWrapper.append(icon, details);
        stepsContainer.appendChild(stepWrapper);
  
        if (index < steps.length - 1) {
          const separator = document.createElement("div");
          separator.classList.add("separator");
          separator.textContent = "⋮";
          stepsContainer.appendChild(separator);
        }
      });
    }
  }
  
  customElements.define("itinerary-step-list", ItineraryStepList);
  