using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AdminStore.Models
{
    public class SyncResult
    {
        public static SyncResult Empty => new SyncResult { TotalAdded = 0, TotalDeleted = 0 };

        public int TotalAdded { get; set; }
        public int TotalDeleted { get; set; }
    }
}