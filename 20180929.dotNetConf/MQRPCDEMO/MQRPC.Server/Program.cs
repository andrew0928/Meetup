using MQRPC.Core;
using System;
using System.Collections.Generic;
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
            using (DemoRpcServer democ = new DemoRpcServer()
            {
                Process = (input, cid) =>
                {
                    Console.WriteLine($"- [{Thread.CurrentThread.ManagedThreadId}] start: {input.MessageBody}");
                    Task.Delay(5000).Wait();
                    Console.WriteLine($"- [{Thread.CurrentThread.ManagedThreadId}] end:   {input.MessageBody}");
                    return new DemoOutputMessage()
                    {
                        ReturnCode = 200,
                        ReturnBody = $"echo: {input.MessageBody}",
                    };
                }
            })
            {
                var task = democ.StartWorkersAsync(5); // start worker with 5 thread(s)...

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
