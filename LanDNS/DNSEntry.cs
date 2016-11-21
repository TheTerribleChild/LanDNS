using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace LanDNS
{
    internal enum EntryState { Active, Expired, Pending}

    public class DNSEntry
    {
        public IPEndPoint ServiceEP { get; set; }
        public string ServiceName { get; set; }
        internal IPEndPoint DNSReceiverEP { get; set; }
        internal IPEndPoint DNSResponderEP { get; set; }
        internal IPEndPoint RemoteEP { get; set; }
        internal DateTime SessionExpirationTime { get; set; }
        internal DateTime SessionRefreshTime { get; set; }
        internal EntryState State { get; set; }
        internal uint SessionKey { get; set; }
        internal uint SecretKey { get; set; }
    }
}
