using System;
using System.Collections.Generic;
using RestSharp;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Velocloud2Connectwise.Models;

namespace Velocloud2Connectwise.Velocloud
{
    public static class Endpoints
    {
        public static string GetEnterpriseProxyEnterprises()
        {
            return "enterpriseProxy/getEnterpriseProxyEnterprises";
        }
        public static string GetEnterpriseProxyEdgeInventory()
        {
            return "enterpriseProxy/getEnterpriseProxyEdgeInventory";
        }
        public static string GetInventoryItems()
        {
            return "vcoInventory/getInventoryItems";
        }
        public static string GetEnterpriseEdge()
        {
            return "enterprise/getEnterpriseEdges";
        }
        public static string GetEnterprises()
        {
            return "/enterprise/getEnterprise";
        }
    }
    class Api
    {
        public string session = "";
        private string username;
        private string password;
        private string baseURL;
        private string loginURL;

        public Api()
        {
            List<string> apiInfo = Sync.GetApiInfo("VeloCloud");
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

            RestClient client = new RestClient(loginURL);
            RestRequest request = new RestRequest(Method.POST);
            request.AddHeader("Content-Type", "application/json");
            request.AddParameter("application/json", jsonBody, ParameterType.RequestBody);
            var response = client.Execute(request);
            foreach (var item in response.Cookies){
                if (item.Name == "velocloud.session")
                    this.session = item.Value;
            }
            return this.session;
        }

        /// <summary>
        /// Get partner enterprises and their edge inventory.
        /// </summary>
        /// <returns></returns>
        public List<Models.Velocloud.EnterpriseProxyEdgeInventory> GetEnterpriseProxyEdgeInventory()
        {
            var client = new RestClient(baseURL);
            var request = new RestRequest(Endpoints.GetEnterpriseProxyEdgeInventory(), Method.POST);
            SetRestRequest(ref request, string.Empty);
            var response = client.Execute<List<Models.Velocloud.EnterpriseProxyEdgeInventory>>(request);
            if (!response.IsSuccessful)
                throw new Exception("Velocloud api error in GetEnterpriseProxyEdgeInventory(): (" + response.StatusCode + ") " + response.Content);
            return response.Data;
        }
        /// <summary>
        /// Get inventory for specified enterprise
        /// </summary>
        /// <returns></returns>
        public List<Models.Velocloud.EnterpriseEdge> GetEnterpriseEdge(int enterpriseId)
        {
            var payload = new
            {
                enterpriseId = enterpriseId,
                with = new List<string> { "links" }
            };
            var jsonBody = JsonConvert.SerializeObject(payload);
            var client = new RestClient(baseURL);
            var request = new RestRequest(Endpoints.GetEnterpriseEdge(), Method.POST);
            SetRestRequest(ref request, jsonBody);
            var response = client.Execute<List<Models.Velocloud.EnterpriseEdge>>(request);
            if (!response.IsSuccessful)
                throw new Exception("Velocloud api error in GetEnterpriseEdge(): (" + response.StatusCode + ") " + response.Content);
            return response.Data;
        }
        /// <summary>
        /// Get list of all enterprises
        /// </summary>
        /// <returns></returns>
        public List<Models.Velocloud.Enterprise> GetEnterprises()
        {
            var payload = new
            {
                enterpriseId = 0
            };
            var jsonBody = JsonConvert.SerializeObject(payload);
            var client = new RestClient(baseURL);
            var request = new RestRequest(Endpoints.GetEnterpriseEdge(), Method.POST);
            SetRestRequest(ref request, jsonBody);
            var response = client.Execute<List<Models.Velocloud.Enterprise>>(request);
            if (!response.IsSuccessful)
                throw new Exception("Velocloud api error in GetEnterprises(): (" + response.StatusCode + ") " + response.Content);
            return response.Data;
        }
        /// <summary>
        /// **** CALL FAILS - Permission denied for this request, using GetEnterpriseProxyEdgeInventory instead *****
        /// Retrieve all the inventory information available with this VCO. This method does not have required parameters. The optional parameters are
        /// enterpriseId - Return inventory items belonging to that enterprise.If the caller context is an enterprise, this value will be taken from token itself.
        /// modifiedSince - Used to retrieve inventory items that have been modified in the last modifiedSince hours.
        /// with - an array containing the string "edge" to get details about details about the provisioned edge if any.
        /// </summary>
        /// <returns></returns>
        public List<Models.Velocloud.Inventory> GetInventoryItems()
        {
            var payload = new
            {
                with = new List<string> { "edge" }
            };
            var jsonBody = JsonConvert.SerializeObject(payload);    // {"with":["edge"]}
            var client = new RestClient(baseURL);
            var request = new RestRequest(Endpoints.GetInventoryItems(), Method.POST);
            SetRestRequest(ref request, jsonBody);
            var response = client.Execute<List<Models.Velocloud.Inventory>>(request);
            if (!response.IsSuccessful)
                throw new Exception("Velocloud api error in GetInventoryItems(): (" + response.StatusCode + ") " + response.Content);
            return response.Data;
        }
        /// <summary>
        /// Retrieves list of companies via the Velocloud API
        /// </summary>
        /// <returns>List<Models.Velocloud.Company></returns>
        public List<Models.Velocloud.Company> GetCompaniesObject()
        {
            var payload = new
            {
                with = new List<string> { "edges" }
            };
            var jsonBody = JsonConvert.SerializeObject(payload);    // {"with":["edges"]}
            var client = new RestClient(baseURL);
            var request = new RestRequest(Endpoints.GetEnterpriseProxyEnterprises(), Method.POST);
            SetRestRequest(ref request, jsonBody);
            var response = client.Execute<List<Models.Velocloud.Company>>(request);
            if (!response.IsSuccessful)
                throw new Exception("Velocloud api error in GetCompaniesObject(): (" + response.StatusCode + ") " + response.Content);
            if (response.Data.Count == 1 && response.Data[0].error != null)
            {
                throw new Exception("Velocloud api error in GetCompaniesObject(): " + response.Data[0].error.message);
            }
            return response.Data;
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
                if (!response.IsSuccessful)
                    throw new Exception("Velocloud api error in GetCompanies(): (" + response.StatusCode + ") " + response.Content);
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

        public void SetRestRequest(ref RestRequest request, string jsonBody)
        {
            if (this.session == "") GetVcoSession();
            request.AddHeader("Content-Type", "application/json");
            request.AddCookie("velocloud.session", this.session);
            if (jsonBody != "")
                request.AddParameter("application/json", jsonBody, ParameterType.RequestBody);
        }
    }
}
