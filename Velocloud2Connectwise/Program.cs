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
            Thread threadNetRequests = new Thread(HandleNetRequests) { IsBackground = true };
            Thread threadJobTimer = new Thread(JobTimer) { IsBackground = true };
            Thread threadCounter = new Thread(StatTimer) { IsBackground = true };
            
            threadNetRequests.Start();
            threadJobTimer.Start();
            threadCounter.Start();

            Console.ReadLine();

        }

        public static void HandleNetRequests()
        {
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
            ExecuteSync();
        }

        static void JobTimer()
        {
            var counterJobElapsed = Metrics.CreateCounter("velocloud2connectwise_job_elapsed", "Elapsed Job Sync");
            while (true)
            {
                Thread.Sleep(Convert.ToInt32(Environment.GetEnvironmentVariable("jobTimer")) * 60 * 1000);
                Console.WriteLine("Job timer elapsed");
                ExecuteSync();
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
        static void ExecuteSync()
        {
            // Setup Prometheus gauges
            Gauge totalAccounts = Metrics.CreateGauge("velocloud2connectwise_account_total", "Total Velocloud customer accounts",
                 new GaugeConfiguration
                 {
                     LabelNames = new[] { "status" }
                 });
            Gauge counterError = Metrics.CreateGauge("velocloud2connectwise_error", "Velocloud2Connectwise sync errors");

            SyncResult result;
            try
            {
                result = SyncController.Execute(1);
                totalAccounts.WithLabels("success").IncTo(result.totalAccount);
                totalAccounts.WithLabels("unmatched").IncTo(result.totalUnmatched);
                totalAccounts.WithLabels("error").IncTo(result.totalErred); // Error count for CW sync-backs, not yet done
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error executing sync: " + ex.Message);
                counterError.Inc(1);
            }
            return;
        }
    }
}
