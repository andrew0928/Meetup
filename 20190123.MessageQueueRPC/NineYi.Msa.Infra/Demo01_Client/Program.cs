using Demo01_Common;
using NineYi.Msa.Infra.Messaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Demo01_Client
{
    class Program
    {
        static void Main(string[] args)
        {
            using (MessageClient<MyMessage> client = new MessageClient<MyMessage>(new MessageClientOptions()
            {
                BusType = MessageClientOptions.MessageBusTypeEnum.QUEUE,
                QueueName = "demo-01",
                ConnectionURL = @"amqp://guest:guest@localhost:5672/",
            }, null, null))
            {

                while (true)
                {
                    client.SendMessage("", new MyMessage()
                    {
                        Text = Guid.NewGuid().ToString()
                    });
                    Task.Delay(1000).Wait();
                }
            }
        }
    }


}
