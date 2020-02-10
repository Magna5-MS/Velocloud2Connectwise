using System;
using System.Net;
using Velocloud2Connectwise.Models;
using Velocloud2Connectwise.Core;
using System.Threading;
using Prometheus;

namespace Velocloud2Connectwise
{
    class Program
    {

        static MetricServer server = new MetricServer(port: Convert.ToInt32(Environment.GetEnvironmentVariable("prometheusPort")));

        static void Main(string[] args)
        {
            server.Start();
#if DEBUG
            SyncController.Execute();
#else
            SetupThreads();
            // Put main thread to sleep to stay alive in docker
            Thread.Sleep(Timeout.Infinite);
#endif

        }
        public static void SetupThreads(){
            Thread threadNetRequests = new Thread(HandleNetRequests) { IsBackground = true };
            Thread threadJobTimer = new Thread(JobTimer) { IsBackground = true };
            Thread threadCounter = new Thread(StatTimer) { IsBackground = true };
            threadNetRequests.Start();
            threadJobTimer.Start();
            threadCounter.Start();
            
        }

        public static void HandleNetRequests()
        {
            Console.WriteLine(String.Format("Setting up listener on port {0}", Environment.GetEnvironmentVariable("triggerPort")));
            HttpListener listener = new HttpListener();
            listener.Prefixes.Add(@"http://+:" + Environment.GetEnvironmentVariable("triggerPort") + "/");
            listener.Start();
            while (listener.IsListening)
            {
                var context = listener.BeginGetContext(new AsyncCallback(ListenerCallback), listener);
                context.AsyncWaitHandle.WaitOne();
            }
        }
        private static void ListenerCallback(IAsyncResult ar)
        {
            Console.WriteLine("Callback detected...");
            // var listener = ar.AsyncState as HttpListener;
            //var context = listener.EndGetContext(ar);
            SyncController.Execute();
        }

        static void JobTimer()
        {
            Console.WriteLine(String.Format("Setting up timer with value {0}", Environment.GetEnvironmentVariable("jobTimer")));
            var counterJobElapsed = Metrics.CreateCounter("velocloud2connectwise_job_elapsed", "Elapsed Job Sync");
            while (true)
            {
                Thread.Sleep(Convert.ToInt32(Environment.GetEnvironmentVariable("jobTimer")) * 60 * 1000);
                Console.WriteLine("Job timer elapsed");
                SyncController.Execute();
                counterJobElapsed.Inc();
            }
            
        }
        
        static void StatTimer()
        {
            Counter counterTicks = Metrics.CreateCounter("velocloud2connectwise_ticks_total", "Velocloud2Connectwise sync console app ticks total");
            
            while (true)
            {
                counterTicks.Inc();
                Thread.Sleep(TimeSpan.FromSeconds(1));
            }
        }
    }
}
