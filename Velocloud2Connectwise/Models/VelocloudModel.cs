using System;
using System.Collections.Generic;
namespace Velocloud2Connectwise.Models.Velocloud
{
    public class Company
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
    public class Inventory
    {
        public int id { get; set; }
        public string deviceSerialNumber { get; set; }
        public string deviceUuid { get; set; }
        public string modelNumber { get; set; }
        public int siteId { get; set; }
        public string description { get; set; }
        public int acknowledged { get; set; }
        public int edgeId { get; set; }
        public InventoryEdge edge { get; set; }
        public string inventoryState{get; set;}
        public string inventoryEdgeState { get; set; }
        public string inventoryAction { get; set; }
        public int vcoOwnerId { get; set; }
        public Owner vcoOwner { get; set; }
        public string modified { get; set; }
    }
    public class InventoryEdge
    {
        public int id { get; set; }
        public string edge { get; set; }
        public InventoryEdgeSite site { get; set; }
    }
    public class InventoryEdgeSite
    {
        public string name { get; set; }
    }
    public class Owner
    {
        public string accountNumber { get; set; }
        public string name { get; set; }
    }
    public class EnterpriseProxyEdgeInventory
    {
        public string enterpriseName { get; set; }
        public int enterpriseId { get; set; }
        public string edgeName { get; set; }
        public int edgeId { get; set; }
        public DateTime created { get; set; }
        public string edgeState { get; set; }
        public string serialNumber { get; set; }
        public string haSerialNumber { get; set; }
        public string activationState { get; set; }
        public DateTime activationTime { get; set; }
        public string modelNumber { get; set; }
        public string softwareVersion { get; set; }
        public DateTime softwareUpdated { get; set; }
        public DateTime lastContact { get; set; }
    }

    #region "Enterprise Edge Detail"
    public class EnterpriseEdge
    {
        public string activationKey { get; set; }
        public string activationKeyExpires { get; set; }
        public string activationState { get; set; }
        public string activationTime { get; set; }
        public int alertsEnabled { get; set; }
        public string buildNumber { get; set; }
        public string created { get; set; }
        public string description { get; set; }
        public string deviceFamily { get; set; }
        public string deviceId { get; set; }
        public string dnsName { get; set; }
        public string edgeHardwareId { get; set; }
        public string edgeState { get; set; }
        public string edgeStateTime { get; set; }
        public string endpointPkiMode { get; set; }
        public int enterpriseId { get; set; }
        public string haLastContact { get; set; }
        public string haPreviousState { get; set; }
        public string haSerialNumber { get; set; }
        public string haState { get; set; }
        public int id { get; set; }
        public int isLive { get; set; }
        public string lastContact { get; set; }
        public string logicalId { get; set; }
        public string modelNumber { get; set; }
        public string modified { get; set; }
        public string name { get; set; }
        public int operatorAlertsEnabled { get; set; }
        public string selfMacAddress { get; set; }
        public string serialNumber { get; set; }
        public string serviceState { get; set; }
        public string serviceUpSince { get; set; }
        public int siteId { get; set; }
        public string softwareUpdated { get; set; }
        public string softwareVersion { get; set; }
        public string systemUpSince { get; set; }
        public List<Link> links { get; set; }
        public List<Link> recentLinks { get; set; }
        public Site site { get; set; }
    }
    public class Link
    {
        public int id { get; set; }
        public DateTime created { get; set; }
        public int edgeId { get; set; }
        public string logicalId { get; set; }
        public string internalId { get; set; }
        public string @interface { get; set; }
        public string macAddress { get; set; }
        public string ipAddress { get; set; }
        public string netmask { get; set; }
        public string networkSide { get; set; }
        public string networkType { get; set; }
        public string displayName { get; set; }
        public string isp { get; set; }
        public string org { get; set; }
        public int lat { get; set; }
        public int lon { get; set; }
        public DateTime lastActive { get; set; }
        public string state { get; set; }
        public string backupState { get; set; }
        public string vpnState { get; set; }
        public DateTime lastEvent { get; set; }
        public string lastEventState { get; set; }
        public int alertsEnabled { get; set; }
        public int operatorAlertsEnabled { get; set; }
        public string serviceState { get; set; }
        public DateTime modified { get; set; }
        public List<string> serviceGroups { get; set; }
    }

    public class Site
    {
        public string city { get; set; }
        public string contactEmail { get; set; }
        public string contactMobile { get; set; }
        public string contactName { get; set; }
        public string contactPhone { get; set; }
        public string country { get; set; }
        public int lat { get; set; }
        public int lon { get; set; }
        public string name { get; set; }
        public string postalCode { get; set; }
        public string state { get; set; }
        public string streetAddress { get; set; }
        public string streetAddress2 { get; set; }
    }
    public class Enterprise
    {
        public int id { get; set; }
        public DateTime created { get; set; }
        public int networkId { get; set; }
        public int gatewayPoolId { get; set; }
        public bool alertsEnabled { get; set; }
        public bool operatorAlertsEnabled { get; set; }
        public string endpointPkiMode { get; set; }
        public string name { get; set; }
        public string domain { get; set; }
        public string prefix { get; set; }
        public string logicalId { get; set; }
        public string accountNumber { get; set; }
        public string description { get; set; }
        public string contactName { get; set; }
        public string contactPhone { get; set; }
        public string contactMobile { get; set; }
        public string contactEmail { get; set; }
        public string streetAddress { get; set; }
        public string streetAddress2 { get; set; }
        public string city { get; set; }
        public string state { get; set; }
        public string postalCode { get; set; }
        public string country { get; set; }
        public int lat { get; set; }
        public int lon { get; set; }
        public string timezone { get; set; }
        public string locale { get; set; }
        public DateTime modified { get; set; }
    }

    #endregion
}
