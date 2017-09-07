using Newtonsoft.Json;

namespace ServiceLibrary.Models
{
    public class TenantInfo
    {
        public string TenantId { get; set; }

        public string TenantName { get; set; }

        public string PackageName { get; set; }
    }
}