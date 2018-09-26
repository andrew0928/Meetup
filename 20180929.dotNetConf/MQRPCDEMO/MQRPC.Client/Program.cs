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
            using (DemoRpcClient demo = new DemoRpcClient())
            {
                int pid = Process.GetCurrentProcess().Id;
                for(int index = 0; index < 100; index++)
                {
                    await demo.SendAsync($"[{pid}]/[{index}] start...");

                    Task.WaitAll(
                        demo.SendAsync($"[{pid}]/[{index}] - job 01..."),
                        demo.SendAsync($"[{pid}]/[{index}] - job 02..."),
                        demo.SendAsync($"[{pid}]/[{index}] - job 03..."),
                        demo.SendAsync($"[{pid}]/[{index}] - job 04..."),
                        demo.SendAsync($"[{pid}]/[{index}] - job 05..."),
                        demo.SendAsync($"[{pid}]/[{index}] - job 06..."),
                        demo.SendAsync($"[{pid}]/[{index}] - job 07..."),
                        demo.SendAsync($"[{pid}]/[{index}] - job 08..."),
                        demo.SendAsync($"[{pid}]/[{index}] - job 09..."),
                        demo.SendAsync($"[{pid}]/[{index}] - job 10...")
                        );

                    await demo.SendAsync($"[{pid}]/[{index}] end...");
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
            _logger.Trace($"- return: {message}");
            return (output.ReturnCode, output.ReturnBody);
            //return (200, "OK好");
        }
    }

}
