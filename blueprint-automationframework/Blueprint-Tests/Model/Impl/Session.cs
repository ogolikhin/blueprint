﻿
using System;
using Newtonsoft.Json;

namespace Model.Impl
{
    public class Session : ISession
    {
        // These properties are returned in the Response Body of a GET request.
        [JsonProperty("UserId")]
        public int UserId { get; set; }
        [JsonProperty("UserName")]
        public string UserName { get; set; }
        [JsonProperty("IsSso")]
        public bool IsSso { get; set; }
        [JsonProperty("LicenseLevel")]
        public int LicenseLevel { get; set; }

        public DateTime? BeginTime { get; set; }
        public DateTime? EndTime { get; set; }

        /// <summary>
        /// The Session ID token is returned in the HTTP headers.
        /// </summary>
        public string SessionId { get; set; }

        public Session()
        { }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="userId">The User ID.</param>
        /// <param name="userName">The Username.</param>
        /// <param name="licenseLevel">The license level</param>  // TODO: Should this be an enum?
        /// <param name="isSso"></param>  // TODO: What is this for?
        /// <param name="sessionId">(optional) This is the session token.</param>
        /// <param name="beginTime">(optional) </param>     // TODO: What is this for?
        /// <param name="endTime">(optional) </param>       // TODO: What is this for?
        public Session(int userId, string userName, int licenseLevel, bool isSso,
            string sessionId = null, DateTime? beginTime = null, DateTime? endTime = null)
        {
            UserId = userId;
            UserName = userName;
            LicenseLevel = licenseLevel;
            IsSso = isSso;
            SessionId = sessionId;
            BeginTime = beginTime;
            EndTime = endTime;
        }

        /// <summary>
        /// Tests whether the specified Session is equal to this one.
        /// </summary>
        /// <param name="session">The Session to compare.</param>
        /// <returns>True if the sessions are equal, otherwise false.</returns>
        public bool Equals(ISession session)
        {
            if (session == null)
            {
                return false;
            }

            return (this.UserId == session.UserId) && (this.UserName == session.UserName) &&
                (this.LicenseLevel == session.LicenseLevel) && (this.IsSso == session.IsSso);
        }
    }
}