using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceHost
{
    class Program
    {
        static void Main(string[] args)
        {
            LanDNS.ServiceHost serviceHost = new LanDNS.ServiceHost();
            serviceHost.Test();

            //for (int i = 0; i < 10000; i ++)
            //    serviceHost.AddService(i.ToString(), new System.Net.IPEndPoint(Utility.WebUtility.GetLocalIPAddress(), 6000));
        }
    }
}
