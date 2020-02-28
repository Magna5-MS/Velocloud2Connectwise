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

        static MetricServer server = new MetricServer(port: Convert.ToInt32(Environment.GetEnvironmentVariable("httpPort")));

        static void Main(string[] args)
        {
            server.Start();
#if DEBUG
            SetupThreads();
            Thread.Sleep(Timeout.Infinite);
#else
            SetupThreads();
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
            Console.WriteLine(String.Format("Setting up listener on port {0}", Environment.GetEnvironmentVariable("httpPort")));
            HttpListener listener = new HttpListener();
            listener.Prefixes.Add(@"http://+:" + Environment.GetEnvironmentVariable("httpPort") + "/");
            listener.Start();
            while (listener.IsListening)
            {
                var context = listener.BeginGetContext(new AsyncCallback(ListenerCallback), listener);
                context.AsyncWaitHandle.WaitOne();
            }
        }
        private static void ListenerCallback(IAsyncResult ar)
        {
            HttpListener l = (HttpListener)ar.AsyncState;
            HttpListenerContext context = l.EndGetContext(ar);
            HttpListenerRequest request = context.Request;

            // Most browser make 2 requests, 1 specifically for favicon
            string response = "";
            if (request.RawUrl == "/trigger")
            {

                Console.WriteLine("Callback detected...");
                new Thread((Object state) =>
                        {
                            SyncController.Execute();
                        }).Start();

                response = "Beginning customer and inventory sync between Velocloud and ConnectWise";
                byte[] buffer = System.Text.Encoding.UTF8.GetBytes(response);
                context.Response.ContentLength64 = buffer.Length;
                System.IO.Stream output = context.Response.OutputStream;
                output.Write(buffer, 0, buffer.Length);
                output.Close();

                // TODO: Print sync results once finished.
                //SyncController.Execute();
                //// Send response
                //if (SyncController.CustomerResult != null)
                //{
                //    response = String.Format("Found {0} unmatched customer accounts from {1} total accounts<br />", SyncController.CustomerResult.totalUnmatched, SyncController.CustomerResult.totalRecords);
                //    response += String.Format("Synced {0} inventory records from {1} total velocloud inventory<br />", SyncController.InventoryResult.totalSync, SyncController.CustomerResult.totalRecords);
                //}

            }

        }

        static void JobTimer()
        {
            Console.WriteLine(String.Format("Setting up timer with value {0}", Environment.GetEnvironmentVariable("jobTimer")));
            var counterJobElapsed = Metrics.CreateCounter("velocloud2connectwise_job_count", "Elapsed Job Sync");
            while (true)
            {
                Int32 uxTimeNow = (Int32)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
                Int32 uxTimeNext = uxTimeNow + Convert.ToInt32(Environment.GetEnvironmentVariable("jobTimer")) * 60;
                Gauge nextSyncTime = Metrics.CreateGauge("velocloud2connectwise_next_sync_time", "Velocloud2Connectwise unix timestamp of next scheduled sync");
                nextSyncTime.IncTo(uxTimeNext);

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
