using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Model
{
    public class AccessControlLicensesInfo : IAccessControlLicensesInfo
    {
        [JsonProperty("LicenseLevel")]
        public int LicenseLevel { get; set; }
        [JsonProperty("Count")]
        public int Count { get; set; }

        public AccessControlLicensesInfo(int licenseLevel, int count)
        {
            LicenseLevel = licenseLevel;
            Count = count;
        }
    }
}
