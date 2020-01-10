using System;
using System.Collections.Generic;
using System.Configuration;
using System.Text.RegularExpressions;
using Magna5.Utilities;
using Velocloud2Connectwise.Models;
namespace Velocloud2Connectwise
{
    class Helper
    {
        public SyncResult syncResults { get; set; }
        /// <summary>
        /// Get VCO and CW companies as objects, add missing ones to CW
        /// </summary>
        public void SyncCompaniesObj()
        {
            syncResults = new SyncResult();
            CwAPI cw = new CwAPI();
            VcoAPI vco = new VcoAPI();
            List<VcoCompany> lstVcoCompanies;
            List<CwCompany> lstCwCompanies;

            try
            {
                lstVcoCompanies = vco.GetCompaniesObject();
            }
            catch(Exception ex)
            {
                throw new Exception("Error retrieving velocloud companies: " + ex.Message);
            }

            try
            {
                lstCwCompanies = cw.GetCompaniesObject();
            }            
            catch(Exception ex)
            {
                throw new Exception("Error retrieving connectwise companies: " + ex.Message);
            }

            List<CwCompany> lstMatches = null;
            
            foreach (VcoCompany company in lstVcoCompanies)
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

            List<VcoCompany> lstUnmatched = null;
            lstUnmatched = lstVcoCompanies.FindAll(i => i.containsMatch == false);

            syncResults.totalAccount = lstVcoCompanies.Count;
            syncResults.totalUnmatched = lstUnmatched.Count;
            

            // Send email report of all unmatched companies
            if (syncResults.totalUnmatched > 0)
            {
                string emailBody = "The following VeloCloud companies could not be found in ConnectWise. Please verify the name and that the company exists in ConnectWise.<br /><br />";
                foreach (VcoCompany company in lstUnmatched)
                {
                    emailBody += company.name + " (" + company.id + ")<br />";
                }
                try
                {
                    Magna5.Utilities.Mail.SendEmail(Environment.GetEnvironmentVariable("emailReportTo"), "donotreply@magna5global.com", "VeloCloud - New Accounts Synced", emailBody, null, null, true);
                }
                catch (Exception ex)
                {
                    throw new Exception("Error sending synced account email: " + ex.Message);
                }

                Console.WriteLine(lstUnmatched.Count.ToString() + " unmatched compan" + (lstUnmatched.Count == 1 ? "y" : "ies") + " in VeloCloud that are not in ConnectWise.");
            }
        
        }
        /// <summary>
        /// Get VCO and CW companies, add missing ones to CW
        /// </summary>
        public void SyncCompanies()
        {
            
            // Get CW companies
            CwAPI cw = new CwAPI();
            List<KeyValuePair<string, string>> cwCompanies = cw.GetCompanies();

            // Get VCO companies to compare them with CW ones
            string id = "0", vcoName = "", streetAddress = "", streetAddress2 = "", city = "", state = "", postalCode = "", country = "", accountNumber = "", contactPhone = "";
            int added = 0;
            int j = 0;
            VcoAPI vco = new VcoAPI();
            List<List<string>> vcoCompanies = vco.GetCompanies();   // Get VCO companies
            foreach (List<string> thisVcoCompany in vcoCompanies)
            {
                int i = 0;
                foreach (string item in thisVcoCompany)
                {
                    i++;
                    if (i == 1) id = item;
                    else if (i == 2) vcoName = item;
                    if (id == "0")
                    {
                        Console.WriteLine(vcoName); // error msg
                    }
                    else
                    {
                        if (i == 3) streetAddress = item;
                        else if (i == 4) streetAddress2 = item;
                        else if (i == 5) city = item;
                        else if (i == 6) state = item;
                        else if (i == 7) postalCode = item;
                        else if (i == 8) country = item;
                        else if (i == 9) accountNumber = item;
                        else if (i == 10) contactPhone = item;
                    }
                }

                j++;
                Console.WriteLine(j.ToString() + ". " + vcoName);
                Console.WriteLine("VcoID: "+id);
                Console.WriteLine("Address: "+streetAddress + " " + streetAddress2 + " " + city + " " + state + " " + postalCode + " " + country);
                Console.WriteLine("AccNum: "+accountNumber);
                Console.WriteLine("PhoneNum: "+contactPhone);

                // compare CW company names with VCO company names
                string vcoNameIdent = MakeIdentifier(vcoName);
                int v = 0;
                foreach (KeyValuePair<string, string> cwCompany in cwCompanies)
                {
                    string cwIdent = cwCompany.Key;
                    string cwName = cwCompany.Value;
                    if (cwName == vcoName || cwIdent == vcoNameIdent) { 
                        Console.WriteLine("VCO company found in CW.");
                        v = 1;
                        break;
                    }
                }

                if (v == 0) {
                    Console.WriteLine("VCO company not found in CW, let's use CW API to POST it there.");
                    string cwPost = cw.PostCompany(vcoName, streetAddress, streetAddress2, city, state, postalCode, accountNumber, contactPhone);
                    Console.WriteLine(cwPost);
                    added++;
                }
                Console.WriteLine("----------------------------"); // vco companies separator
            }
            Console.WriteLine(added.ToString() + " new compan"+(added==1 ? "y" : "ies") + " added to CW.");
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
            string identifier = name;
            Regex rgx = new Regex("[^a-zA-Z0-9 ]"); // Company Identifier must be letter, numbers, and spaces only.
            identifier = rgx.Replace(identifier, "");
            identifier = identifier.Substring(0, Math.Min(25, identifier.Length));
            return identifier;
        }
    }
}
