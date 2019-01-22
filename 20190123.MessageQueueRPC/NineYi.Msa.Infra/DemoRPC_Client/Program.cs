using NineYi.Msa.Infra;
using NineYi.Msa.Infra.Messaging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DemoRPC_Client
{
    class Program
    {
        //static void Main(string[] args)
        //{
        //    Main(args).Wait();
        //}

        static async Task Main(string[] args)
        {
            int pid = Process.GetCurrentProcess().Id;

            Console.WriteLine("".PadRight(80, '-'));
            Console.WriteLine($"* MQRPC-Client, PID: {pid}");
            Console.WriteLine("".PadRight(80, '-'));


            //new TrackContext()
            //{
            //    ShopId = 8888,
            //    MemberId = "andrew",
            //    SessionId = "0000",
            //    ChannelId = "1111",
            //    RequestId = Guid.NewGuid().ToString()
            //}

            using (var client = new MessageClient<DemoInputMessage, DemoOutputMessage>(new MessageClientOptions()
            {
                ConnectionName = "demo-client",
                BusType = MessageClientOptions.MessageBusTypeEnum.QUEUE,
                QueueName = "demo",
                ConnectionURL = args[0], //"amqp://guest:guest@localhost:5672/",
            },
            null,
            null))
            {
                DemoOutputMessage output = null;
                Stopwatch timer = new Stopwatch();
                for (int index = 1; index <= 100; index++)
                {
                    output = await client.SendMessageAsync("", new DemoInputMessage()
                    {
                        MessageBody = $"[C:{pid}]/[{index:000}] start..."
                    }, null);
                    Console.WriteLine($"- [{DateTime.Now:HH:mm:ss}] {output.ReturnCode}, {output.ReturnBody}");

                    Console.WriteLine($"- [{DateTime.Now:HH:mm:ss}] - sending 10 jobs to worker queue at the same time...");
                    timer.Restart();
                    Task.WaitAll(
                        client.SendMessageAsync("", new DemoInputMessage() { MessageBody = $"[C:{pid}]/[{index:000}] - job 01..." }, null),
                        client.SendMessageAsync("", new DemoInputMessage() { MessageBody = $"[C:{pid}]/[{index:000}] - job 02..." }, null),
                        client.SendMessageAsync("", new DemoInputMessage() { MessageBody = $"[C:{pid}]/[{index:000}] - job 03..." }, null),
                        client.SendMessageAsync("", new DemoInputMessage() { MessageBody = $"[C:{pid}]/[{index:000}] - job 04..." }, null),
                        client.SendMessageAsync("", new DemoInputMessage() { MessageBody = $"[C:{pid}]/[{index:000}] - job 05..." }, null),
                        client.SendMessageAsync("", new DemoInputMessage() { MessageBody = $"[C:{pid}]/[{index:000}] - job 06..." }, null),
                        client.SendMessageAsync("", new DemoInputMessage() { MessageBody = $"[C:{pid}]/[{index:000}] - job 07..." }, null),
                        client.SendMessageAsync("", new DemoInputMessage() { MessageBody = $"[C:{pid}]/[{index:000}] - job 08..." }, null),
                        client.SendMessageAsync("", new DemoInputMessage() { MessageBody = $"[C:{pid}]/[{index:000}] - job 09..." }, null),
                        client.SendMessageAsync("", new DemoInputMessage() { MessageBody = $"[C:{pid}]/[{index:000}] - job 10..." }, null));
                    Console.WriteLine($"- [{DateTime.Now:HH:mm:ss}] - all jobs (01 ~ 10) execute complete and return, average throughput: {10000.0 / timer.ElapsedMilliseconds} jobs/sec");

                    //await RemoteCallAsync($"[C:{pid}]/[{index:000}] end...");
                    output = await client.SendMessageAsync("", new DemoInputMessage()
                    {
                        MessageBody = $"[C:{pid}]/[{index:000}] end..."
                    }, null);
                    Console.WriteLine($"- [{DateTime.Now:HH:mm:ss}] {output.ReturnCode}, {output.ReturnBody}");

                    Console.WriteLine();
                }
            }
        }
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
