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

    internal class MessageSYN : Message
    {

        public uint ClientSequence { get; private set; }

        public MessageSYN(uint clientSequence) : base(MessageType.SYN)
        {
            this.ClientSequence = clientSequence;
        }
    }

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

    internal class MessageSYNACK : Message
    {
        public uint SequenceSum { get; private set; }
        
        public MessageSYNACK(uint sequenceSum) : base(MessageType.SYNACK)
        {
            this.SequenceSum = sequenceSum;
            
        }
    }

    internal class MessageAccept : Message
    {
        public uint SequenceSum { get; private set; }
        
        public MessageAccept(uint sequenceSum) : base(MessageType.Accept)
        {
            this.SequenceSum = sequenceSum;
        }
    }

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

    internal class MessageRefresh : Message
    {
        public uint SequenceSum { get; private set; }

        public MessageRefresh(uint sequenceSum) : base(MessageType.Refresh)
        {
            this.SequenceSum = sequenceSum;
        }
    }

    internal class MessageRequest : Message
    {
        public string ServiceName { get; private set; }

        public MessageRequest(string serviceName): base(MessageType.Request)
        {
            this.ServiceName = serviceName;
        }
    }

    internal class MessageReturnRequest : Message
    {
        public IPEndPoint ServiceEndpoint { get; private set; }

        public MessageReturnRequest(IPEndPoint serviceEndpoint) : base(MessageType.ReturnRequest)
        {
            this.ServiceEndpoint = serviceEndpoint;
        }
    }

    internal class MessageGetDNS : Message
    {
        public MessageGetDNS() : base(MessageType.GetDNS)
        {
        }
    }

    internal class MessageReplyDNSInfo : Message
    {
        public IPEndPoint DNSReceiverEP { get; private set; }
        public IPEndPoint DNSReplyEP { get; private set; }

        public MessageReplyDNSInfo(IPEndPoint dnsReceiverEP, IPEndPoint dnsReplyEP) : base(MessageType.ReplyDNSInfo)
        {
            this.DNSReceiverEP = dnsReceiverEP;
            this.DNSReplyEP = dnsReplyEP;
        }
    }
}
