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
