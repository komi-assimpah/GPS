using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using ProxyCache.Models;

namespace ProxyCache.OpenStreetMap
{
    internal class OpenStreetMapREST
    {
        static readonly HttpClient client = new HttpClient();

        public static async Task<Position> ResolveAddress(string address)
        {
            string escapedAddress = Uri.EscapeDataString(address);
            string url = $"{Utils.NominatimOSMBaseUrl}/search?q={escapedAddress}&format=json";

            try
            {
                client.DefaultRequestHeaders.UserAgent.ParseAdd("HTTP_Client/1.0 (sagesseadabadji@gmail.com)");
                HttpResponseMessage response = await client.GetAsync(url);
                response.EnsureSuccessStatusCode();
                string responseBody = await response.Content.ReadAsStringAsync();

                var results = JsonConvert.DeserializeObject<List<dynamic>>(responseBody);
                if (results == null || results.Count == 0)
                {
                    Console.WriteLine("\nNo results found for address");
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
                    Console.WriteLine("\nLatitude or longitude not found");
                    return null;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\nError resolving address\n{ex.Message}");
                return null;
            }
        }

        public static async Task<Itinerary> GetItinerary(Position startPosition, Position endPosition, string profile = "walking")
        {
            string url = string.Format(
                CultureInfo.InvariantCulture,
                "{0}/{1}/{2},{3};{4},{5}?steps=true&overview=full",
                Utils.RouterOSRMBaseUrl, profile, startPosition.Lng, startPosition.Lat, endPosition.Lng, endPosition.Lat
            );

            try
            {
                HttpResponseMessage response = await client.GetAsync(url);
                response.EnsureSuccessStatusCode();
                string responseBody = await response.Content.ReadAsStringAsync();

                var result = JsonConvert.DeserializeObject<dynamic>(responseBody);
                if (result == null || result.routes.Count == 0)
                {
                    Console.WriteLine("\nRoute not found");
                    return new Itinerary();
                }

                double distance = (double)result.routes[0].distance;
                double duration = (double)result.routes[0].duration;

                List<Step> steps = JsonConvert.DeserializeObject<List<Step>>(result.routes[0].legs[0].steps.ToString());

                List<Instruction> instructions = DecodeInstructions(steps);

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

        private static List<Instruction> DecodeInstructions(List<Step> steps)
        {
            List<Instruction> instructions = new List<Instruction>();
            foreach (var step in steps)
            {
                string move = (string)step.Maneuver.Type + " " + (string)step.Maneuver.Modifier;
                string name = (string)step.Name;
                string distance = ((double)step.Distance).ToString();

                string instruction = move + (name != "" || name == null ? " on " : "") + name + " for " + distance + " meters";

                Position position = new Position
                {
                    Lat = step.Intersections[0].Location[1],
                    Lng = step.Intersections[0].Location[0]
                };

                instructions.Add(
                    new Instruction { Text = instruction, Position = position }
                );
            }

            return instructions;
        }
    }
}
