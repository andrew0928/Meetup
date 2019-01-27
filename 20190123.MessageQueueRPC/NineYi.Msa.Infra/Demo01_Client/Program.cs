using Demo01_Common;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NineYi.Msa.Infra;
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
            #region setup DI
            var services = new ServiceCollection()
                .AddLogging()
                //.AddSingleton<TrackContext>((service) =>
                //{
                //    return new TrackContext()
                //    {
                //        RequestId = $"request-{Guid.NewGuid():N}",
                //        SessionId = $"session-{DateTime.Now.Ticks}",
                //        ShopId = 9527,
                //        MemberId = "andrew",
                //        ChannelId = $"facebook-{Guid.NewGuid():N}"
                //    };
                //})
                .AddScoped<TrackContext>((service) =>
                {
                    return new TrackContext()
                    {
                        RequestId = $"request-{Guid.NewGuid():N}",
                        SessionId = $"session-demo-00000001",
                        ShopId = 9527,
                        MemberId = "andrew",
                        ChannelId = $"facebook-demo-00000001"
                    };
                })
                .BuildServiceProvider();


            #endregion


            using (MessageClient<MyMessage> client = new MessageClient<MyMessage>(new MessageClientOptions()
            {
                BusType = MessageClientOptions.MessageBusTypeEnum.QUEUE,
                QueueName = "demo-01",
                ConnectionURL = @"amqp://guest:guest@localhost:5672/",
                //}, null, null))
            }, services, null))
            {

                //while (true)
                {
                    using (var scope = services.CreateScope())
                    {
                        var track = services.GetRequiredService<TrackContext>();
                        string message = $"Hello, {track.MemberId}";
                        string cid = client.SendMessage("", new MyMessage()
                        {
                            Text = message
                        });

                        Console.WriteLine($"- message sent (cid: {cid}, request-id: {track.RequestId}, message: {message}).");
                    }
                    Task.Delay(1000).Wait();
                }
            }
        }
    }


}
