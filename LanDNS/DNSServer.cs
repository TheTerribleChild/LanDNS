using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;

namespace LanDNS
{

    public class DNSServer
    {
        private static DNSServer _instance;
        public static DNSServer Instance {
            get {
                if (_instance == null)
                    _instance = new DNSServer();
                return _instance;
            }
        }

        private IPEndPoint DnsReceiverEP;
        private IPEndPoint DnsResponderEP;

        private Utility.WebUtility.UdpConnector dnsListener;
        private Utility.WebUtility.UdpConnector dnsResponder;

        public static readonly int DNS_PORT = 5053;

        private Dictionary<uint, DNSEntryWrapper> entryDictionary;
        private Dictionary<string, DNSEntry> activeServiceDictionary;

        public static int ActiveSessionDuration { get; set; }
        public static int PendingSessionDuration { get; set; }

        private class DNSEntryWrapper : DNSEntry
        {

            private System.Timers.Timer expirationTimer;
            public int SessionInterval { get; private set; }

            public DNSEntryWrapper(IPEndPoint remoteEP, uint sessionKey) : base()
            {
                this.DNSReceiverEP = DNSServer.Instance.DnsReceiverEP;
                this.DNSResponderEP = DNSServer.Instance.DnsResponderEP;
                this.RemoteEP = remoteEP;
                this.SessionKey = sessionKey;
                this.SessionExpirationTime = DateTime.MinValue;
                this.SessionRefreshTime = DateTime.MinValue;
                this.expirationTimer = new System.Timers.Timer();
                this.expirationTimer.Elapsed += (sender, e) => DNSServer.Instance.EntryExpireEvent(sender, e, this);
                this.State = EntryState.Pending;
            }

            public DNSEntry GetSession()
            {
                return (DNSEntry)this;
            }

            public void ActivateSession(IPEndPoint serviceEP, string serviceName, int sessionInterval)
            {
                this.ServiceEP = serviceEP;
                this.ServiceName = serviceName;
                this.SessionInterval = sessionInterval;
                this.expirationTimer.Interval = SessionInterval;
                this.State = EntryState.Active;
                RenewalEntry();
            }

            public void RenewalEntry()
            {

                SessionExpirationTime = DateTime.Now.AddMilliseconds(SessionInterval);
                SessionRefreshTime = DateTime.Now.AddMilliseconds(SessionInterval * 0.75);
                expirationTimer.Interval = SessionInterval;
                expirationTimer.Stop();
                expirationTimer.Start();
                State = EntryState.Active;
            }

        }

        private DNSServer()
        {
            this.entryDictionary = new Dictionary<uint, DNSEntryWrapper>();
            this.activeServiceDictionary = new Dictionary<string, DNSEntry>();
            ActiveSessionDuration = 60000;
            PendingSessionDuration = 10000;
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
            DnsReceiverEP = new IPEndPoint(hostIP, DNS_PORT);
            DnsResponderEP = new IPEndPoint(hostIP, Utility.WebUtility.GetNextAvailableUDPPortNumber());

            dnsResponder = new Utility.WebUtility.UdpConnector(DnsResponderEP);

            dnsListener = new Utility.WebUtility.UdpConnector(DnsReceiverEP);
            dnsListener.MessageReceived += this.ParseIncomeMessage;
            dnsListener.Listen = true;
        }

        private void ParseIncomeMessage(object sender, Utility.WebUtility.MessageReceivedEventArgs args)
        {
            try
            {
                MessageType type = Utility.SerializeUtility.DeserializeJsonString<Message>(args.Message).Type;
                //Console.WriteLine(type + " " + args.RemoteEndpoint + " " + args.LocalEndpoint);

                switch (type)
                {
                    case MessageType.SYN:
                        SynReceived(Utility.SerializeUtility.DeserializeJsonString<MessageSYN>(args.Message), args.RemoteEndpoint);
                        break;
                    case MessageType.ACK:
                        AckReceived(Utility.SerializeUtility.DeserializeJsonString<MessageACK>(args.Message), args.RemoteEndpoint);
                        break;
                    case MessageType.Refresh:
                        RefreshReceived(Utility.SerializeUtility.DeserializeJsonString<MessageRefresh>(args.Message), args.RemoteEndpoint);
                        break;
                    case MessageType.Request:
                        RequestReceived(Utility.SerializeUtility.DeserializeJsonString<MessageRequest>(args.Message), args.RemoteEndpoint);
                        break;
                    case MessageType.GetDNS:
                        GetDNSReceived(Utility.SerializeUtility.DeserializeJsonString<MessageGetDNS>(args.Message), args.RemoteEndpoint);
                        break;
                    default:
                        break;
                }
            }
            catch(Exception)
            {
                return;
            }
        }

        private void SynReceived(MessageSYN message, IPEndPoint remoteEndPoint)
        {
            uint serverSequence = 0;
            uint sequenceSum = GenerateSessionKey(message.ClientSequence, out serverSequence);
            DNSEntryWrapper newSession = new DNSEntryWrapper(remoteEndPoint, sequenceSum);
            MessageSYNACK synackReply = new MessageSYNACK(serverSequence);
            entryDictionary.Add(sequenceSum, newSession);
            dnsResponder.SendMessage(Utility.SerializeUtility.SerializeToJsonString(synackReply), remoteEndPoint);
        }

        private void AckReceived(MessageACK message, IPEndPoint remoteEndPoint)
        {
            MessageReject rejectMessage = null;

            if(!entryDictionary.ContainsKey(message.SequenceSum))
                rejectMessage = new MessageReject(message.SequenceSum, "Unknown sequence sum : " + message.SequenceSum);
            else if(remoteEndPoint != entryDictionary[message.SequenceSum].RemoteEP)
                rejectMessage = new MessageReject(message.SequenceSum, "Endpoint does not match");
            else if (activeServiceDictionary.ContainsKey(message.ServiceName))
                rejectMessage = new MessageReject(message.SequenceSum, "Service name " + message.ServiceName + " already taken");

            if(rejectMessage != null)
            {
                dnsResponder.SendMessage(Utility.SerializeUtility.SerializeToJsonString(rejectMessage), remoteEndPoint);
                return;
            }

            DNSEntryWrapper entry = entryDictionary[message.SequenceSum];
            entry.ActivateSession(message.ServiceEP, message.ServiceName, ActiveSessionDuration);
            activeServiceDictionary.Add(entry.ServiceName, entry);
            MessageAccept acceptMessage = new MessageAccept(entry.GetSession());
            dnsResponder.SendMessage(Utility.SerializeUtility.SerializeToJsonString(acceptMessage), remoteEndPoint);
        }

        private void RefreshReceived(MessageRefresh message, IPEndPoint remoteEndPoint)
        {
            if (!entryDictionary.ContainsKey(message.SequenceSum))
            {
                dnsResponder.SendMessage(Utility.SerializeUtility.SerializeToJsonString(new MessageReject(message.SequenceSum, "Unknown sequence sum : " + message.SequenceSum)), remoteEndPoint);
            }
            entryDictionary[message.SequenceSum].RenewalEntry();
            dnsResponder.SendMessage(Utility.SerializeUtility.SerializeToJsonString(new MessageAccept(entryDictionary[message.SequenceSum])), remoteEndPoint);
        }

        private void RequestReceived(MessageRequest message, IPEndPoint remoteEndPoint)
        {
            if (activeServiceDictionary.ContainsKey(message.ServiceName))
                dnsResponder.SendMessage(Utility.SerializeUtility.SerializeToJsonString(new MessageReturnRequest(activeServiceDictionary[message.ServiceName].ServiceEP, true)), remoteEndPoint);
            else
                dnsResponder.SendMessage(Utility.SerializeUtility.SerializeToJsonString(new MessageReturnRequest(remoteEndPoint, false)), remoteEndPoint);
        }

        private void GetDNSReceived(MessageGetDNS message, IPEndPoint remoteEndPoint)
        {
            dnsResponder.SendMessage(Utility.SerializeUtility.SerializeToJsonString(new MessageReplyDNSInfo(DnsReceiverEP, DnsResponderEP)), remoteEndPoint);
        }

        private uint GenerateSessionKey(uint clientSequence, out uint serverSequence)
        {
            uint potentialKey;

            do
            {
                serverSequence = (uint)new Random().Next();
                potentialKey = clientSequence + serverSequence;
            } while (entryDictionary.ContainsKey(potentialKey));

            return potentialKey;
        }

        private void EntryExpireEvent(Object source, ElapsedEventArgs e, DNSEntryWrapper expiredSession)
        {
            Console.WriteLine("Session expired:");

        }

        private void AddSession()
        {

        }

        private void RemoveSession()
        {

        }
    }
}
