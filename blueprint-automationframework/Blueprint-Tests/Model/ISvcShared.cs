using System;
using System.Collections.Generic;
using System.Net;
using Model.ArtifactModel.Impl;

namespace Model
{
    public interface ISvcShared : IDisposable
    {
        /// <summary>
        /// Searches for users or groups that match the search criteria.
        /// </summary>
        /// <param name="user">The user to authenticate the REST call.</param>
        /// <param name="search">(optional) The sub-string to search for in the display name or E-mail address of the user or group.</param>
        /// <param name="allowEmptyEmail">(optional) If false, do not return users and groups with an empty E-mail address.  Default (null) = false.</param>
        /// <param name="limit">(optional) The max number of results to return.  Default (null) = 5.  The min is 1 and max is 500.</param>
        /// <param name="includeGuests">(optional) If false, do not return guests.  Default (null) = true.</param>
        /// <param name="expectedStatusCodes">(optional) Expected status codes for the request.  By default only 200 OK is expected.</param>
        /// <returns>The list of UserOrGroupInfo that was returned.</returns>
        List<UserOrGroupInfo> FindUserOrGroup(IUser user,
            string search = null,
            bool? allowEmptyEmail = null,
            int? limit = null,
            bool? includeGuests = null,
            List<HttpStatusCode> expectedStatusCodes = null);
    }
}
