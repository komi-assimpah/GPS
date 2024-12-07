using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.ServiceModel;
using System.ServiceModel.Description;

namespace RoutingServer
{
    class Program
    {
        static void Main(string[] args)
        {
            Uri baseAddress = new Uri("http://localhost:8733/RoutingServer/");
            using (ServiceHost host = new ServiceHost(typeof(RoutingService), baseAddress))
            {
                try
                {
                    host.Open();
                    Console.WriteLine("Service is running at " + baseAddress);
                    Console.WriteLine("Press <Enter> to terminate the service...");
                    Console.ReadLine();
                }
                catch (Exception ex)
                {
                    Console.WriteLine("An error occurred: " + ex.Message);
                    host.Abort();
                }
            }
        }
    }
}
