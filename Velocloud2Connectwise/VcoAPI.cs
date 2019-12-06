using System;
using System.Collections.Generic;
using RestSharp;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Velocloud2Connectwise
{
    class VcoAPI
    {
        private string username;
        private string password;
        private string baseURL;
        private string loginURL;

        public VcoAPI()
        {
            List<string> apiInfo = Helper.GetApiInfo("VeloCloud");
            username = apiInfo[0];
            password = apiInfo[1];
            baseURL = apiInfo[2];
            loginURL = apiInfo[3];  // description
        }

        /// <summary>
        /// Get velocloud.session value from the header to use in further API calls
        /// </summary>
        public string GetVcoSession()
        {
            var auth = new
            {
                username = username,
                password = password
            };
            var jsonBody = JsonConvert.SerializeObject(auth);

            string vcoSession = "";
            var client = new RestClient(loginURL);
            var request = new RestRequest(Method.POST);
            request.AddHeader("Content-Type", "application/json");
            request.AddParameter("application/json", jsonBody, ParameterType.RequestBody);
            var response = client.Execute(request);
            foreach (var item in response.Cookies){
                if (item.Name == "velocloud.session")
                    vcoSession = item.Value;
            }
            return vcoSession;
        }

        /// <summary>
        /// Get a list of VCO companies (name and address info)
        /// </summary>
        public List<List<string>> GetCompanies()
        {
            string vcoSession = GetVcoSession();
            string result = "";
            List<List<string>> companyInfo = new List<List<string>>();
            if (vcoSession != "")
            {
                var payload = new {
                    with = new List<string> {"edges"}
                };
                var jsonBody = JsonConvert.SerializeObject(payload);    // {"with":["edges"]}

                var client = new RestClient(baseURL);
                var request = new RestRequest("enterpriseProxy/getEnterpriseProxyEnterprises");
                request.AddHeader("Content-Type", "application/json");
                request.AddParameter("application/json", jsonBody, ParameterType.RequestBody);
                request.AddCookie("velocloud.session", vcoSession);
                var response = client.Post(request);
                var content = response.Content;

                if (content.IndexOf("error") > 0)
                    result = "VCO Session key has expired.";
                else
                {
                    JArray jsonArray = JArray.Parse(content);
                    for (int j = 0; j < jsonArray.Count; j++) {
                        string id = (string)jsonArray[j]["id"];
                        string name = (string)jsonArray[j]["name"]; // cw: name, identifier
                        string streetAddress = (string)jsonArray[j]["streetAddress"]; // cw: addressLine1
                        string streetAddress2 = (string)jsonArray[j]["streetAddress2"]; // cw: addressLine2
                        string city = (string)jsonArray[j]["city"]; // cw: city
                        string state = (string)jsonArray[j]["state"]; // cw: state
                        string postalCode = (string)jsonArray[j]["postalCode"]; // cw: zip
                        string country = (string)jsonArray[j]["country"]; // cw: {struct}
                        string accountNumber = (string)jsonArray[j]["accountNumber"]; // cw: accountNumber
                        string contactPhone = (string)jsonArray[j]["contactPhone"]; // cw: phoneNumber

                        companyInfo.Add(new List<string> { id, name, streetAddress, streetAddress2, city, state, postalCode, country, accountNumber , contactPhone });
                    }
                }
            }
            else{
                result = "VCO Session key is invalid.";
            }
            if (result != "")
                companyInfo.Add(new List<string> { "0", result });
            return companyInfo;
        }
    }
}
