using MQRPC.Core;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MQRPC.Client
{
    class Program
    {
        static void Main(string[] args)
        {
            Main().Wait();
        }

        static async Task Main()
        {
            int pid = Process.GetCurrentProcess().Id;

            Console.WriteLine("".PadRight(80, '-'));
            Console.WriteLine($"* MQRPC-Client, PID: {pid}");
            Console.WriteLine("".PadRight(80, '-'));

            using (DemoRpcClient demo = new DemoRpcClient())
            {
                for(int index = 0; index < 100; index++)
                {
                    await demo.SendAsync($"[C:{pid}]/[{index}] start...");

                    Task.WaitAll(
                        demo.SendAsync($"[C:{pid}]/[{index}] - job 01..."),
                        demo.SendAsync($"[C:{pid}]/[{index}] - job 02..."),
                        demo.SendAsync($"[C:{pid}]/[{index}] - job 03..."),
                        demo.SendAsync($"[C:{pid}]/[{index}] - job 04..."),
                        demo.SendAsync($"[C:{pid}]/[{index}] - job 05..."),
                        demo.SendAsync($"[C:{pid}]/[{index}] - job 06..."),
                        demo.SendAsync($"[C:{pid}]/[{index}] - job 07..."),
                        demo.SendAsync($"[C:{pid}]/[{index}] - job 08..."),
                        demo.SendAsync($"[C:{pid}]/[{index}] - job 09..."),
                        demo.SendAsync($"[C:{pid}]/[{index}] - job 10...")
                        );

                    await demo.SendAsync($"[C:{pid}]/[{index}] end...");
                }
            }
        }
    }



    public class DemoRpcClient: RpcClientBase<DemoInputMessage, DemoOutputMessage>
    {
        public DemoRpcClient() : base("demo")
        {
        }

        public async Task<(int code, string body)> SendAsync(string message)
        {
            _logger.Trace($"- call:   {message}");
            var output = await this.CallAsync(
                "",
                new DemoInputMessage()
                {
                    MessageBody = message,
                },
                new Dictionary<string, object>());
            _logger.Trace($"- return: {output.ReturnCode}, {output.ReturnBody}");
            return (output.ReturnCode, output.ReturnBody);
            //return (200, "OK好");
        }
    }

}
