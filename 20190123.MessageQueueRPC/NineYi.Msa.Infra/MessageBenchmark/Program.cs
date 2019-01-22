using NineYi.Msa.Infra.Messaging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MessageBenchmark
{
    class Program
    {
        static async Task Main(string[] args)
        {
            int worker_threads = 10;
            int worker_instances = 10;
            long _message_max_count = 50000;
            Stopwatch timer = new Stopwatch();


            // insert 5000 messages
            Console.WriteLine($"# insert test message(s): {_message_max_count}");
            using (var client = new MessageClient<DemoMessage>(new MessageClientOptions()
            {
                ConnectionName = "benchmark-client",
                BusType = MessageClientOptions.MessageBusTypeEnum.QUEUE,
                QueueName = "benchmark-queue"
            }, null, null))
            {
                timer.Restart();
                for (int i = 0; i < _message_max_count; i++)
                {
                    client.SendMessage("", new DemoMessage(), null);
                    // Console.Write(".");
                }
                Console.WriteLine();
                Console.WriteLine($"# send message: {_message_max_count / timer.Elapsed.TotalSeconds} mps");
            }




            long _message_per_second = 0;
            long _message_total = 0;

            // dequeue 5000 messages
            Console.WriteLine();
            Console.WriteLine($"# process test message(s): {_message_max_count}");

            List<MessageWorker<DemoMessage>> workers = new List<MessageWorker<DemoMessage>>();

            // start worker(s)...
            for(int index = 0; index < worker_instances; index++)
            {
                var worker = new MessageWorker<DemoMessage>(new MessageWorkerOptions()
                {
                    ConnectionName = $"benchmark-worker({index + 1})",
                    QueueName = "benchmark-queue",
                    WorkerThreadsCount = worker_threads,
                    PrefetchCount = 1
                },null, null, null)
                {
                    Process = (message, cid, scope) =>
                    {
                        Interlocked.Increment(ref _message_per_second);
                        Interlocked.Increment(ref _message_total);
                        return;
                    }
                };

                workers.Add(worker);
            }


            foreach (var worker in workers)
            {
                var x = worker.StartAsync(CancellationToken.None);
            }

            // run benchmark
            int zero_mps_count = 0;
            do
            {
                timer.Restart();
                Task.Delay(500).Wait();
                Console.WriteLine($"- dequeue: {_message_per_second / timer.Elapsed.TotalSeconds,-5} mps, total: {_message_total} / {_message_max_count}");
                if (_message_per_second == 0) zero_mps_count++; else zero_mps_count = 0;
                _message_per_second = 0;
            } while (zero_mps_count < 3);

            // stop worker(s)...
            Task[] tasks = new Task[worker_instances];
            for (int index = 0; index < workers.Count; index++)
            {
                tasks[index] = workers[index].StopAsync(CancellationToken.None);
            }
            Task.WaitAll(tasks);



            //using (var server = new MessageWorker<DemoMessage>(new MessageWorkerOptions()
            //{
            //    ConnectionName = "benchmark-worker",
            //    QueueName = "benchmark-queue",
            //    WorkerThreadsCount = worker_threads,
            //    PrefetchCount = 0
            //}, null)
            //{
            //    Process = (message, cid, scope) =>
            //    {
            //        Interlocked.Increment(ref _message_per_second);
            //        Interlocked.Increment(ref _message_total);
            //        return;
            //    }
            //})
            //{
            //    var start = server.StartAsync(CancellationToken.None);

            //    int zero_mps_count = 0;
            //    do
            //    {
            //        timer.Restart();
            //        Task.Delay(500).Wait();
            //        Console.WriteLine($"- dequeue: {_message_per_second / timer.Elapsed.TotalSeconds, -5} mps, total: {_message_total} / {_message_max_count}");
            //        if (_message_per_second == 0) zero_mps_count++; else zero_mps_count = 0;
            //        _message_per_second = 0;
            //    } while (zero_mps_count < 3);

            //    await server.StopAsync(CancellationToken.None);
            //}
        }
    }




    public class DemoMessage : MessageBase
    {

    }
}
