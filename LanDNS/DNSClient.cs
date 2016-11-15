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
            udpConnector.MessageReceived += ParseIncomeMessage;
        }

        public void Start()
        {
            udpConnector.Listen = true;
            Message msg = new MessageSYN(10);
            udpConnector.SendMessage(Utility.SerializeUtility.SerializeToJsonString(msg), new IPEndPoint(IPAddress.Broadcast, 5053));
        }

        private void ParseIncomeMessage(object sender, Utility.WebUtility.MessageReceivedEventArgs args)
        {
            Console.WriteLine(args.RemoteEndpoint + " " + args.LocalEndpoint + " " + args.Message);
            try
            {
                MessageType type = Utility.SerializeUtility.DeserializeJsonString<Message>(args.Message).Type;
                Message message;

                switch (type)
                {
                    case MessageType.SYNACK:
                        message = Utility.SerializeUtility.DeserializeJsonString<MessageSYN>(args.Message);
                        break;
                    case MessageType.Accept:
                        message = Utility.SerializeUtility.DeserializeJsonString<MessageACK>(args.Message);
                        break;
                    case MessageType.Reject:
                        message = Utility.SerializeUtility.DeserializeJsonString<MessageRefresh>(args.Message);
                        break;
                    case MessageType.ReplyDNSInfo:
                        message = Utility.SerializeUtility.DeserializeJsonString<MessageRequest>(args.Message);
                        break;
                    case MessageType.ReturnRequest:
                        break;
                    default:
                        break;
                }
            }
            catch (Exception e)
            {
                return;
            }
        }
    }
}
