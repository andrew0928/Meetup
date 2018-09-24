using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MQRPC.Core
{
    public class MessageBase
    {
        //public string TestMessage { get; set; }
        //public string requestId { get; set; }
        //public DateTime requestStartUtcTime { get; set; }
        //public string correlationId { get; set; }
    }

    public class DemoInputMessage : MessageBase
    {
        public string MessageBody { get; set; }
    }

    public class DemoOutputMessage : MessageBase
    {
        public int ReturnCode { get; set; }
        public string ReturnBody { get; set; }
    }
}
