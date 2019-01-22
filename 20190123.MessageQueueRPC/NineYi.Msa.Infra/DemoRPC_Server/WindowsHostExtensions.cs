using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace DemoRPC_Server
{
    public static class WindowsHostExtensions
    {
        /// <summary>
        /// Starts the host synchronously.
        /// </summary>
        /// <param name="host"></param>
        public static void WinStart(this IHost host)
        {
            SetConsoleCtrlHandler(ShutdownHandler, true);
            host.StartAsync().GetAwaiter().GetResult();
        }

        /// <summary>
        /// Block the calling thread until shutdown is triggered via Ctrl+C or SIGTERM.
        /// </summary>
        /// <param name="host">The running <see cref="IHost"/>.</param>
        public static void WaitForWinShutdown(this IHost host)
        {
            host.WaitForWinShutdownAsync().Wait(); //.GetAwaiter().GetResult();
        }

        /// <summary>
        /// Returns a Task that completes when shutdown is triggered via the given token.
        /// </summary>
        /// <param name="host">The running <see cref="IHost"/>.</param>
        /// <param name="token">The token to trigger shutdown.</param>
        public static async Task WaitForWinShutdownAsync(this IHost host, CancellationToken token = default)
        {
            //await Task.Run(()=>{ close.WaitOne(); });

            string[] message = new string[]
            {
                "BackgroundService EXIT Handler",
                "Kernel32: SetConsoleCtrlHandler"
            };

            int index = Task.WaitAny(
                host.WaitForShutdownAsync(),
                Task.Run(() => { close.WaitOne(); }));

            switch (index)
            {
                case 0:
                    // IHostedServices
                    break;
                case 1:
                    // Kernel32: SetConsoleCtrlHandler
                    await host.StopAsync();
                    close_ack.Set();
                    SetConsoleCtrlHandler(ShutdownHandler, false);
                    break;
            }


            //Console.WriteLine($"SHUTDOWN SIGNAL: {message[index]}");


            //await host.StopAsync();
            //close_ack.Set();
            //SetConsoleCtrlHandler(ShutdownHandler, false);
            //Console.WriteLine($"SHUTDOWN SIGNAL: {message[index]}");
        }

        private static ManualResetEvent close = new ManualResetEvent(false);
        private static ManualResetEvent close_ack = new ManualResetEvent(false);

        [DllImport("Kernel32")]
        static extern bool SetConsoleCtrlHandler(EventHandler handler, bool add);

        delegate bool EventHandler(CtrlType sig);


        // reference: https://docs.microsoft.com/en-us/windows/console/handlerroutine
        enum CtrlType
        {
            CTRL_C_EVENT = 0,
            CTRL_BREAK_EVENT = 1,
            CTRL_CLOSE_EVENT = 2,
            CTRL_LOGOFF_EVENT = 5,
            CTRL_SHUTDOWN_EVENT = 6
        }

        private static bool ShutdownHandler(CtrlType sig)
        {
            Console.WriteLine($"EVENT: ShutdownHandler({sig}) - RECEIVED");
            close.Set();
            close_ack.WaitOne();
            Console.WriteLine($"EVENT: ShutdownHandler({sig}) - DONE");
            return true;
        }

    }
    
}
