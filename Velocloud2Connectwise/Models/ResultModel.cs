using System;
namespace Velocloud2Connectwise.Models
{
    public class SyncResult
    {
        public SyncResult()
        {
        }
        public string syncType { get; set; } // customer/inventory
        public int totalRecords { get; set; } = 0;
        public int totalUnmatched { get; set; } = 0;
        public int totalErred { get; set; } = 0;
        public int totalSync { get; set; } = 0;
    }
}
