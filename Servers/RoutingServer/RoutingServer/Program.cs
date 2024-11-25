using System;
using System.Globalization;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using Newtonsoft.Json;


namespace RoutingServer
{
    internal class Program
    {

        static readonly HttpClient client = new HttpClient();

        public List<Contract> retrieveContracts()
        {
            string responseBody = callJCDecaux("contracts?").Result;
            List<Contract> contracts = JsonConvert.DeserializeObject<List<Contract>>(responseBody);
            return contracts;
        }

        public List<Station> retrieveStations(string contractName)
        {
            string responseBody = callJCDecaux("stations?contract=" + contractName + "&").Result;
            List<Station> stations = JsonConvert.DeserializeObject<List<Station>>(responseBody);
            return stations;
        }

        public Station getStation(int stationNumber, string contractName)
        {
            List<Station> stations = retrieveStations(contractName);

            foreach (Station station in stations)
            {
                if (station.Number == stationNumber)
                {
                    return station;
                }
            }
            return null;
        }

        public Station getClosestStation(int stationNumber, string contractName)
        {
            double minDistance = double.MaxValue;
            Station choosenStation = getStation(stationNumber, contractName);
            Station closestStation = null;

            foreach (Contract contract in retrieveContracts())
            {
                List<Station> stations = retrieveStations(contract.Name);

                foreach (Station station in stations)
                {
                    double distance = Math.Sqrt(
                        Math.Pow(station.Position.Lat - choosenStation.Position.Lat, 2)
                        + Math.Pow(station.Position.Lng - choosenStation.Position.Lng, 2));
                    if (distance < minDistance && distance != 0)
                    {
                        minDistance = distance;
                        closestStation = station;
                    }
                }
            }

            return closestStation;
        }



        public Dictionary<string, (Station Station, Contract Contract)> computeClosestAvailable(List<Position> locations, List<Contract> contracts)
        {
            double minOriginDistance = double.MaxValue;
            double minDestinationDistance = double.MaxValue;

            var closest = new Dictionary<string, (Station Station, Contract Contract)>
            {
                { "Origin", (null, null) },
                { "Destination", (null, null) }
            };

            foreach (var contract in contracts)
            {
                List<Station> stations = retrieveStations(contract.Name);

                foreach (Station station in stations)
                {
                    Position position = station.Position;

                    if (station.Available_bikes > 0)
                    {
                        double distance = Math.Sqrt(
                            Math.Pow(position.Lat - locations[0].Lat, 2)
                            + Math.Pow(position.Lng - locations[0].Lng, 2)
                        );
                        if (distance < minOriginDistance)
                        {
                            minOriginDistance = distance;
                            closest["Origin"] = (station, contract);
                        }
                    }

                    if (station.Available_bike_stands > 0)
                    {
                        double distance = Math.Sqrt(
                            Math.Pow(position.Lat - locations[1].Lat, 2)
                            + Math.Pow(position.Lng - locations[1].Lng, 2)
                        );
                        if (distance < minDestinationDistance)
                        {
                            minDestinationDistance = distance;
                            closest["Destination"] = (station, contract);
                        }
                    }
                }
            }

            return closest;
        }


        static async Task<string> callJCDecaux(string content)
        {
            string url = "https://api.jcdecaux.com/vls/v1/";
            string key = "apiKey=d6bd03040ecae0e3244f4ba002c10bb9a34d85df";
            try
            {
                HttpResponseMessage response = await client.GetAsync(url + content + key);
                response.EnsureSuccessStatusCode();
                string responseBody = await response.Content.ReadAsStringAsync();
                return responseBody;
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine("\nException Caught!");
                Console.WriteLine($"Error calling JC Decaux : {ex.Message}");
                return string.Empty;
            }
        }

        static async Task<Position> resolveAddress(string address)
        {
            string escapedAddress = Uri.EscapeDataString(address);
            string url = "https://nominatim.openstreetmap.org/search?q=" + escapedAddress + "&format=json";
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


        static void Main(string[] args)
        {
            Program program = new Program();
            Console.WriteLine("Welcome to our GPS Routing Server\n");
            List<string> actions = new List<string> { "Browse Contracts", "Plan a journey", "Exit" };
            program.performAction(actions, "Choose Action", new List<string> { });
        }

        private void performAction(List<string> actions, string action, List<string> target)
        {
            switch (action)
            {
                case "Choose Action":
                    Console.WriteLine("\nChoosing an action...\n");
                    Console.WriteLine("\nWhat next? Here are some suggestions :\n");

                    printOptions(actions);
                    int index = chooseOption(actions.Count, "action");
                    string choosenAction = actions[index];

                    performAction(actions, choosenAction, target);
                    break;


                case "Plan a journey":
                    Console.WriteLine("\nPlanning a journey...\n");

                    Console.Write("\nEnter the origin address :\nAddress : ");
                    string originAddress = Console.ReadLine();
                    Position originPosition = resolveAddress(originAddress).Result;

                    Console.Write("\nEnter the destination address :\nAddress : ");
                    string destinationAddress = Console.ReadLine();
                    Position destinationPosition = resolveAddress(destinationAddress).Result;

                    List<Contract> contracts = retrieveContracts();


                    if (originPosition != null && destinationPosition != null)
                    {
                        List<Position> positions = new List<Position>() { originPosition, destinationPosition };
                        Dictionary<string, (Station Station, Contract Contract)> closest = computeClosestAvailable(positions, contracts);

                        Station originStation = closest["Origin"].Station;
                        Contract originContract = closest["Origin"].Contract;
                        Console.WriteLine($"\nThe station closest to '{originAddress}' is : {originStation.Name}");
                        Console.WriteLine($"Its contract is : {originContract.Name}");


                        Station destinationStation = closest["Destination"].Station;
                        Contract destinationContract = closest["Destination"].Contract;
                        Console.WriteLine($"\nThe station closest to '{destinationAddress}' is : {destinationStation.Name}");
                        Console.WriteLine($"Its contract is : {destinationContract.Name}");
                    }
                    else
                    {
                        Console.WriteLine("\nUnable to resolve addresses.");
                    }

                    actions = new List<string> { "Browse Contracts", "Plan a journey", "Exit" };
                    target = new List<string> { };
                    performAction(actions, "Choose Action", target);
                    break;


                case "Browse Contracts":
                    Console.WriteLine("\nBrowsing contracts ...\n");
                    contracts = retrieveContracts();

                    Console.WriteLine("\nJCDecaux's contracts list :\n");
                    printOptions(contracts.Select(contract => contract.Name).ToList());

                    actions = new List<string> { "Choose Contract", "Plan a journey", "Exit" };
                    target = new List<string> { };
                    performAction(actions, "Choose Action", target);
                    break;


                case "Choose Contract":
                    Console.WriteLine("\nChoosing contracts ...\n");
                    contracts = retrieveContracts();

                    index = chooseOption(contracts.Count, "contract");
                    Contract choosenContract = contracts[index];
                    printObject(choosenContract, "Contract");

                    actions = new List<string> { "Browse Stations", "Browse Contracts", "Plan a journey", "Exit" };
                    target = new List<string> { choosenContract.Name };
                    performAction(actions, "Choose Action", target);
                    break;


                case "Browse Stations":
                    Console.WriteLine("\nBrowsing stations ...\n");
                    List<Station> stations = retrieveStations(target.Last());
                    if (stations.Count != 0)
                    {
                        Console.WriteLine("\nStations of the choosen contract :\n");
                        printOptions(stations.Select(station => station.Name).ToList());

                        actions = new List<string> { "Choose Station", "Browse Contracts", "Exit" };
                        target = new List<string> { target.Last() };
                    }
                    else
                    {
                        Console.WriteLine("\nUnfortunately, the choosen contract doesn't have any station\n");
                        actions = new List<string> { "Browse Contracts", "Plan a journey", "Exit" };
                        target = new List<string> { };
                    }
                    performAction(actions, "Choose Action", target);
                    break;


                case "Choose Station":
                    Console.WriteLine("\nChoosing stations ...\n");
                    stations = retrieveStations(target.Last());

                    index = chooseOption(stations.Count, "station");
                    Station choosenStation = stations[index];
                    printObject(choosenStation, "Station");

                    actions = new List<string> { "Get Closest Station", "Browse Stations", "Browse Contracts", "Plan a journey", "Exit" };
                    target = new List<string> { choosenStation.Number.ToString(), choosenStation.Contract_name };
                    performAction(actions, "Choose Action", target);
                    break;


                case "Get Closest Station":
                    Console.WriteLine("\nGetting closest station ...\n");
                    Station closestStation = getClosestStation(int.Parse(target.First()), target.Last());

                    Console.WriteLine("\nClosest to the choosen station :");
                    printObject(closestStation, "Station");

                    actions = new List<string> { "Browse Stations", "Browse Contracts", "Plan a journey", "Exit" };
                    target = new List<string> { target.Last() };
                    performAction(actions, "Choose Action", target);
                    break;


                case "Exit":
                    Environment.Exit(0);
                    break;

            }

        }

        private Type retrieveType(string typeName)
        {
            Type type = Type.GetType(typeName);
            if (type == null)
            {
                Assembly assembly = Assembly.GetExecutingAssembly();
                type = assembly.GetTypes().FirstOrDefault(t => t.Name == typeName);
            }
            return type;
        }

        private void printOptions(List<string> names)
        {
            int index = 1;
            foreach (string name in names)
            {
                Console.WriteLine(index + ". " + name);
                index++;
            }
        }

        private int chooseOption(int size, string type)
        {
            int choice;
            bool isValidInt;

            Console.Write("\nPlease, choose a" + ((type == "action") ? "n" : "") + " " + type + " :\nNumber : ");
            string input = Console.ReadLine();
            isValidInt = int.TryParse(input, out choice);
            while (!isValidInt || choice < 1 || choice > size)
            {
                Console.Write("\nNot a valid index. Please, retry\nNumber : ");
                input = Console.ReadLine();
                isValidInt = int.TryParse(input, out choice);
            }
            return int.Parse(input) - 1;
        }

        private void printObject(object obj, string typeName)
        {

            switch (typeName)
            {

                case "Contract":
                    Contract contract = (Contract)obj;
                    Console.WriteLine("\nContract Name: " + contract.Name);
                    Console.WriteLine("Commercial Name: " + contract.Commercial_name);
                    Console.Write("Cities:");
                    foreach (string city in contract.Cities)
                    {
                        Console.Write(" " + city);
                    }
                    Console.WriteLine("\nCountry Code: " + contract.Country_code);
                    break;


                case "Station":
                    Station station = (Station)obj;
                    Console.WriteLine("\nStation Number: " + station.Number);
                    Console.WriteLine("Station Name: " + station.Name);
                    Console.WriteLine("Contract Name: " + station.Contract_name);
                    Console.WriteLine("Address: " + station.Address);
                    Console.WriteLine("Position: " + station.Position.Lat + " " + station.Position.Lng);
                    Console.WriteLine("Banking: " + station.Banking);
                    Console.WriteLine("Bonus: " + station.Bonus);
                    Console.WriteLine("Bike Stands: " + station.Bike_stands);
                    Console.WriteLine("Available Bike Stands: " + station.Available_bike_stands);
                    Console.WriteLine("Available Bikes: " + station.Available_bikes);
                    Console.WriteLine("Status: " + station.Status);
                    Console.WriteLine("Last Update: " + station.Last_update);
                    break;

            }

        }

    }

    public class Contract
    {
        public string Name { get; set; }
        public string Commercial_name { get; set; }
        public List<string> Cities { get; set; }
        public string Country_code { get; set; }
    }

    public class Station
    {
        public int Number { get; set; }
        public string Contract_name { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }
        public Position Position { get; set; }
        public bool Banking { get; set; }
        public bool Bonus { get; set; }
        public int Bike_stands { get; set; }
        public int Available_bike_stands { get; set; }
        public int Available_bikes { get; set; }
        public string Status { get; set; }
        public long Last_update { get; set; }

    }

    public class Position
    {
        public double Lat { get; set; }
        public double Lng { get; set; }
    }

}


