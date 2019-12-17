using System;
using System.Collections.Generic;
using Magna5.Utilities;

namespace Velocloud2Connectwise
{
    class Program
    {
        static void Main(string[] args)
        {
            int action = 1; // debug modes

            if (action == 1) // run the program
            {
                try
                {
                    Database.ExecuteNonQuery("INSERT INTO PRODUCTION.dbo.applicationLog(appName, appId, note, d, error) VALUES('VCO2CW', 'VCO2CW', 'Start Syncing', GETDATE(), 0)", "cnnProduction");
                    Helper h = new Helper();
                    h.SyncCompanies();
                    Database.ExecuteNonQuery("INSERT INTO PRODUCTION.dbo.applicationLog(appName, appId, note, d, error) VALUES('VCO2CW', 'VCO2CW', 'End Syncing', GETDATE(), 0)", "cnnProduction");
                    //Console.ReadLine();
                }
                catch (Exception e)
                {
                    string msg = e.ToString().Replace("'", "");
                    //Console.WriteLine(msg);
                    //Console.ReadLine();
                    Database.ExecuteNonQuery("INSERT INTO PRODUCTION.dbo.applicationLog(appName, appId, note, d, error) VALUES('VCO2CW', 'VCO2CW', '" + msg + "', GETDATE(), 1)", "cnnProduction");
                }
            }
            else if (action == 100) // print a list of CW companies
            {
                CwAPI cw = new CwAPI();
                List<KeyValuePair<string, string>> cwCompanies = cw.GetCompanies();
                foreach (KeyValuePair<string, string> cwCompany in cwCompanies)
                    Console.WriteLine(cwCompany.Value + " // " + cwCompany.Key);
            }
            else if (action == 101) // count CW companies
            {
                CwAPI cw = new CwAPI();
                Console.WriteLine(cw.CountCompanies());
                Console.ReadLine();
            }
            else if (action == 102) // print company's json by id
            {
                CwAPI cw = new CwAPI();
                Console.WriteLine(cw.GetCompany(19497));
                Console.ReadLine();
            }
            else if (action == 200) // print a list of VCO companies
            {
                string id = "0", vcoName = "";
                VcoAPI vco = new VcoAPI();
                List<List<string>> vcoCompanies = vco.GetCompanies();
                foreach (List<string> thisVcoCompany in vcoCompanies) {
                    int i = 0;
                    foreach (string item in thisVcoCompany) {
                        i++;
                        if (i == 1) id = item;
                        else if (i == 2) vcoName = item;
                    }
                    Console.WriteLine(vcoName);
                    Console.WriteLine(id);
                    Console.WriteLine("---------------------");
                }
            }
        }
    }
}
