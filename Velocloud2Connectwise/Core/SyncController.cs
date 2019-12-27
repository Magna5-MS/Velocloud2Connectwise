using System;
using System.Collections;
using System.Collections.Generic;
namespace Velocloud2Connectwise.Core
{
    public static class SyncController
    {
        public static void Execute(int action)
        {
            if (action == 1) // run the program
            {
                try
                {
                    Console.WriteLine("STARTING Velocloud to Connectwise Sync");
                    Helper h = new Helper();
                    h.SyncCompaniesObj();
                    Console.WriteLine("Velocloud to Connectwise Sync FINISHED");
                }
                catch (Exception e)
                {
                    Console.Write("ERROR in SyncCompaniesObj(): " + e.Message);
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
            }
            else if (action == 102) // print company's json by id
            {
                CwAPI cw = new CwAPI();
                Console.WriteLine(cw.GetCompany(19497));
            }
            else if (action == 200) // print a list of VCO companies
            {
                string id = "0", vcoName = "";
                VcoAPI vco = new VcoAPI();
                List<List<string>> vcoCompanies = vco.GetCompanies();
                foreach (List<string> thisVcoCompany in vcoCompanies)
                {
                    int i = 0;
                    foreach (string item in thisVcoCompany)
                    {
                        i++;
                        if (i == 1) id = item;
                        else if (i == 2) vcoName = item;
                    }
                    Console.WriteLine(vcoName);
                    Console.WriteLine(id);
                    Console.WriteLine("---------------------");
                }
            }
            return;
        }
    }
}
