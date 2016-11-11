using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace LanDNS
{
    internal class Session
    {
        public Utility.WebUtility.UdpConnector Connector { get; private set; }

    }

    public class DNSServer
    {
        private IPEndPoint DNS_LISTENER_EP;
        private IPEndPoint DNS_RESPONDER_EP;

        private Utility.WebUtility.UdpConnector dnsListener;
        private Utility.WebUtility.UdpConnector dnsResponder;

        public DNSServer()
        {
            
        }

        public void Start()
        {
            
            Console.WriteLine("Checking for other DNS in network....");

            MessageReplyDNSInfo dnsInfo;
            int dnsCount = DNSUtility.GetLanDNS(out dnsInfo);
            if (dnsCount > 0)
            {
                Console.WriteLine("One or more DNS already exist in the network");
                return;
            }

            Console.WriteLine("No DNS detected. Starting DNS");

            IPAddress hostIP = Utility.WebUtility.GetLocalIPAddress();
            DNS_LISTENER_EP = new IPEndPoint(hostIP, 5053);
            DNS_RESPONDER_EP = new IPEndPoint(hostIP, Utility.WebUtility.GetNextAvailableUDPPortNumber());

            dnsListener = new Utility.WebUtility.UdpConnector(DNS_LISTENER_EP);
            dnsListener.MessageReceived += this.ParseDNSMessage;

            dnsResponder = new Utility.WebUtility.UdpConnector(DNS_RESPONDER_EP);

            dnsListener.Listen = true;
        }

        public void ParseDNSMessage(object sender, Utility.WebUtility.MessageReceivedEventArgs args)
        {
            try
            {
                MessageType type = Utility.SerializeUtility.DeserializeJsonString<Message>(args.Message).Type;
                Console.WriteLine(type + " " + args.RemoteEndpoint + " " + args.LocalEndpoint);
                Message message;

                switch (type)
                {
                    case MessageType.SYN:
                        message = Utility.SerializeUtility.DeserializeJsonString<MessageSYN>(args.Message);
                        break;
                    case MessageType.ACK:
                        message = Utility.SerializeUtility.DeserializeJsonString<MessageACK>(args.Message);
                        break;
                    case MessageType.Refresh:
                        message = Utility.SerializeUtility.DeserializeJsonString<MessageRefresh>(args.Message);
                        break;
                    case MessageType.Request:
                        message = Utility.SerializeUtility.DeserializeJsonString<MessageRequest>(args.Message);
                        break;
                    case MessageType.GetDNS:
                        GetDNSReceived(Utility.SerializeUtility.DeserializeJsonString<MessageGetDNS>(args.Message), args.RemoteEndpoint);
                        break;
                    default:
                        break;
                }
            }
            catch(Exception e)
            {
                return;
            }
        }

        private void SynReceived(MessageSYN message, IPEndPoint remoteEndPoint)
        {

        }

        private void AckReceived(MessageSYN message, IPEndPoint remoteEndPoint)
        {

        }

        private void RefreshReceived(MessageSYN message, IPEndPoint remoteEndPoint)
        {

        }

        private void RequestReceived(MessageSYN message, IPEndPoint remoteEndPoint)
        {

        }

        private void GetDNSReceived(MessageGetDNS message, IPEndPoint remoteEndPoint)
        {
            dnsResponder.SendMessage(Utility.SerializeUtility.SerializeToJsonString(new MessageReplyDNSInfo(DNS_LISTENER_EP, DNS_RESPONDER_EP)), remoteEndPoint);
        }
    }
}
