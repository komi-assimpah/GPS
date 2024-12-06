namespace ProxyCache
{
    public static class Utils
    {
        public const string JCDecauxAPIKey = "d6bd03040ecae0e3244f4ba002c10bb9a34d85df";
        public const string JCDecauxBaseURL = "https://api.jcdecaux.com/vls/v1";

        public const string NominatimOSMBaseUrl = "https://nominatim.openstreetmap.org";

        public const string RouterOSRMBaseUrl = "https://router.project-osrm.org/route/v1";

        public const string ORSApiKey = "5b3ce3597851110001cf6248863d8fc1bc55493fa434eea86000ea6e";
        public const string ORSBaseUrl = "https://api.openrouteservice.org/v2/directions";//cycling-regular?api_key=${apiKey}&start=${this.start[1]},${this.start[0]}&end=${this.end[1]},${this.end[0]};


        public const int batchSize = 100;
    }
}
