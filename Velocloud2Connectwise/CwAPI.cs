﻿using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;

namespace Velocloud2Connectwise
{
    class CwAPI
    {
        private string publicKey;
        private string privateKey;
        private string baseURL;
        private string clientId;
        private string auth;

        public CwAPI()
        {
            List<string> apiInfo = Helper.GetApiInfo("ConnectWise");
            publicKey = apiInfo[0];
            privateKey = apiInfo[1];
            baseURL = apiInfo[2];
            clientId = apiInfo[3];  // description
            auth = Base64Encode("Magna5+" + publicKey + ":" + privateKey);
        }

        /// <summary>
        /// Get a list of CW companies (identifier=>name)
        /// </summary>
        public List<KeyValuePair<string, string>> GetCompanies()
        {
            string info = "";
            //var companies = new List<string>();
            var companies = new List<KeyValuePair<string, string>>();

            if (clientId != "")
            {
                string pageSize = CountCompanies(); // 202

                var client = new RestClient(baseURL);
                var request = new RestRequest("company/companies?pageSize="+ pageSize);
                request.AddHeader("clientId", clientId);
                request.AddHeader("Authorization", "Basic " + auth);
                var response = client.Get(request);
                var content = response.Content;

                if (content.IndexOf("Unauthorized") >= 0)
                    info = "Incorrect public/private keys";
                else
                {
                    JArray jsonArray = JArray.Parse(content);
                    for (int j = 0; j < jsonArray.Count; j++)
                    {
                        string id = (string)jsonArray[j]["id"];
                        string name = (string)jsonArray[j]["name"];
                        string identifier = (string)jsonArray[j]["identifier"];
                        companies.Add(new KeyValuePair<string, string>(identifier, name));
                    }
                }
            }
            else
                info = "Keys not found.";

            if (info != "")
                companies.Add(new KeyValuePair<string, string>(info, ""));
            
            return companies;
        }

        /// <summary>
        /// Add a new company to CW
        /// </summary>
        public string PostCompany(string name, string addressLine1, string addressLine2, string city, string state, string zip, string accountNumber, string phoneNumber) {
            string info = "";
           
            if (clientId != "")
            {
                string identifier = Helper.MakeIdentifier(name);
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
        public string CountCompanies()
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
            return info;
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

        public static string Base64Encode(string plainText)
        {
            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(plainText);
            return System.Convert.ToBase64String(plainTextBytes);
        }
    }
}
