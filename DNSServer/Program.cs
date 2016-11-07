using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DNSServer
{
    class Program
    {
        static void Main(string[] args)
        {
            LanDNS.DNSServer server = new LanDNS.DNSServer();
            server.Start();
        }
    }
}
