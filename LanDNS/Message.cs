using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace LanDNS
{

    internal enum MessageType { SYN, ACK, SYNACK, Accept, Reject, Refresh, Request, ReturnRequest, GetDNS, ReplyDNSInfo }

    internal class Message
    {
        public MessageType Type { get; private set; }

        public Message(MessageType type)
        {
            this.Type = type;
        }
    }

    //First step of 3 way handshake with DNS Server.
    internal class MessageSYN : Message
    {
        public uint ClientSequence { get; private set; }

        public MessageSYN(uint clientSequence) : base(MessageType.SYN)
        {
            this.ClientSequence = clientSequence;
        }
    }

    //Third step of 3 way handshake with DNS Server.
    internal class MessageACK : Message
    {
        public uint ServerSequence { get; private set; }
        public string ServiceName { get; private set; }
        public IPEndPoint ServiceIP { get; private set; }

        public MessageACK(uint serverSequence, string serviceName) : base(MessageType.ACK)
        {
            this.ServerSequence = serverSequence;
            this.ServiceName = serviceName;
        }
    }

    //Second step of 3 way handshake with DNS Server.
    internal class MessageSYNACK : Message
    {
        public uint SequenceSum { get; private set; }
        
        public MessageSYNACK(uint sequenceSum) : base(MessageType.SYNACK)
        {
            this.SequenceSum = sequenceSum;
            
        }
    }

    //Accept client's request to host IP for service.
    internal class MessageAccept : Message
    {
        public uint SequenceSum { get; private set; }
        public DNSSession Session { get; private set; }
        
        public MessageAccept(uint sequenceSum, DNSSession session) : base(MessageType.Accept)
        {
            this.SequenceSum = sequenceSum;
            this.Session = session;
        }
    }

    //Reject the client's request to host IP for service.
    internal class MessageReject : Message
    {
        public uint SequenceSum { get; private set; }
        public string Reason { get; private set; }

        public MessageReject(uint sequenceSum, string reason) : base(MessageType.Reject)
        {
            this.SequenceSum = sequenceSum;
            this.Reason = reason;
        }
    }

    //Request to refresh service.
    internal class MessageRefresh : Message
    {
        public uint SequenceSum { get; private set; }

        public MessageRefresh(uint sequenceSum) : base(MessageType.Refresh)
        {
            this.SequenceSum = sequenceSum;
        }
    }

    //Request DNS for IP for service.
    internal class MessageRequest : Message
    {
        public string ServiceName { get; private set; }
        public uint SecretKey { get; private set; }

        public MessageRequest(string serviceName, uint secretKey = 0): base(MessageType.Request)
        {
            this.ServiceName = serviceName;
            this.SecretKey = secretKey;
        }
    }

    //Return a client's request for service.
    internal class MessageReturnRequest : Message
    {
        public IPEndPoint ServiceEndpoint { get; private set; }

        public MessageReturnRequest(IPEndPoint serviceEndpoint) : base(MessageType.ReturnRequest)
        {
            this.ServiceEndpoint = serviceEndpoint;
        }
    }

    //Broadcast message to request for DNS Info.
    internal class MessageGetDNS : Message
    {
        public MessageGetDNS() : base(MessageType.GetDNS)
        {
        }
    }

    //Reply to MessageGetDNS with DNS info.
    internal class MessageReplyDNSInfo : Message
    {
        public IPEndPoint DNSListenerEP { get; private set; }
        public IPEndPoint DNSResponderEP { get; private set; }

        public MessageReplyDNSInfo(IPEndPoint dnsListenerEP, IPEndPoint dnsResponderEP) : base(MessageType.ReplyDNSInfo)
        {
            this.DNSListenerEP = dnsListenerEP;
            this.DNSResponderEP = dnsResponderEP;
        }
    }
}
