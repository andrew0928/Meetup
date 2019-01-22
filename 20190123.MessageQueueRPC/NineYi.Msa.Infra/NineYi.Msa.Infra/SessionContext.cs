using System;
using System.Collections.Generic;
using System.Text;

namespace NineYi.Msa.Infra
{
    [Obsolete]
    public class SessionContext
    {
        public string SessionId { get; set; }

        public string MemberId { get; set; }

        public string ChannelId { get; set; }

        public string SessionToken { get; set; }
    }
}
