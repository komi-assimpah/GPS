using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using ProxyCache.Models;

namespace ProxyCache.OpenStreetMap
{
    internal class OpenRouteServiceREST
    {
        static readonly HttpClient client = new HttpClient();

        public static async Task<Itinerary> GetItinerary(Position startPosition, Position endPosition, string profile = "foot-walking")
        {
            string url = string.Format(
                CultureInfo.InvariantCulture,
                "https://api.openrouteservice.org/v2/directions/{0}?api_key={1}&start={2},{3}&end={4},{5}&steps=true&overview=full&language=fr",
                profile, Utils.OpenRouteServiceAPIKey, startPosition.Lng, startPosition.Lat, endPosition.Lng, endPosition.Lat
            );
            //string url = $"https://api.openrouteservice.org/v2/directions/{profile}?api_key={Utils.OpenRouteServiceAPIKey}&start={startPosition.Lng},{startPosition.Lat}&end={endPosition.Lng},{endPosition.Lat}&steps=true&overview=full&language=fr";

            try
            {
                HttpResponseMessage response = await client.GetAsync(url);
                response.EnsureSuccessStatusCode();
                string responseBody = await response.Content.ReadAsStringAsync();

                var result = JsonConvert.DeserializeObject<dynamic>(responseBody);
                if (result == null || result.error != null)
                {
                    Console.WriteLine("\nRoute not found");
                    return new Itinerary();
                }

                double distance = (double)result.features[0].properties.segments[0].distance;
                double duration = (double)result.features[0].properties.segments[0].duration;

                var stepsJson = result.features[0].properties.segments[0].steps;
                List<Step> steps = JsonConvert.DeserializeObject<List<Step>>(stepsJson.ToString());

                var coordinatesJson = result.features[0].geometry.coordinates;
                List<List<double>> coordinatesList = coordinatesJson.ToObject<List<List<double>>>();
                List<Position> coordinates = new List<Position>();
                foreach (var coordinate in coordinatesList)
                    coordinates.Add(new Position
                    {
                        Lng = coordinate[0],
                        Lat = coordinate[1]
                    });

                List<Instruction> instructions = DecodeInstructions(steps, coordinates);

                Itinerary itinerary = new Itinerary
                {
                    Distance = distance,
                    Duration = duration,
                    Instructions = instructions
                };

                return itinerary;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\nError computing itinerary\n{ex.Message}");
                return new Itinerary();
            }
        }

        private static List<Instruction> DecodeInstructions(List<Step> steps, List<Position> geometryCoordinates)
        {
            List<Instruction> instructions = new List<Instruction>();

            foreach (var step in steps)
            {
                string move = (string)step.Instruction;
                string name = (string)step.Name ?? "Unknown road";
                double distance = (double)step.Distance;

                string instruction = $"{move} on {name} for {distance} meters";

                int waypointIndex = (int)step.WayPoints[0];
                double lng = geometryCoordinates[waypointIndex].Lng;
                double lat = geometryCoordinates[waypointIndex].Lat;

                Position position = new Position
                {
                    Lat = lat,
                    Lng = lng
                };

                instructions.Add(
                    new Instruction { Text = instruction, Position = position }
                );
            }

            return instructions;
        }

    }

}
