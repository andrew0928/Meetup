using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using NineYi.Msa.Infra.Messaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Demo01_Common;
using NineYi.Msa.Infra;

namespace Demo01_Server
{
    class Program
    {
        static void Main(string[] args)
        {
            var host = new HostBuilder()
                .ConfigureServices((context, services) =>
                {
                    services.AddSingleton<MessageWorkerOptions>(new MessageWorkerOptions()
                    {
                        WorkerThreadsCount = 1,
                        QueueName = "demo-01",
                        ConnectionURL = "amqp://guest:guest@localhost:5672/",
                    });
                    services.AddSingleton<IHostedService, MessageWorker<MyMessage>>();
                    services.AddScoped<TrackContext>((sp) => 
                    {
                        return new TrackContext()
                        {
                            RequestId = $"worker-{Guid.NewGuid():N}"
                        };
                    });
                })
                .Build();


            (host.Services.GetRequiredService<IHostedService>() as MessageWorker<MyMessage>).Process = ((input, cid, scope) =>
            {
                var track = scope.ServiceProvider.GetService<TrackContext>();
                Console.WriteLine($"Process Message: {input.Text}");

                if (track != null)
                {
                    Console.WriteLine($"- Correlation Id: {cid}");
                    Console.WriteLine($"- Track.RequestId: {track.RequestId}");
                }
            });

            using (host)
            {
                host.Start();
                host.WaitForShutdown();
            }
        }
    }





}
