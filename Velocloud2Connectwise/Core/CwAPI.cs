using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;
using System.Web;
using System.Net.Http.Headers;

using System.Linq;
using Velocloud2Connectwise.Models;

namespace Velocloud2Connectwise.ConnectWise
{
    public static class Endpoints
    {
        public static string GetCompanies(int pageSize, string conditions = "" )
        {
            return string.Format("company/companies?pageSize={0}&conditions={1}", pageSize, HttpUtility.UrlEncode(conditions));
        }
        public static string GetProductItems()
        {
            return "procurement/products";
        }
        public static string GetConfigurations(int pageSize, int page, string conditions = "")
        {
            return String.Format("company/configurations?pageSize={0}&page={1}&conditions={2}", pageSize, page, HttpUtility.UrlEncode(conditions));
        }
        public static string GetConfigurationTypes()
        {
            return "/company/configurations/types";
        }
        public static string AddConfiguration()
        {
            return "/company/configurations";
        }
        public static string GetManufacturers()
        {
            return "/procurement/manufacturers";
        }
    }

    class Api
    {
        private string publicKey;
        private string privateKey;
        private string baseURL;
        private string clientId;
        private string auth;

        public Api()
        {
            List<string> apiInfo = Sync.GetApiInfo("ConnectWise");
            publicKey = apiInfo[0];
            privateKey = apiInfo[1];
            baseURL = apiInfo[2];
            clientId = apiInfo[3];  // description
            auth = Base64Encode("Magna5+" + publicKey + ":" + privateKey);
        }
        public List<Models.ConnectWise.Manufacturer> GetManufacturers()
        {
            var client = new RestClient(baseURL);
            var request = new RestRequest(Endpoints.GetManufacturers());
            SetRestRequest(ref request, "");
            var response = client.Get<List<Models.ConnectWise.Manufacturer>>(request);
            return response.Data;
        }
        public List<Models.ConnectWise.Company> GetCompanies(string conditions)
        {
            int pageSize = CountCompanies();
            var client = new RestClient(baseURL);
            var request = new RestRequest(Endpoints.GetCompanies(pageSize, conditions));
            SetRestRequest(ref request, "");
            var response = client.Get<List<Models.ConnectWise.Company>>(request);
            return response.Data;
        }

        public List<Models.ConnectWise.IdNameInfo> GetConfigurationTypes()
        {
            var client = new RestClient(baseURL);
            var request = new RestRequest(Endpoints.GetConfigurationTypes());
            SetRestRequest(ref request, "");
            var response = client.Get<List<Models.ConnectWise.IdNameInfo>>(request);
            return response.Data;
        }
        public List<Models.ConnectWise.Configuration> GetConfigurations(string conditions)
        {
            var client = new RestClient(baseURL);
            List<Models.ConnectWise.Configuration> configs = new List<Models.ConnectWise.Configuration>();

            int page = 1;
            int pageSize = 500;
            bool getNextPage = true;
            while (getNextPage)
            {
                var request = new RestRequest(Endpoints.GetConfigurations(pageSize, page, conditions));
                SetRestRequest(ref request, "");
                var response = client.Get<List<Models.ConnectWise.Configuration>>(request);
                if (response.Data.Count < pageSize)
                    getNextPage = false;
                page += 1;
                configs.AddRange(response.Data);
            }
            return configs;
        }
        /// <summary>
        /// Get a list of cwCompany object (identifier=>name)
        /// </summary>
        public List<Models.ConnectWise.Company> GetCompaniesObject()
        {

            int pageSize = CountCompanies(); 
            var client = new RestClient(baseURL);
            var request = new RestRequest(Endpoints.GetCompanies(pageSize));
            SetRestRequest(ref request, "");
            var response = client.Get<List<Models.ConnectWise.Company>>(request);
            if (response.ErrorException != null)
            {
                throw new Exception(String.Format("Error in GetCompaniesObject(): {0}", response.ErrorMessage));
            }
            return response.Data;
        }
        public Models.ConnectWise.Configuration AddConfiguration(Models.ConnectWise.Configuration configuration)
        {
            var client = new RestClient(baseURL);
            var request = new RestRequest(Endpoints.AddConfiguration());
            var settings = new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore,
                MissingMemberHandling = MissingMemberHandling.Ignore
            };
            SetRestRequest(ref request, JsonConvert.SerializeObject(configuration, settings));
            var response = client.Post<Models.ConnectWise.Configuration>(request);
            if (response.ErrorException != null)
            {
                throw new Exception(String.Format("Error in AddConfiguration(): {0}", response.ErrorMessage));
            }
            return response.Data;
        }
        /// <summary>
        /// Add a new company to CW
        /// </summary>
        public string PostCompany(string name, string addressLine1, string addressLine2, string city, string state, string zip, string accountNumber, string phoneNumber) {
            string info = "";
           
            if (clientId != "")
            {
                string identifier = Sync.MakeIdentifier(name);
                var payload = new{
                    name = name,
                    identifier = identifier,
                    addressLine1 = addressLine1,
                    addressLine2 = addressLine2,
                    city = city,
                    state = state,
                    zip = zip,
                    accountNumber = accountNumber,
                    phoneNumber = phoneNumber
                };
                var jsonBody = JsonConvert.SerializeObject(payload);

                var client = new RestClient(baseURL);
                var request = new RestRequest("company/companies");
                request.AddHeader("clientId", clientId);
                request.AddHeader("Authorization", "Basic " + auth);
                request.AddHeader("Accept", "application/json");
                request.Method = Method.POST;
                request.AddParameter("application/json", jsonBody, ParameterType.RequestBody);
                var response = client.Execute(request);
                var content = response.Content; // json structure

                if (content.IndexOf("Unauthorized") >= 0)
                    info = "Incorrect public/private keys";
                else {
                    if (content.IndexOf("errors") > -1)
                    {
                        JObject jsonObject = JObject.Parse(content);
                        var errors = jsonObject["errors"];
                        JArray jsonArray = JArray.Parse(errors.ToString());
                        for (int j = 0; j < jsonArray.Count; j++)
                        {
                            string field = (string)jsonArray[j]["field"];
                            if (field == "state") // error "State Makati City not found" (country Philippines)
                                info = PostCompany(name, addressLine1, addressLine2, city, "", zip, accountNumber, phoneNumber);
                            else
                                info += "Error in " + field + ": " + jsonArray[j]["message"] + "\n";
                        }
                    }
                    else
                        info = content;
                }
            }
            else
                info = "Keys not found.";

            return info;
        }

        /// <summary>
        /// Get the number of existing companies
        /// </summary>
        public int CountCompanies()
        {
            string info = "";
            if (clientId != "")
            {
                var client = new RestClient(baseURL);
                var request = new RestRequest("company/companies/count");
                request.AddHeader("clientId", clientId);
                request.AddHeader("Authorization", "Basic " + auth);
                var response = client.Get(request);
                var content = response.Content;

                if (content.IndexOf("Unauthorized") >= 0)
                    info = "Incorrect public/private keys";
                else
                {
                    JObject jsonObject = JObject.Parse(content);
                    var count = (string)jsonObject["count"];
                    info = count;
                }
            }
            else
                info = "Keys not found.";
            return Convert.ToInt32(info);
        }

        /// <summary>
        /// Get company info by id
        /// </summary>
        public string GetCompany(int id)
        {
            string info = "";
            if (clientId != "")
            {
                var client = new RestClient(baseURL);
                var request = new RestRequest("company/companies/" + id.ToString());
                request.AddHeader("clientId", clientId);
                request.AddHeader("Authorization", "Basic " + auth);
                var response = client.Get(request);
                var content = response.Content;

                if (content.IndexOf("Unauthorized") >= 0)
                    info = "Incorrect public/private keys";
                else
                {
                    info = content;
                }
            }
            else
                info = "Keys not found.";

            return info;
        }

        public void SetRestRequest(ref RestRequest request, string jsonBody)
        {
            request.AddHeader("clientId", clientId);
            request.AddHeader("Authorization", "Basic " + auth);
            request.AddHeader("Accept", "application/json");
            if (jsonBody != "")
                request.AddParameter("application/json", jsonBody, ParameterType.RequestBody);
        }

        public static string Base64Encode(string plainText)
        {
            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(plainText);
            return System.Convert.ToBase64String(plainTextBytes);
        }
    }
}
