using ProxyCache;
using System;
using System.ServiceModel;
using System.ServiceModel.Description;

class Program
{
    static void Main(string[] args)
    {
        // Base address for the service
        Uri baseAddress = new Uri("http://localhost:8733/Design_Time_Addresses/ProxyCache/ProxyService/");

        // Create the ServiceHost instance
        using (ServiceHost host = new ServiceHost(typeof(ProxyService), baseAddress))
        {
            try
            {
                // Add a service endpoint
                host.AddServiceEndpoint(
                    typeof(IProxyService),
                    new BasicHttpBinding
                    {
                        TransferMode = TransferMode.Streamed,
                        MaxReceivedMessageSize = int.MaxValue,
                        ReaderQuotas = new System.Xml.XmlDictionaryReaderQuotas
                        {
                            MaxDepth = int.MaxValue,
                            MaxStringContentLength = int.MaxValue,
                            MaxArrayLength = int.MaxValue,
                            MaxBytesPerRead = int.MaxValue,
                            MaxNameTableCharCount = int.MaxValue
                        }
                    },
                    "");

                // Enable metadata exchange
                ServiceMetadataBehavior smb = new ServiceMetadataBehavior
                {
                    HttpGetEnabled = true
                };
                host.Description.Behaviors.Add(smb);

                // Start the service
                host.Open();
                Console.WriteLine("The service is ready at {0}", baseAddress);
                Console.WriteLine("Press <Enter> to stop the service.");
                Console.ReadLine();

                // Close the service
                host.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine("An error occurred: {0}", ex.Message);
                host.Abort();
            }
        }
    }
}
