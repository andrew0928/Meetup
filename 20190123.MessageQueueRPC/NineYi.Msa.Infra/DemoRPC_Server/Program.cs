//#define WINDOWS

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NineYi.Msa.Infra;
using NineYi.Msa.Infra.Messaging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DemoRPC_Server
{
    class Program
    {
        static void Main(string[] args)
        {
            int pid = Process.GetCurrentProcess().Id;
            int worker_count = 5;

            Random rnd = new Random();

            Console.WriteLine("".PadRight(80, '-'));
            Console.WriteLine($"* MQRPC-Server(threads: {worker_count}), PID: {pid}");
            Console.WriteLine("".PadRight(80, '-'));

            var host = new HostBuilder()
                .ConfigureServices((context, services) =>
                {
                    services.Add(new ServiceDescriptor(
                        typeof(IHostedService),
                        (sp) =>
                        {
                            return new MessageWorker<DemoInputMessage, DemoOutputMessage>(
                                new MessageWorkerOptions()
                                {
                                    ConnectionName = "demo-server",
                                    WorkerThreadsCount = 5,
                                    PrefetchCount = 1,
                                    QueueName = "demo",
                                    ConnectionURL = args[0], //"amqp://guest:guest@localhost:5672/",
                                }, null, sp)
                            {
                                Process = (input, cid, scope) =>
                                {
                                    Console.WriteLine($"- [T:{Thread.CurrentThread.ManagedThreadId:000}] start: {input.MessageBody}");
                                    Task.Delay(1000 + rnd.Next(4000)).Wait();
                                    Console.WriteLine($"- [T:{Thread.CurrentThread.ManagedThreadId:000}] end:   {input.MessageBody}");
                                    return new DemoOutputMessage()
                                    {
                                        ReturnCode = 200,
                                        ReturnBody = $"echo: [S:{pid}]/{input.MessageBody}",
                                    };
                                },
                            };
                        },
                        ServiceLifetime.Singleton));

                    services.AddSingleton<InfraContext>(new InfraContext()
                    {
                        ApplicationName = context.HostingEnvironment.ApplicationName,
                        EnvironmentName = context.HostingEnvironment.EnvironmentName,
                        HostGroup = "G1",
                        HostId = "ANDREW-PC",
                        Market = "tw",
                        Region = "dc-01"
                    });
                    services.AddScoped<TrackContext>();
                })
                .ConfigureLogging((context, logger) =>
                {
                })
                .Build();

            using (host)
            {
                host.WinStart();

                host.WaitForWinShutdown();

//#if (WINDOWS)
//                SetConsoleCtrlHandler(ShutdownHandler, true);
//#endif

//                host.Start();

//#if (WINDOWS)
//                close.WaitOne();
//                host.StopAsync(CancellationToken.None).Wait();
//                close_ack.Set();
//                SetConsoleCtrlHandler(ShutdownHandler, false);
//#else
//                host.WaitForShutdown();
//#endif

            }
        }

//#if (WINDOWS)
//        private static ManualResetEvent close = new ManualResetEvent(false);
//        private static ManualResetEvent close_ack = new ManualResetEvent(false);

//        [DllImport("Kernel32")]
//        static extern bool SetConsoleCtrlHandler(EventHandler handler, bool add);

//        delegate bool EventHandler(CtrlType sig);
//        enum CtrlType
//        {
//            CTRL_C_EVENT = 0,
//            CTRL_BREAK_EVENT = 1,
//            CTRL_CLOSE_EVENT = 2,
//            CTRL_LOGOFF_EVENT = 5,
//            CTRL_SHUTDOWN_EVENT = 6
//        }
//        private static bool ShutdownHandler(CtrlType sig)
//        {
//            close.Set();
//            Console.WriteLine($"EVENT: ShutdownHandler({sig}) - RECEIVED");
//            close_ack.WaitOne();
//            Console.WriteLine($"EVENT: ShutdownHandler({sig}) - DONE");
//            return true;
//        }
//#endif


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
