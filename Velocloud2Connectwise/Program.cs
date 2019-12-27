using System;
using System.Net;
using Velocloud2Connectwise.Core;
using System.Threading;
using System.Configuration;
using Prometheus;
namespace Velocloud2Connectwise
{
    class Program
    {
        static HttpListener listener;
        static Thread listenerThread;

        static void Main(string[] args)
        {
            ThreadPool.QueueUserWorkItem(delegate { HandleNetRequests(); });
            ThreadPool.QueueUserWorkItem(delegate { JobTimer(); });
            ThreadPool.QueueUserWorkItem(delegate { StatTimer(); });
            Console.ReadLine();
        }

        public static void HandleNetRequests()
        {
            listener = new HttpListener();
            listener.Prefixes.Add(@"http://+:8077/");
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
            SyncController.Execute(1);
        }

        static void JobTimer()
        {
            while (true)
            {
                Thread.Sleep(Convert.ToInt32(ConfigurationManager.AppSettings["jobTimer"]) * 60 * 1000);
                Console.WriteLine("Job timer elapsed");
                SyncController.Execute(1);
            }
            
        }
        private static readonly Counter TickTock = Metrics.CreateCounter("velocloud2connectwise_ticks_total", "Velocloud2Connectwise sync console app ticks total");
        static void StatTimer()
        {
            var server = new MetricServer(hostname: ConfigurationManager.AppSettings["prometheusServer"],
                                          port: Convert.ToInt32(ConfigurationManager.AppSettings["prometheusPort"]));

            server.Start();
            while (true)
            {
                TickTock.Inc();
                Thread.Sleep(TimeSpan.FromSeconds(1));
            }
        }
    }
}
