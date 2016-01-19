using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace ServiceLibrary.Models
{
    [JsonObject]
    public class LicenseTransaction
    {
        /// <summary>
        /// The ID of this license transaction
        /// </summary>
        /// <remarks>
        /// Populated from LicenseActivities Table.
        /// </remarks>
        public int LicenseActivityId { get; set; }

        /// <summary>
        /// The ID of the user for this license transaction.
        /// </summary>
        /// <remarks>
        /// Populated from LicenseActivities Table.
        /// </remarks>
        public int UserId { get; set; }

        /// <summary>
        /// The type of license of the user for this license transaction.
        /// </summary>
        /// <remarks>
        /// Populated from LicenseActivities Table.
        /// </remarks>
        public int LicenseType { get; set; }

        /// <summary>
        /// The type of license transaction.
        /// </summary>
        /// <remarks>
        /// Populated from LicenseActivities Table.
        /// </remarks>
        public int TransactionType { get; set; }

        /// <summary>
        /// The action that prompted this license transaction.
        /// </summary>
        /// <remarks>
        /// Populated from LicenseActivities Table.
        /// </remarks>
        public int ActionType { get; set; }

        /// <summary>
        /// The type of the consumer of this license transaction.
        /// </summary>
        /// <remarks>
        /// Populated from LicenseActivities Table.
        /// </remarks>
        public int ConsumerType { get; set; }

        /// <summary>
        /// The time in UTC that this transaction occurred.
        /// </summary>
        /// <remarks>
        /// Populated from LicenseActivities Table.
        /// </remarks>
        public DateTime Date { get; set; }

        /// <summary>
        /// The current active licenses after this license transaction.
        /// </summary>
        /// <remarks>
        /// Populated from LicenseActivitiyDetails Table.
        /// </remarks>
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

        /// <summary>
        /// The name of the user for this license transaction.
        /// </summary>
        /// <remarks>
        /// Added by LicenseController.
        /// </remarks>
        public string Username { get; set; }
        /// <summary>
        /// The department of the user for this license transaction.
        /// </summary>
        /// <remarks>
        /// Added by LicenseController.
        /// </remarks>
        public string Department { get; set; }
    }
}
