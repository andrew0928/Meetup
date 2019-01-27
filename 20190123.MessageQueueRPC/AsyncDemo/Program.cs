using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace AsyncDemo
{
    class Program
    {
        static async Task Demo_CallerAsync()
        {
            TimeLineInfo tlinfo = new TimeLineInfo("caller", _total_seconds);

            tlinfo.Run(_timer.Elapsed, TimeSpan.FromSeconds(5), 'A');
            var job = Demo_WorkerAsync();
            tlinfo.Run(_timer.Elapsed, TimeSpan.FromSeconds(5), 'C');
            await job;
            tlinfo.Run(_timer.Elapsed, TimeSpan.FromSeconds(10), 'E');
        }

        static async Task Demo_WorkerAsync()
        {
            TimeLineInfo tlinfo = new TimeLineInfo("job", _total_seconds);

            tlinfo.Run(_timer.Elapsed, TimeSpan.FromSeconds(3), 'B');
            await Task.Delay(3000);
            tlinfo.Run(_timer.Elapsed, TimeSpan.FromSeconds(10), 'D');
        }


        static void Main(string[] args)
        {
            Thread worker = new Thread(ScreenWorker);
            worker.Start();
            _timer.Restart();



            Demo_CallerAsync().Wait();




            worker.Join();
        }


        //static WaitHandle _wait = null;
        static int _total_seconds = 60;
        static Stopwatch _timer = new Stopwatch();

        static Dictionary<string, TimeLineInfo> _chart = new Dictionary<string, TimeLineInfo>();


        static void ScreenWorker()
        {
            TimeLineInfo timebar = new TimeLineInfo("clock", _total_seconds);

            while (_timer.Elapsed.TotalSeconds < _total_seconds)
            {
                timebar.Set(_timer.Elapsed, (((int)_timer.Elapsed.TotalSeconds+1)%5==0) ?'|':'=');

                Thread.Sleep(100);
                //Console.Clear();
                Console.SetCursorPosition(0, 0);
                foreach (string tid in _chart.Keys)
                {
                    Console.WriteLine($"{_chart[tid].Name.PadLeft(10)}: {new string(_chart[tid].Bar)}");
                }
            }
        }




        public class TimeLineInfo
        {
            public char[] Bar;
            public string Name;

            public TimeLineInfo(string name, int total_seconds)
            {
                //int tid = Thread.CurrentThread.ManagedThreadId;
                Bar = new char[total_seconds];
                for (int i = 0; i < total_seconds; i++) Bar[i] = '_';

                this.Name = name;

                Program._chart.Add(name, this);
            }

            public void Set(TimeSpan elasped, char ch)
            {
                this.Bar[(int)elasped.TotalSeconds] = ch;
            }

            public void Run(TimeSpan elasped, TimeSpan duration, char ch)
            {
                Stopwatch timer = new Stopwatch();
                timer.Restart();
                while (timer.Elapsed < duration)
                {
                    this.Set(elasped + timer.Elapsed, ch);
                    Thread.Sleep(100);
                }
            }
        }

    }





}
