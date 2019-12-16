using System;
namespace Velocloud2Connectwise.Models
{
    public class VcoCompany
    {
        public string id { get; set; }
        public string name { get; set; }
        public string streetAddress { get; set; }
        public string streetAddress2 { get; set; }
        public string city { get; set; }
        public string state { get; set; }
        public string postalCode { get; set; }
        public string country { get; set; }
        public string accountNumber { get; set; }
        public string contactPhone { get; set; }
        public bool containsMatch { get; set; } = true;
    }
    public class CwCompany
    {
        public string id { get; set; }
        public string name { get; set; }
        public string identifier { get; set; }
    }
}
