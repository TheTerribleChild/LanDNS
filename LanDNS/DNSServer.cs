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
        private static readonly IPEndPoint DNS_ENDPOINT = new IPEndPoint(IPAddress.Any, 5053);

        private Utility.WebUtility.UdpConnector dnsListener;
        private Utility.WebUtility.UdpConnector dnsResponder;

        public DNSServer()
        {
            this.dnsListener = new Utility.WebUtility.UdpConnector(DNS_ENDPOINT);
            this.dnsListener.MessageReceived += this.ParseDNSMessage;

            this.dnsResponder = new Utility.WebUtility.UdpConnector();
        }

        public void Start()
        {
            MessageReplyDNSInfo dnsInfo;
            int dnsCount = DNSUtility.GetLanDNS(out dnsInfo);
            if (dnsCount > 0)
            {
                Console.WriteLine("One or more DNS already exist in the network");
                return;
            }
            dnsListener.Listen = true;
        }

        public void ParseDNSMessage(object sender, Utility.WebUtility.MessageReceivedEventArgs args)
        {
            Console.WriteLine("Thread ID" + Thread.CurrentThread.ManagedThreadId);
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
                        message = Utility.SerializeUtility.DeserializeJsonString<MessageGetDNS>(args.Message);
                        break;
                    default:
                        break;
                }
                Message returnMessage = new MessageReject(0, "Hello");
                dnsListener.SendMessage(Utility.SerializeUtility.SerializeToJsonString(returnMessage), args.RemoteEndpoint);

            }
            catch(Exception e)
            {
                Message message = new MessageReject(0, "unknown message");
                dnsListener.SendMessage(Utility.SerializeUtility.SerializeToJsonString(message), args.RemoteEndpoint);
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

        }
    }
}
