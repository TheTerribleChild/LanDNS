using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace LanDNS
{
    public class DNSClient
    {
        public static readonly IPEndPoint DNS_ENDPOINT = new IPEndPoint(IPAddress.Any, 5053);
        private Utility.WebUtility.UdpConnector udpConnector;

        public DNSClient()
        {
            udpConnector = new Utility.WebUtility.UdpConnector();
            udpConnector.MessageReceived += MessageReceived;
        }

        public void Start()
        {
            udpConnector.Listen = true;
            Message msg = new MessageSYN(10);
            udpConnector.SendMessage(Utility.SerializeUtility.SerializeToJsonString(msg), new IPEndPoint(IPAddress.Broadcast, 5053));
        }

        private void MessageReceived(object sender, Utility.WebUtility.MessageReceivedEventArgs args)
        {
            Console.WriteLine(args.RemoteEndpoint + " " + args.LocalEndpoint + " " + args.Message);
        }
    }
}
