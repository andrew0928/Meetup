using System;
using System.Collections.Generic;
using System.Text;

namespace NineYi.Msa.Infra.Messaging
{

    public class MessageWorkerOptions : MessageOptions
    {


        public string QueueName = "default-queue";
        public bool QueueDurable = true;


        public int WorkerThreadsCount = 5;
        public ushort PrefetchCount = 1;


        public readonly static MessageWorkerOptions Default = new MessageWorkerOptions();
    }

}
