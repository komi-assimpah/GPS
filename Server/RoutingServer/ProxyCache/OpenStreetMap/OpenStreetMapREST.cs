using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ProxyCache.Models;

namespace ProxyCache.OpenStreetMap
{
    internal class OpenStreetMapREST
    {
        static readonly HttpClient client = new HttpClient();

        //returns the position of the given address
        public static async Task<Position> ResolveAddress(string address)
        {
            string escapedAddress = Uri.EscapeDataString(address);
            string url = $"{Utils.osmNominatimBaseUrl}/search?q=" + escapedAddress + "&format=json";
            try
            {

                client.DefaultRequestHeaders.UserAgent.ParseAdd("HTTP_Client/1.0 (sagesseadabadji@gmail.com)");
                HttpResponseMessage response = await client.GetAsync(url);
                response.EnsureSuccessStatusCode();
                string responseBody = await response.Content.ReadAsStringAsync();
                var results = JsonConvert.DeserializeObject<List<dynamic>>(responseBody);

                if (results == null || results.Count == 0)
                {
                    Console.WriteLine("No results found for the given address.");
                    return null;
                }

                var firstResult = results[0];
                if (firstResult.lat != null && firstResult.lon != null)
                {
                    double lat = double.Parse(firstResult.lat.ToString(), CultureInfo.InvariantCulture);
                    double lon = double.Parse(firstResult.lon.ToString(), CultureInfo.InvariantCulture);
                    return new Position { Lat = lat, Lng = lon };
                }
                else
                {
                    Console.WriteLine("Latitude or longitude not found in the result.");
                    return null;
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine("\nException Caught!");
                Console.WriteLine($"Error resolving address: {ex.Message}");
                return null;
            }
        }


        //return the nearest city of the given position
        public static async Task<City> GetNearestCity(Position position)
        {
            string apiUrl = $"{Utils.osmNominatimBaseUrl}/reverse?format=json&lat={DoubleToString(position.Lat)}&lon={DoubleToString(position.Lng)}";

            HttpResponseMessage response = await client.GetAsync(apiUrl);

            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine("Latitude or longitude not found in the result.");
                return null;
            }

            string jsonResult = await response.Content.ReadAsStringAsync();
            JObject jsonObject = JObject.Parse(jsonResult);
            string locationCity = jsonObject["address"]["town"]?.ToString() ?? jsonObject["address"]["city"]?.ToString() ?? jsonObject["address"]["district"]?.ToString() ?? jsonObject["address"]["municipality"]?.ToString();

            if (locationCity == null)
            {
                Console.WriteLine("City not found");
                return null;
            }
            return new City { Name = locationCity?.Split(' ')[0].ToLower() };
        }


        //get itineraries between two positions
        //const url = `https://api.openrouteservice.org/v2/directions/cycling-regular?api_key=${apiKey}&start=${this.start[1]},${this.start[0]}&end=${this.end[1]},${this.end[0]}`;
        public static async Task<Itinerary> GetItinerary(Position start, Position end)
        {
            string url = $"{Utils.orsBaseUrlCycling}?api_key={Utils.orsAPIKey}&start={start.Lat},{start.Lng}&end={end.Lat},{end.Lng}";
            HttpResponseMessage response = await client.GetAsync(url);
            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine("Error getting itinerary");
                return null;
            }
            string jsonResult = await response.Content.ReadAsStringAsync();

            //TO TEST IS BUGGY
            Console.WriteLine(jsonResult);


            Itinerary itinerary = JsonConvert.DeserializeObject<Itinerary>(jsonResult);
            return itinerary;
        }



 




        private static string DoubleToString(double coordinate)
        {
            return coordinate.ToString(CultureInfo.InvariantCulture);
        }
    }
}
