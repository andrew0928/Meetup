using MQRPC.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MQRPC.Client
{
    class Program
    {
        static void Main(string[] args)
        {
            using (DemoRpcClient demo = new DemoRpcClient())
            {
                var a = demo.SendAsync("Hello!");
                Console.WriteLine($"received: {a.Result.code}, {a.Result.body}");
                
                var b = demo.SendAsync("This is Andrew speaking...");
                Console.WriteLine($"received: {b.Result.code}, {b.Result.body}");

                List<Task<(int code, string body)>> tasks = new List<Task<(int code, string body)>>();
                for (int i = 0; i < 10; i++)
                {
                    tasks.Add(demo.SendAsync($"Sequence Messages: #{i:00000}..."));
                }

                for (int i = 0; i < 10; i++)
                {
                    var c = tasks[i];
                    c.Wait();
                    Console.WriteLine($"received: {c.Result.code}, {c.Result.body}");
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
            var output = await this.CallAsync(
                "",
                new DemoInputMessage()
                {
                    MessageBody = message,
                },
                new Dictionary<string, object>());
            //Console.WriteLine($"Message Send:     [{message}]");
            return (output.ReturnCode, output.ReturnBody);
            //return (200, "OK好");
        }
    }

}
