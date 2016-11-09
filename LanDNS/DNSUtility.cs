using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace LanDNS
{
    internal static class DNSUtility
    {
        internal static readonly int DNS_PORT = 5053;

        internal static int GetLanDNS(out MessageReplyDNSInfo dnsReply)
        {
            UdpClient client = new UdpClient();
            client.Client.ReceiveTimeout = 5000;

            MessageGetDNS getDNSMessage = new MessageGetDNS();
            byte[] data = Encoding.UTF8.GetBytes(Utility.SerializeUtility.SerializeToJsonString(getDNSMessage));

            IPEndPoint broadcastEP = new IPEndPoint(IPAddress.Broadcast, DNS_PORT);
            IPEndPoint dnsEP = new IPEndPoint(IPAddress.Any, 0);

            int dnsCount = 0;

            client.Send(data, data.Length, broadcastEP);

            while (true)
            {
                byte[] incomingData = client.Receive(ref dnsEP);
                try
                {
                    dnsReply = Utility.SerializeUtility.DeserializeJsonString<MessageReplyDNSInfo>(Encoding.UTF8.GetString(incomingData));
                    dnsCount++;
                    if (dnsCount > 1)
                        break;
                }
                catch (Exception) { }
            }

            client.Close();

            return dnsCount;
        }
    }
}
