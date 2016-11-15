using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace LanDNS
{
    internal enum SessionState { Active, Exprired, Pending}

    internal class DNSSession
    {
        internal IPEndPoint DNSReceiverEP { get; set; }
        internal IPEndPoint DNSResponderEP { get; set; }
        internal IPEndPoint RemoteEP { get; set; }
        internal IPEndPoint ServiceEP { get; set; }
        internal DateTime SessionExpirationTime { get; set; }
        internal DateTime SessionRefreshTime { get; set; }
        internal uint SessionKey { get; set; }
        internal uint SecretKey { get; set; }
    }
}
