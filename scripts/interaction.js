// Références des éléments
const mapComponent = document.getElementById("mapComponent");
const departureBar = document.getElementById("departure");
const destinationBar = document.getElementById("destination");

// Variables pour stocker les coordonnées
let departureCoordinates = null;
let destinationCoordinates = null;

// Fonction pour afficher le popup
function showPopupMessage(message, color = "#CE472F", duration = 5000) {
    const popup = document.createElement("popup-message");
    popup.setAttribute("text", message);
    popup.setAttribute("color", color);
    popup.setAttribute("duration", duration);
    document.body.appendChild(popup);
}

// Écouteurs pour chaque `search-bar`
departureBar.addEventListener("address-selected", (event) => {
    departureCoordinates = event.detail.coordinates;
    mapComponent.setAttribute("start", JSON.stringify(departureCoordinates));

    // Vérifiez si les coordonnées de destination sont déjà définies
    if (destinationCoordinates && JSON.stringify(departureCoordinates) === JSON.stringify(destinationCoordinates)) {
        showPopupMessage("Le départ et la destination doivent être différents");
    }
});

destinationBar.addEventListener("address-selected", (event) => {
    destinationCoordinates = event.detail.coordinates;
    mapComponent.setAttribute("end", JSON.stringify(destinationCoordinates));

    // Vérifiez si les coordonnées de départ sont déjà définies
    if (departureCoordinates && JSON.stringify(departureCoordinates) === JSON.stringify(destinationCoordinates)) {
        showPopupMessage("Le départ et la destination doivent être différents");
    }
});

// Écoute de l'événement `destination-selected`
mapComponent.addEventListener("destination-selected", (event) => {
    const { address } = event.detail;
    destinationBar.updateInputWithCoordinates(address);
});













let currentDetailComponent = null;
let currentSuggestionComponent = null;

document.getElementById("itineraries").addEventListener("show-details", (event) => {
  const { details, id } = event.detail;
  const suggestionComponent = document.getElementById(id);

  // Vérifie si un autre détail est déjà affiché
  if (currentDetailComponent && currentSuggestionComponent) {
    currentDetailComponent.remove();  // Supprime l'élément `ItineraryDetails` en cours
    currentSuggestionComponent.classList.remove("hidden");  // Restaure la visibilité du `ItinerarySuggestion`
  }

  // Crée et configure le nouveau composant ItineraryDetails
  const detailComponent = document.createElement("itinerary-details");
  detailComponent.updateSteps(details.steps);

  // Cache le composant ItinerarySuggestion original et insère le composant de détails après
  suggestionComponent.classList.add("hidden");
  suggestionComponent.insertAdjacentElement("afterend", detailComponent);

  // Met à jour les références du composant actuel
  currentDetailComponent = detailComponent;
  currentSuggestionComponent = suggestionComponent;
});
