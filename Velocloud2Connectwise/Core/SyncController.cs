using System;
using System.Collections;
using System.Collections.Generic;
using Velocloud2Connectwise.Models;
using Prometheus;
using Sentry;
using System.Threading;
namespace Velocloud2Connectwise.Core
{
    public static class SyncController
    {
        public static SyncResult CustomerResult { get; set; }
        public static SyncResult InventoryResult { get; set; }
        static Semaphore s = new Semaphore(1, 1);   // Will ensure that threads running Execute() method will run one at a time, in case triggered multiple times at once

        public static void ExportTest()
        {
            ConnectWise.Api cw = new ConnectWise.Api();
            Velocloud.Api vco = new Velocloud.Api();
            
            Helpers.ExportObjectToCsv<List<Models.ConnectWise.Configuration>>(cw.GetConfigurations(@"company/name=""Magna5"""), "/Users/gregro/Documents/connectwise-configurations-2.csv");
            //Helpers.ExportObjectToCsv<List<Models.ConnectWise.Manufacturer>>(cw.GetManufacturers(), "/Users/gregro/Documents/connectwise-manufacturers.csv");

            //List<Models.ConnectWise.Company> lstMatchingCustomers = cw.GetCompanies(@"name=""" + name + "\"");
            //List<Models.Velocloud.EnterpriseEdge> lstEdge = vco.GetEnterpriseEdge(1927);
            //List<Models.ConnectWise.Company> lstCompanies = cw.GetCompanies(@"name=""Magna5""");
            //Helpers.ExportObjectToCsv<List<Models.Velocloud.EnterpriseEdge>>(lstEdge, "/Users/gregro/Documents/velocloud-edge-detail-demo-co.csv");
        }
        /// <summary>
        /// Executes process to compare company accounts between Velocloud and ConnectWise and syncs inventory data.
        /// </summary>
        /// <returns>Sync Result object with total records, total unmatched, total synced & total errors.</returns>
        public static void Execute(string filteredCompanyName = "")
        {

            using (SentrySdk.Init())
            {
                s.WaitOne();

                Console.WriteLine("STARTING Velocloud to Connectwise Sync");

                Int32 uxTimeStart = (Int32)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
                Gauge nextSyncTime = Metrics.CreateGauge("velocloud2connectwise_next_sync_time", "Velocloud2Connectwise unix timestamp of next scheduled sync");
                nextSyncTime.IncTo(uxTimeStart);

                // Setup Prometheus gauges
                Gauge totalCustomerSync = Metrics.CreateGauge("velocloud2connectwise_account_total", "Total Velocloud customer accounts",
                     new GaugeConfiguration
                     {
                         LabelNames = new[] { "status" }
                     });
                Gauge totalInventorySync = Metrics.CreateGauge("velocloud2connectwise_inventory_total", "Total Velocloud inventory records",
                     new GaugeConfiguration
                     {
                         LabelNames = new[] { "status" }
                     });
                Gauge syncError = Metrics.CreateGauge("velocloud2connectwise_errors", "Velocloud2Connectwise sync errors");

                // Execute Sync 
                Sync sync = new Sync();

                List<Models.Velocloud.Company> companies = null;
                try
                {
                    companies = sync.SyncCompanyData(filteredCompanyName);
                    CustomerResult = sync.syncResults["customer"];
                    totalCustomerSync.WithLabels("success").IncTo(sync.syncResults["customer"].totalRecords);
                    totalCustomerSync.WithLabels("unmatched").IncTo(sync.syncResults["customer"].totalUnmatched);
                    totalCustomerSync.WithLabels("error").IncTo(sync.syncResults["customer"].totalErred);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error executing companies sync: " + ex.Message);
                    SentrySdk.CaptureException(ex);
                    syncError.Inc(1);
                }


                if (companies != null)
                {
                    try
                    {
                        // Use the list of companies from previous call to retrieve inventory data and sync to ConnectWise
                        sync.SyncInventory(companies);
                        InventoryResult = sync.syncResults["inventory"];
                        totalInventorySync.WithLabels("success").IncTo(sync.syncResults["inventory"].totalRecords);
                        totalInventorySync.WithLabels("unmatched").IncTo(sync.syncResults["inventory"].totalUnmatched);
                        totalInventorySync.WithLabels("synced").IncTo(sync.syncResults["inventory"].totalSync);
                        totalInventorySync.WithLabels("error").IncTo(sync.syncResults["inventory"].totalErred);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Error executing inventory sync: " + ex.Message);
                        SentrySdk.CaptureException(ex);
                        syncError.Inc(1);
                    }
                }
                else
                {
                    Console.WriteLine("Cannot sync inventory.  Problem retrieving velocloud companies.");
                }

                Int32 uxTimeEnd = (Int32)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
                Gauge lastSyncTime = Metrics.CreateGauge("velocloud2connectwise_last_sync_time", "Velocloud2Connectwise unix timestamp of last sync");
                lastSyncTime.IncTo(uxTimeEnd);

                Int32 uxDuration = uxTimeEnd - uxTimeStart;
                Gauge syncDuration = Metrics.CreateGauge("velocloud2connectwise_sync_duration_seconds", "Velocloud2Connectwise the number of seconds the last sync took to run");
                syncDuration.IncTo(uxDuration);

                Console.WriteLine("Velocloud to Connectwise Sync FINISHED");
                s.Release();
                return;
            }
        }
    }
}
