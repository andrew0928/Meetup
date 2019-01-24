using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using NineYi.Msa.Infra.Messaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Demo01_Common;

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
                        WorkerThreadsCount = 5,
                        QueueName = "demo-01",
                        ConnectionURL = "amqp://guest:guest@localhost:5672/",
                    });
                    //services.AddSingleton<NineYi.Msa.Infra.Messaging.MessageKeeper>();
                    services.AddSingleton<IHostedService, MessageWorker<MyMessage>>();
                })
                .Build();


            (host.Services.GetRequiredService<IHostedService>() as MessageWorker<MyMessage>).Process = ((input, cid, scope) =>
            {
                Console.WriteLine($"Hello, {input.Text}");
            });

            using (host)
            {
                host.Start();
                host.WaitForShutdown();
            }
        }
    }





}
