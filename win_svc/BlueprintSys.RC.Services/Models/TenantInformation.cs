using System;

namespace BlueprintSys.RC.Services.Models
{
    public class TenantInformation
    {
        public string TenantId { get; set; }

        public string TenantName { get; set; }

        public string BlueprintConnectionString { get; set; }

        public int PackageLevel { get; set; }

        public string PackageName { get; set; }

        public DateTime? StartDate { get; set; }

        public DateTime? ExpirationDate { get; set; }

        public string AdminStoreLog { get; set; }
    }
}
