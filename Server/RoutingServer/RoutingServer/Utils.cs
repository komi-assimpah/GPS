using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel.Web;
using System.Text;
using System.Threading.Tasks;

namespace RoutingServer
{
    internal class Utils
    {
        public static void AddCorsHeaders()
        {
            var context = WebOperationContext.Current;
            if (context != null)
            {
                var headers = context.OutgoingResponse.Headers;

                if (headers["Access-Control-Allow-Origin"] == null)
                {
                    //context.OutgoingResponse.Headers.Add("Access-Control-Allow-Origin", "http://127.0.0.1:5501"); // Remplacez par l'origine de votre application front-end

                    context.OutgoingResponse.Headers.Add("Access-Control-Allow-Origin", "*");
                    context.OutgoingResponse.Headers.Add("Access-Control-Allow-Methods", "GET, POST, PUT, DELETE, OPTIONS");
                    context.OutgoingResponse.Headers.Add("Access-Control-Allow-Headers", "Content-Type, Accept");
                }
            }
        }
    }
}
