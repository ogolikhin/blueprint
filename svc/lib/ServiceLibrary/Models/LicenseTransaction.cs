using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace ServiceLibrary.Models
{
    [JsonObject]
    public class LicenseTransaction
    {
        // Populated from LicenseActivities table

        public int LicenseActivityId { get; set; }
        public int UserId { get; set; }
        public int LicenseType { get; set; }
        public int TransactionType { get; set; }
        public int ActionType { get; set; }
        public int ConsumerType { get; set; }
        public DateTime Date { get; set; }

        // Populated from LicenseActivitiyDetails Table

        public IDictionary<int, int> ActiveLicenses { get; set; }

        /// <summary>
        /// Sets ActiveLicenses from a string of the form "a:x;b:y;c:z" where a, b, c are license levels
        /// and x, y, z are counts.
        /// </summary>
        [JsonIgnore]
        public string Details
        {
            set
            {
                ActiveLicenses = value.Split(';')
                    .Where(pair => pair.Length != 0)
                    .Select(pair => pair.Split(':').Select(int.Parse).Take(2).ToArray())
                    .ToDictionary(pair => pair[0], pair => pair[1]);
            }
        }

        // Added by LicenseController

        public string Username { get; set; }
        public string Department { get; set; }
    }
}
