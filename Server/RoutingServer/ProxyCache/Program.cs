using System;
using System.ServiceModel;
using System.ServiceModel.Description;

namespace ProxyCache
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                // URL de base définie
                string baseAddress = "http://localhost:8733/Design_Time_Addresses/ProxyCache/ProxyService/";
                Uri baseAddr = new Uri(baseAddress);

                // Création du ServiceHost avec l'URL de base
                ServiceHost host = new ServiceHost(typeof(ProxyService), baseAddr);

                // Configuration du binding
                var binding = new BasicHttpBinding
                {
                    TransferMode = TransferMode.Streamed,
                    MaxReceivedMessageSize = 2147483647,
                    ReaderQuotas =
                    {
                        MaxDepth = 2147483647,
                        MaxStringContentLength = 2147483647,
                        MaxArrayLength = 2147483647,
                        MaxBytesPerRead = 2147483647,
                        MaxNameTableCharCount = 2147483647
                    }
                };

                // Ajout de l'endpoint principal
                host.AddServiceEndpoint(
                    typeof(IProxyService),
                    binding,
                    "" // URL relative vide car l'URL de base est déjà définie
                );

                // Configuration des metadata
                ServiceMetadataBehavior metaBehavior = new ServiceMetadataBehavior
                {
                    HttpGetEnabled = true,
                    HttpsGetEnabled = true
                };
                host.Description.Behaviors.Add(metaBehavior);

                // Ajout de l'endpoint metadata
                host.AddServiceEndpoint(
                    typeof(IMetadataExchange),
                    MetadataExchangeBindings.CreateMexHttpBinding(),
                    "mex"
                );

                // Configuration du debug
                ServiceDebugBehavior debugBehavior = host.Description.Behaviors.Find<ServiceDebugBehavior>();
                if (debugBehavior == null)
                {
                    debugBehavior = new ServiceDebugBehavior();
                    host.Description.Behaviors.Add(debugBehavior);
                }
                debugBehavior.IncludeExceptionDetailInFaults = true;

                // Démarrage du service
                host.Open();

                Console.WriteLine($"ProxyCache Service est démarré à: {baseAddress}");
                Console.WriteLine("Appuyez sur une touche pour arrêter le service...");
                Console.ReadKey();

                // Fermeture propre du service
                host.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Une erreur est survenue: {ex.Message}");
                Console.WriteLine(ex.StackTrace); // Ajout de la stack trace pour plus de détails
                Console.ReadKey();
            }
        }
    }
}