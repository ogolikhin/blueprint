
using System;
using System.Collections.Generic;
using System.Net;

namespace Model
{
    public interface IAccessControl
    {
        List<ISession> Sessions { get; }

        void AuthorizeOperation(int userId, string operation = null, IArtifact artifact = null);    // PUT /sessions[/{op}[/{artifactId}]]

        ISession CreateSession(int userId,
            string username = null,
            DateTime? beginTime = null,
            DateTime? endTime = null,
            bool? isSso = null,
            int? licenseLevel = null,
            List<HttpStatusCode> expectedStatusCodes = null);

        ISession CreateSession(ISession session, List<HttpStatusCode> expectedStatusCodes = null);        // POST /sessions/{userId}

        void DeleteSession(ISession session, List<HttpStatusCode> expectedStatusCodes = null);   // DELETE /sessions

        ISession GetSession(int userId, uint pageSize = 0, uint pageNumber = 0);     // GET /sessions  or  GET /sessions/select?ps={ps}&pn={pn}
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
        HttpStatusCode GetStatus();                         // GET /status
    }
}
