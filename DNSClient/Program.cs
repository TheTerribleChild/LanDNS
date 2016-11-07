using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DNSClient
{
    class Program
    {
        static void Main(string[] args)
        {
            LanDNS.DNSClient client = new LanDNS.DNSClient();
            client.Start();
        }
    }
}
