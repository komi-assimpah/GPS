using System;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.ServiceModel.Web;

namespace RoutingServer
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Console.WriteLine("Démarrage du service RoutingServer...");
            ServiceHost host = null;

            try
            {
                Utils.AddCorsHeaders();
                // Configuration du client ProxyService
                var proxyBinding = new BasicHttpBinding
                {
                    MaxReceivedMessageSize = 2147483647,
                    ReaderQuotas = new System.Xml.XmlDictionaryReaderQuotas
                    {
                        MaxDepth = 32,
                        MaxStringContentLength = 2147483647,
                        MaxArrayLength = 2147483647,
                        MaxBytesPerRead = 4096,
                        MaxNameTableCharCount = 16384
                    }
                };

                var proxyAddress = new EndpointAddress("http://localhost:8733/Design_Time_Addresses/ProxyCache/ProxyService/");
                ChannelFactory<ProxyServiceReference.IProxyService> factory =
                    new ChannelFactory<ProxyServiceReference.IProxyService>(proxyBinding, proxyAddress);

                // Configuration du RoutingService
                var baseAddress = new Uri("http://localhost:8733/Design_Time_Addresses/RoutingServer/Service1/");
                host = new ServiceHost(typeof(RoutingService), baseAddress);

                // Configuration du endpoint REST
                var webBinding = new WebHttpBinding
                {
                    MaxReceivedMessageSize = 2147483647,
                    MaxBufferSize = 2147483647
                };

                var endpoint = host.AddServiceEndpoint(typeof(IRoutingService), webBinding, "");
                endpoint.Behaviors.Add(new WebHttpBehavior
                {
                    AutomaticFormatSelectionEnabled = false,
                    DefaultOutgoingResponseFormat = WebMessageFormat.Json
                });



                // Configuration des comportements
                var serviceBehaviors = host.Description.Behaviors;

                var debug = serviceBehaviors.Find<ServiceDebugBehavior>();
                if (debug == null)
                {
                    debug = new ServiceDebugBehavior();
                    serviceBehaviors.Add(debug);
                }
                debug.IncludeExceptionDetailInFaults = true;

                var metadata = serviceBehaviors.Find<ServiceMetadataBehavior>();
                if (metadata == null)
                {
                    metadata = new ServiceMetadataBehavior { HttpGetEnabled = true };
                    serviceBehaviors.Add(metadata);
                }

                // Démarrage du service
                host.Open();

                Console.WriteLine("Le service est démarré à l'adresse : " + baseAddress);
                Console.WriteLine("Endpoint REST disponible à : " + baseAddress + "suggestJourney");
                Console.WriteLine("Appuyez sur Entrée pour arrêter le service...");
                Console.ReadLine();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Une erreur s'est produite : {ex.Message}");
                Console.WriteLine($"StackTrace : {ex.StackTrace}");
                Console.WriteLine("Appuyez sur Entrée pour fermer...");
                Console.ReadLine();
            }
            finally
            {
                if (host != null)
                {
                    try
                    {
                        host.Close();
                        Console.WriteLine("Service arrêté.");
                    }
                    catch
                    {
                        host.Abort();
                    }
                }
            }
        }
    }
}