using Newtonsoft.Json;
using System;

namespace ServiceLibrary.Models
{
    public class Session
    {
        /// <summary>
        /// The ID of the user of this Session.
        /// </summary>
        public int UserId { get; set; }

        /// <summary>
        /// The ID of this Session.
        /// </summary>
        [JsonIgnore] //do not send session id
        public Guid SessionId { get; set; }

        /// <summary>
        /// The time in UTC that this Session began, or Null if this Session is no longer valid.
        /// </summary>
        public DateTime? BeginTime { get; set; }

        /// <summary>
        /// The time in UTC that this Session ended, or will expire.
        /// </summary>
        public DateTime EndTime { get; set; }

        /// <summary>
        /// UserName
        /// </summary>
        public string UserName { get; set; }

        /// <summary>
        /// The name of the user of this Session.
        /// </summary>
        public bool IsSso { get; set; }

        /// <summary>
        /// The level of license of the user of this Session.
        /// </summary>
        public int LicenseLevel { get; set; }

        public bool IsExpired()
        {
            return EndTime <= DateTime.UtcNow;
        }

        public static string Convert(Guid guid)
        {
            return guid.ToString(("N"));
        }

        public static Guid Convert(string val)
        {
            return Guid.ParseExact(val, "N");
        }
    }
}
