using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Velocloud2Connectwise.Models;

namespace Velocloud2Connectwise
{
    class Sync
    {
        public Dictionary<string, SyncResult> syncResults { get; set; }
        //public List<SyncResult> syncResults { get; set; }
        private ConnectWise.Api cw;
        private Velocloud.Api vco;

        public Sync()
        {
            cw = new ConnectWise.Api();
            vco = new Velocloud.Api();

            syncResults = new Dictionary<string, SyncResult>();
            syncResults.Add("customer", new SyncResult());
            syncResults.Add("inventory", new SyncResult());
        }
        public void GetEdgeTest()
        {
            Velocloud.Api vco = new Velocloud.Api();
            Helpers.ExportObjectToCsv<List<Models.Velocloud.Company>>(vco.GetCompaniesObject(), "/Users/gregro/Documents/velocloud-companies.csv");
        }
        /// <summary>
        /// Get VCO and CW companies as objects, add missing ones to CW.  Returns list of velocloud companies.
        /// </summary>
        public List<Models.Velocloud.Company> SyncCompanyData(string filterCompanyName = "")
        {
            List<Models.Velocloud.Company> lstVcoCompanies;
            List<Models.ConnectWise.Company> lstCwCompanies;

            try
            {
                lstVcoCompanies = vco.GetCompaniesObject();
            }
            catch (Exception ex)
            {
                throw new Exception("Error retrieving velocloud companies: " + ex.Message);
            }

            try
            {
                lstCwCompanies = cw.GetCompaniesObject();
            }
            catch (Exception ex)
            {
                throw new Exception("Error retrieving connectwise companies: " + ex.Message);
            }

            if (filterCompanyName.Length > 0)
            {
                // If specific company name provided, use only that company
                List<Models.Velocloud.Company> lstFilteredList;
                lstFilteredList = lstVcoCompanies.FindAll(i => i.name == filterCompanyName);
                lstVcoCompanies = lstFilteredList;
            }

            List<Models.ConnectWise.Company> lstMatches = null;

            foreach (Models.Velocloud.Company company in lstVcoCompanies)
            {
                Console.WriteLine("VcoID: " + company.id);
                Console.WriteLine("Address: " + company.streetAddress + " " + company.streetAddress2 + " " + company.city + " " + company.state + " " + company.postalCode + " " + company.country);
                Console.WriteLine("AccNum: " + company.accountNumber);
                Console.WriteLine("PhoneNum: " + company.contactPhone);


                // Search for company in cw list
                lstMatches = lstCwCompanies.FindAll(i =>
                                (i.name == company.name) || i.identifier == MakeIdentifier(company.name)
                            );
                if (lstMatches.Count == 0)
                    company.containsMatch = false;
                else
                    Console.WriteLine("VCO company found in CW.");
            }

            // Now take a look at our unmatched companies and send email if necessary

            List<Models.Velocloud.Company> lstUnmatched = null;
            lstUnmatched = lstVcoCompanies.FindAll(i => i.containsMatch == false);
            syncResults["customer"].totalRecords= lstVcoCompanies.Count;
            syncResults["customer"].totalUnmatched = lstUnmatched.Count;


            // Send email report of all unmatched companies
            if (syncResults["customer"].totalUnmatched > 0)
            {
                string emailBody = "The following VeloCloud companies could not be found in ConnectWise. Please verify the name and that the company exists in ConnectWise.<br /><br />";
                foreach (Models.Velocloud.Company company in lstUnmatched)
                {
                    emailBody += company.name + " (" + company.id + ")<br />";
                }
                try
                {
                    Helpers.SendEmail(Environment.GetEnvironmentVariable("smtpRelay"),
                                        Environment.GetEnvironmentVariable("emailReportTo"),
                                        "donotreply@magna5global.com",
                                        "VeloCloud - New Accounts Synced",
                                        emailBody, null, null, true);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error sending synced account email: " + ex.Message);
                }

                Console.WriteLine(lstUnmatched.Count.ToString() + " unmatched compan" + (lstUnmatched.Count == 1 ? "y" : "ies") + " in VeloCloud that are not in ConnectWise.");
            }
            return lstVcoCompanies;

        }
        /// <summary>
        /// Retrieves inventory data from Velocloud and inserts missing data into ConnectWise
        /// </summary>
        public void SyncInventory(List<Models.Velocloud.Company> lstCompanies)
        {
            // Loop through velocloud customer accounts and retrieve edge inventory data by enteprise id
            List<Models.Velocloud.EnterpriseEdge> lstVeloInventory;
            foreach (Models.Velocloud.Company company in lstCompanies)
            {
                // Find the customer record based on velocloud customer name
                List<Models.ConnectWise.Company> lstMatchingCustomers = cw.GetCompanies(@"name=""" + company.name + "\"");
                if (lstMatchingCustomers.Count == 0)
                {
                    Console.WriteLine("Matching customer not found in Connectwise with name " + company.name);
                    syncResults["inventory"].totalUnmatched += 1;
                    continue;
                }

                // Retrieve ConnectWise inventory for company
                List<Models.ConnectWise.Configuration> lstCwInventory;
                try
                {
                    lstCwInventory = cw.GetConfigurations(@"company/name=""" + company.name + "\"");
                }
                catch (Exception ex)
                {
                    throw new Exception("Error retrieving connectwise inventory:" + ex.Message);
                }
                Console.WriteLine("Found " + lstCwInventory.Count + " ConnectWise inventory (configuration) records");

                // Retrieve VeloCloud inventory for company 
                try
                {
                    lstVeloInventory = vco.GetEnterpriseEdge(Convert.ToInt32(company.id));
                }
                catch (Exception ex)
                {
                    throw new Exception("Error retrieving velocloud inventory: " + ex.Message);
                }

                Console.WriteLine("Found " + lstVeloInventory.Count + " Velocloud inventory records for enterprise id " + company.id);

                syncResults["inventory"].totalRecords += lstVeloInventory.Count;

                // Loop through each piece of Velocloud inventory and compare against ConnectWise
                foreach (Models.Velocloud.EnterpriseEdge veloInv in lstVeloInventory)
                {
                    // Search for matching inventory based on serial number
                    if ((lstCwInventory.FirstOrDefault(i => (i.serialNumber == veloInv.serialNumber))) != null)
                    {
                        // Found match, already exists in connectwise, ignore record
                        Console.WriteLine("Veloclound inventory item with serial " + veloInv.serialNumber + " already synced");
                        continue;
                    }

                    Console.WriteLine("Velocloud inventory item with serial " + veloInv.serialNumber + " not found in Connectwise, attempting add");
                    // Didn't find matching inventory, setup new one
                    Models.ConnectWise.Configuration newInv = new Models.ConnectWise.Configuration();

                    // Post new piece of inventory;
                    newInv.company = lstMatchingCustomers[0];
                    newInv.name = veloInv.name;

                    newInv.status = new Models.ConnectWise.IdNameInfo() { id = 2, name = "Active" };
                    newInv.type = new Models.ConnectWise.IdNameInfo() { id = 65, name="Switch/Router" }; // Switch/Router
                    newInv.serialNumber = veloInv.serialNumber;
                    newInv.notes = veloInv.haSerialNumber;
                    newInv.modelNumber = veloInv.modelNumber;
                    newInv.osInfo = veloInv.softwareVersion;
                    newInv.manufacturer = new Models.ConnectWise.IdNameInfo { id = 245, name = "Velocloud" };
                    if (veloInv.created.Length > 0)
                            newInv.installationDate = veloInv.created.Substring(0, veloInv.created.Length - 5) + "Z"; // strip off ms
                    newInv.macAddress = veloInv.selfMacAddress;
                    if (veloInv.links != null) {
                        if (veloInv.links.Count > 0)
                            newInv.ipAddress = veloInv.links[0].ipAddress;
                    }

                    try
                    {
                        cw.AddConfiguration(newInv);
                        Console.WriteLine("Success adding inventory item to Connectwise with serial #" + veloInv.serialNumber);
                    }
                    catch (Exception ex)
                    {
                        throw new Exception("Connectwise POST failed to add new inventory: " + ex.Message);
                    }
                    syncResults["inventory"].totalSync += 1;
                }
            }


         
            
        }

      

        /// <summary>
        /// Get public/private keys and base URL for API
        /// </summary>
        public static List<string> GetApiInfo(string apiName)
        {
            string baseURL = "", consumerKey = "", consumerSecret = "", additionalVal = "";
            if (apiName == "ConnectWise")
            {
                baseURL = !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("cwBaseURL")) ? Environment.GetEnvironmentVariable("cwBaseURL") : "";
                consumerKey = !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("cwConsumerKey")) ? Environment.GetEnvironmentVariable("cwConsumerKey") : "";
                consumerSecret = !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("cwConsumerSecret")) ? Environment.GetEnvironmentVariable("cwConsumerSecret") : "";
                additionalVal = !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("cwClientID")) ? Environment.GetEnvironmentVariable("cwClientID") : "";
            }
            else if (apiName == "VeloCloud")
            {
                baseURL = !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("vcoBaseURL")) ? Environment.GetEnvironmentVariable("vcoBaseURL") : "";
                consumerKey = !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("vcoConsumerKey")) ? Environment.GetEnvironmentVariable("vcoConsumerKey") : "";
                consumerSecret = !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("vcoConsumerSecret")) ? Environment.GetEnvironmentVariable("vcoConsumerSecret") : "";
                additionalVal = !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("vcoLoginURL")) ? Environment.GetEnvironmentVariable("vcoLoginURL") : "";
            }

            List<string> apiInfo = new List<string>();
            apiInfo.Add(consumerKey);
            apiInfo.Add(consumerSecret);
            apiInfo.Add(baseURL);
            apiInfo.Add(additionalVal);

            return apiInfo;
        }

        /// <summary>
        /// Remove non-alphanumeric characters, truncate to 25 chars
        /// </summary>
        public static string MakeIdentifier(string name)
        {
            if (String.IsNullOrEmpty(name)) 
                name = "";
            
            string identifier = name;

            Regex rgx = new Regex("[^a-zA-Z0-9 ]"); // Company Identifier must be letter, numbers, and spaces only.
            identifier = rgx.Replace(identifier, "");
            identifier = identifier.Substring(0, Math.Min(25, identifier.Length));
            return identifier;
        }
    }
}
