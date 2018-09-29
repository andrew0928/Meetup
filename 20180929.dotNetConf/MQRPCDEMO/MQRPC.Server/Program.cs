using MQRPC.Core;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MQRPC.Server
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

            using (DemoRpcServer democ = new DemoRpcServer()
            {
                Process = (input, cid) =>
                {
                    Console.WriteLine($"- [T:{Thread.CurrentThread.ManagedThreadId:000}] start: {input.MessageBody}");
                    Task.Delay(1000 + rnd.Next(4000)).Wait();
                    Console.WriteLine($"- [T:{Thread.CurrentThread.ManagedThreadId:000}] end:   {input.MessageBody}");
                    return new DemoOutputMessage()
                    {
                        ReturnCode = 200,
                        ReturnBody = $"echo: [S:{pid}]/{input.MessageBody}",
                    };
                }
            })
            {
                var task = democ.StartWorkersAsync(worker_count); // start worker with 5 thread(s)...

                Console.WriteLine("PRESS ENTER to EXIT...");
                Console.ReadLine();

                Console.WriteLine("- shuting down...");
                democ.StopWorkers();
                Console.WriteLine("- shuted down.");
            }

        }
    }

    public class DemoRpcServer : RpcServerBase<DemoInputMessage, DemoOutputMessage>
    {
        public DemoRpcServer() : base("demo")
        {
        }
    }
}
