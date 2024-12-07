using System;
using System.ServiceModel;
using System.ServiceModel.Description;

namespace RoutingServer
{
    public class Program
    {
        static void Main(string[] args)
        {
            // Define the base address for the service
            Uri baseAddress = new Uri("http://localhost:8733/Design_Time_Addresses/RoutingServer/Service1");

            // Create the ServiceHost instance
            using (ServiceHost host = new ServiceHost(typeof(RoutingService), baseAddress))
            {
                try
                {
                    // Add a service endpoint
                    host.AddServiceEndpoint(
                        typeof(IRoutingService),
                        new BasicHttpBinding(), // Use BasicHttpBinding for SOAP compatibility
                        "");

                    // Enable metadata exchange (for WSDL retrieval)
                    ServiceMetadataBehavior smb = new ServiceMetadataBehavior
                    {
                        HttpGetEnabled = true,  // Enable HTTP metadata retrieval
                        HttpsGetEnabled = true // Set to true if using HTTPS
                    };
                    host.Description.Behaviors.Add(smb);

                    // Add a MEX (Metadata Exchange) endpoint
                    host.AddServiceEndpoint(
                        typeof(IMetadataExchange),
                        MetadataExchangeBindings.CreateMexHttpBinding(),
                        "mex");

                    // Start the service
                    host.Open();
                    Console.WriteLine("Service is running at: " + baseAddress);
                    Console.WriteLine("Metadata is available at: " + baseAddress + "mex");
                    Console.WriteLine("Press Enter to stop the service...");
                    Console.ReadLine();
                }
                catch (Exception ex)
                {
                    Console.WriteLine("An error occurred: " + ex.Message);
                }
            }
        }
    }
}
