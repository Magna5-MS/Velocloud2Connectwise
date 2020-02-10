using System;
using System.Collections;
using System.Collections.Generic;
using Velocloud2Connectwise.Models;
using Prometheus;
using Sentry;
namespace Velocloud2Connectwise.Core
{
    public static class SyncController
    {
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
            using (SentrySdk.Init("https://da4d4d6ac04e4f858a1a2707c8b4fa14@sentry.io/2239777"))
            {
                Console.WriteLine("STARTING Velocloud to Connectwise Sync");

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
                Gauge customerSyncError = Metrics.CreateGauge("velocloud2connectwise_customersync_error", "Velocloud2Connectwise customer sync errors");
                Gauge inventorySyncError = Metrics.CreateGauge("velocloud2connectwise_inventorysync_error", "Velocloud2Connectwise inventory sync errors");

                // Execute Sync 
                Sync sync = new Sync();

                List<Models.Velocloud.Company> companies = null;
                try
                {
                    companies = sync.SyncCompanyData(filteredCompanyName);
                    totalCustomerSync.WithLabels("success").IncTo(sync.syncResults["customer"].totalRecords);
                    totalCustomerSync.WithLabels("unmatched").IncTo(sync.syncResults["customer"].totalUnmatched);
                    totalCustomerSync.WithLabels("error").IncTo(sync.syncResults["customer"].totalErred);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error executing companies sync: " + ex.Message);
                    SentrySdk.CaptureException(ex);
                    customerSyncError.Inc(1);
                }


                if (companies != null)
                {
                    try
                    {
                        // Use the list of companies from previous call to retrieve inventory data and sync to ConnectWise
                        sync.SyncInventory(companies);
                        totalInventorySync.WithLabels("success").IncTo(sync.syncResults["inventory"].totalRecords);
                        totalInventorySync.WithLabels("unmatched").IncTo(sync.syncResults["inventory"].totalUnmatched);
                        totalInventorySync.WithLabels("synced").IncTo(sync.syncResults["inventory"].totalSync);
                        totalInventorySync.WithLabels("error").IncTo(sync.syncResults["inventory"].totalErred);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Error executing inventory sync: " + ex.Message);
                        SentrySdk.CaptureException(ex);
                        inventorySyncError.Inc(1);
                    }
                }
                else
                {
                    Console.WriteLine("Cannot sync inventory.  Problem retrieving velocloud companies.");
                }

                Console.WriteLine("Velocloud to Connectwise Sync FINISHED");
                return;
            }
        }
    }
}
