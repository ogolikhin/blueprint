
using System;

namespace Model
{
    public interface ISession
    {
        // These properties are returned in the Response Body of a GET request.
        int UserId { get; }
        DateTime? BeginTime { get; }
        DateTime? EndTime { get; }
        string UserName { get; }
        bool IsSso { get; }
        int LicenseLevel { get; }

        /// <summary>
        /// The Session ID token is returned in the HTTP headers.
        /// </summary>
        string SessionId { get; }

        /// <summary>
        /// Tests whether the specified Session is equal to this one.
        /// </summary>
        /// <param name="session">The Session to compare.</param>
        /// <returns>True if the sessions are equal, otherwise false.</returns>
        bool Equals(ISession session);
    }
}
