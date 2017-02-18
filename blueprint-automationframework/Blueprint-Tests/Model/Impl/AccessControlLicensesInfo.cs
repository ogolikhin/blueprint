using Newtonsoft.Json;

namespace Model.Impl
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
