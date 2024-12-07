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
    }
}
