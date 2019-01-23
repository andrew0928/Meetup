using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

namespace ThreadSyncDemo
{
    class Program
    {
        static void Main(string[] args)
        {
            _timer.Restart();

            List<Thread> threads = new List<Thread>();
            for (int count = 0; count < 10; count++)
            {
                threads.Add(new Thread(Worker));
            }

            threads.Add(new Thread(ManualResetEventDemoWorker));
            //threads.Add(new Thread(AutoResetEventDemoWorker));

            threads.Add(new Thread(ScreenWorker));

            foreach (var t in threads) t.Start();
            foreach (var t in threads) t.Join();
        }


        static WaitHandle _wait = null;
        static int _total_seconds = 60;
        static Stopwatch _timer = new Stopwatch();
        static readonly object _sync = new object();

        static Dictionary<int, ThreadInfo> _chart = new Dictionary<int, ThreadInfo>();

        static void Worker()
        {
            int tid = Thread.CurrentThread.ManagedThreadId;
            lock (_sync) _chart[tid] = new ThreadInfo("worker", _total_seconds);

            Thread.Sleep(1000);
            while(_timer.Elapsed.TotalSeconds < _total_seconds)
            {
                Thread.Sleep(100);
                if (_wait != null) _wait.WaitOne();
                _chart[tid].Set(_timer.Elapsed, '#');
            }
        }

        static void ScreenWorker()
        {
            while(_timer.Elapsed.TotalSeconds < _total_seconds)
            {
                Thread.Sleep(300);
                //Console.Clear();
                Console.SetCursorPosition(0, 0);
                foreach(int tid in _chart.Keys)
                {
                    Console.WriteLine($"{_chart[tid].name.PadLeft(10)} [{tid:D03}]: {new string(_chart[tid].bar)}");
                }
            }
        }

        static void ManualResetEventDemoWorker()
        {
            int tid = Thread.CurrentThread.ManagedThreadId;
            lock (_sync) _chart[tid] = new ThreadInfo("manual", _total_seconds);

            _wait = new ManualResetEvent(false);
            while (_timer.Elapsed.TotalSeconds < _total_seconds)
            {
                ((ManualResetEvent)_wait).Reset();
                _chart[tid].Set(_timer.Elapsed, 'R');
                Thread.Sleep(3000);

                ((ManualResetEvent)_wait).Set();
                _chart[tid].Set(_timer.Elapsed, 'S');
                Thread.Sleep(3000);
            }
        }

        static void AutoResetEventDemoWorker()
        {
            int tid = Thread.CurrentThread.ManagedThreadId;
            lock (_sync) _chart[tid] = new ThreadInfo("auto", _total_seconds);

            _wait = new AutoResetEvent(false);
            while (_timer.Elapsed.TotalSeconds < _total_seconds)
            {
                //((AutoResetEvent)_wait).Reset();
                //_chart[tid].Set(_timer.Elapsed, 'R');
                Thread.Sleep(3000);

                ((AutoResetEvent)_wait).Set();
                _chart[tid].Set(_timer.Elapsed, 'S');
                Thread.Sleep(3000);
            }
        }
    }




    public class ThreadInfo
    {
        public char[] bar;
        public string name;

        public ThreadInfo(string x, int _total_seconds)
        {
            int tid = Thread.CurrentThread.ManagedThreadId;
            name = "worker";
            bar = new char[_total_seconds];
            for (int i = 0; i < _total_seconds; i++) bar[i] = '_';

            this.name = x;
        }

        public void Set(TimeSpan elasped, char ch)
        {
            this.bar[(int)elasped.TotalSeconds] = ch;
        }
    }
}
