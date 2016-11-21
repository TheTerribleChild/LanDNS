using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace LanDNS
{
    public class ServiceHost
    {
        private Utility.WebUtility.UdpConnector connector;

        private IPAddress localIpAddress;
        private IPEndPoint dnsListenerEP;
        private IPEndPoint dnsResponderEP;

        private System.Timers.Timer refreshTimer;

        public Dictionary<string, DNSEntry> Entries { get; private set; }

        public ServiceHost()
        {
            this.connector = new Utility.WebUtility.UdpConnector();
            this.localIpAddress = Utility.WebUtility.GetLocalIPAddress();

            MessageReplyDNSInfo dnsReply;
            int dnsCount = DNSUtility.GetLanDNS(out dnsReply);

            dnsListenerEP = dnsReply.DNSListenerEP;
            dnsResponderEP = dnsReply.DNSResponderEP;
        }

        ~ServiceHost()
        {

        }

        public bool AddService(string serviceName, IPEndPoint serviceEP)
        {
            uint clientSequence = (uint)new Random().Next();

            MessageSYN syn = new MessageSYN(clientSequence);
            MessageSYNACK synack = null;
            MessageACK ack = null;
            MessageAccept accept = null;
            MessageReject reject = null;

            bool synced = false;

            for(int retrySend = 0; retrySend < 3; retrySend++)
            {
                connector.SendMessage(Utility.SerializeUtility.SerializeToJsonString(syn), dnsListenerEP);

                while(true)
                {
                    IPEndPoint remoteEP;
                    string message = connector.GetMessage(out remoteEP, 2000);
                    if (message == null)
                        break;
                    try
                    {
                        if (remoteEP.Equals(dnsResponderEP))
                        {
                            synack = Utility.SerializeUtility.DeserializeJsonString<MessageSYNACK>(message);
                            synced = true;
                            break;
                        }
                    }
                    catch (Exception) { } 
                }
                if (synced)
                    break;
            }

            if (!synced)
                return false;

            ack = new MessageACK(clientSequence + synack.ServerSequence, serviceName, serviceEP);

            for (int retrySend = 0; retrySend < 3; retrySend++)
            {
                connector.SendMessage(Utility.SerializeUtility.SerializeToJsonString(ack), dnsListenerEP);
                while (true)
                {
                    IPEndPoint remoteEP;
                    string message = connector.GetMessage(out remoteEP, 2000);
                    if (message == null)
                        break;
                    try
                    {
                        if (remoteEP.Equals(dnsResponderEP))
                        {
                            Message messageType = Utility.SerializeUtility.DeserializeJsonString<Message>(message);
                            if(messageType.Type == MessageType.Accept)
                            {
                                accept = Utility.SerializeUtility.DeserializeJsonString<MessageAccept>(message);
                            }
                            else if(messageType.Type == MessageType.Reject)
                            {
                                reject = Utility.SerializeUtility.DeserializeJsonString<MessageReject>(message);
                            }
                            break;
                        }
                    }
                    catch (Exception) { }
                }
                if (accept != null || reject != null)
                    break;
            }

            if(accept != null)
            {
                //Console.WriteLine(accept.ToString());
            }else if(reject != null)
            {
                //Console.WriteLine(reject.Reason);

                return false;
            }

            return true;
        }

        public void Test()
        {
            uint r = (uint)new Random().Next();
            for (int i = 0; i < 500000; i++)
            {
                MessageSYN syn = new MessageSYN((uint)i*r);
                connector.SendMessage(Utility.SerializeUtility.SerializeToJsonString(syn), dnsListenerEP);
            }
        }
    }
}
