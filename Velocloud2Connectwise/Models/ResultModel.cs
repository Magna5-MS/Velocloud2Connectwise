using System;
namespace Velocloud2Connectwise.Models
{
    public class SyncResult
    {
        public SyncResult()
        {
        }
        public int totalAccount { get; set; } = 0;
        public int totalUnmatched { get; set; } = 0;
        public int totalErred { get; set; } = 0;
    }
}
