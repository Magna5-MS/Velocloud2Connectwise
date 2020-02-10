using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Collections.Generic;
namespace Velocloud2Connectwise.Models.ConnectWise
{
    public class Company
    {
        public string id { get; set; }
        public string name { get; set; }
        public string identifier { get; set; }
        public Info _info { get; set; }
    }
    public class IdNameInfo
    {
        public int id { get; set; }
        public string name { get; set; }
        public Info _info { get; set; }
    }
    public class IdentifierNameInfo
    {
        public int id { get; set; }
        public string name { get; set; }
        public Info _info { get; set; }
    }
    public partial class Info
    {
        [JsonProperty("additionalProp1")]
        public string AdditionalProp1 { get; set; }

        [JsonProperty("additionalProp2")]
        public string AdditionalProp2 { get; set; }

        [JsonProperty("additionalProp3")]
        public string AdditionalProp3 { get; set; }
    }

    
    public class Configuration
    {
        public int? id { get; set; }
        public string name { get; set; }
        public IdNameInfo type { get; set; }
        public IdNameInfo status { get; set; }
        public Company company { get; set; }
        public IdNameInfo contact { get; set; }
        public IdNameInfo site { get; set; }
        public int? locationId { get; set; }
        public int? businessUnitId { get; set; }
        public string deviceIdentifier { get; set; }
        public string serialNumber { get; set; }
        public string modelNumber { get; set; }
        public string tagNumber { get; set; }
        public string purchaseDate { get; set; }
        public string installationDate { get; set; }
        public IdentifierNameInfo installedBy { get; set; }
        public string warrantyExpirationDate { get; set; }
        public string vendorNotes { get; set; }
        public string notes { get; set; }
        public string macAddress { get; set; }
        public string lastLoginName { get; set; }
        public bool? billFlag { get; set; }
        public int? backupSuccesses { get; set; }
        public int? backupIncomplete { get; set; }
        public int? backupFailed { get; set; }
        public int? backupRestores { get; set; }
        public string lastBackupDate { get; set; }
        public string backupServerName { get; set; }
        public int? backupBillableSpaceGb { get; set; }
        public string backupProtectedDeviceList { get; set; }
        public int? backupYear { get; set; }
        public int? backupMonth { get; set; }
        public string ipAddress { get; set; }
        public string defaultGateway { get; set; }
        public string osType { get; set; }
        public string osInfo { get; set; }
        public string cpuSpeed { get; set; }
        public string ram { get; set; }
        public string localHardDrives { get; set; }
        public int? parentConfigurationId { get; set; }
        public IdentifierNameInfo vendor { get; set; }
        public IdNameInfo manufacturer { get; set; }
        //public List<Question> questions { get; set; }
        public bool? activeFlag { get; set; }
        public string managementLink { get; set; }
        public string remoteLink { get; set; }
        public IdNameInfo sla { get; set; }
        public string mobileGuid { get; set; }
        public Info _info { get; set; }
        public bool? displayVendorFlag { get; set; }
        public int? companyLocationId { get; set; }
        public bool? showRemoteFlag { get; set; }
        public bool? showAutomateFlag { get; set; }
        public bool? needsRenewalFlag { get; set; }
        public string manufacturerPartNumber { get; set; }
        //public List<CustomField> customFields { get; set; }
    }
    public class Question
    {
        public int answerId { get; set; }
        public int questionId { get; set; }
        public string question { get; set; }
        public string answer { get; set; }
        public int sequenceNumber { get; set; }
        public NumberOfDecimals numberOfDecimals { get; set; }
        public string fieldType { get; set; }
        public bool requiredFlag { get; set; }
    }
    public class CustomField
    {
        public int id { get; set; }
        public string caption { get; set; }
        public string type { get; set; }
        public string entryMethod { get; set; }
        public int numberOfDecimals { get; set; }
        public string value { get; set; }
    }
    public class NumberOfDecimals
    {
        public bool hasValue { get; set; }
        public string value { get; set; }
    }
    public class Manufacturer
    {
        public int id { get; set; }
        public string name { get; set; }
        public bool inactiveFlag { get; set; }
    }
}
