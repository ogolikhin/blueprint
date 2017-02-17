using System;
using Newtonsoft.Json;

namespace Model.Impl
{
    public class LicenseActivity: ILicenseActivity
    {
        #region Properties
        [JsonProperty("LicenseActivityId")]
        public int LicenseActivityId { get; set; }

        [JsonProperty("UserId")]
        public int UserId { get; set; }

        [JsonProperty("LicenseType")]
        public int LicenseType { get; set; }

        [JsonProperty("TransactionType")]
        public int TransactionType { get; set; }

        [JsonProperty("ActionType")]
        public int ActionType { get; set; }

        [JsonProperty("ConsumerType")]
        public int ConsumerType { get; set; }

        [JsonProperty("Date")]
        public DateTime Date { get; set; }
        /// TODO: add ActiveLicenses
        [JsonProperty("Username")]
        public string Username { get; set; }

        [JsonProperty("Department")]
        public string Department { get; set; }
        #endregion Properties
        public LicenseActivity()
        { }

        public LicenseActivity(int licenseActivityId, int userId, int licenseType, int transactionType, int actionType, int consumerType,
            DateTime date, string username, string department)
        {
            LicenseActivityId = licenseActivityId;
            UserId = userId;
            LicenseType = licenseType;
            TransactionType = transactionType;
            ActionType = actionType;
            ConsumerType = consumerType;
            Date = date;
            Username = username;
            Department = department;
        }

    }
}
