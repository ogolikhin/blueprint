﻿using System.Collections.Generic;
using System.Net;
using Utilities.Facades;

namespace Model
{
    public interface IBlueprintServer
    {
        string Address { get; set; }


        /// <summary>
        /// Login to the Blueprint server using the Basic authorization method.
        /// </summary>
        /// <param name="user">The user whose credentials will be used to login.</param>
        /// <param name="token">(optional) The user token to use for the request.  By default, if null was passed, we get a valid token for the user.
        /// If you don't want to use a token, you should pass an empty string here.</param>
        /// <param name="maxRetries">(optional) The maximum number of times to retry the login in case we get socket timeouts.</param>
        /// <returns>The RestResponse received from the server.</returns>
        RestResponse LoginUsingBasicAuthorization(IUser user, string token = null, uint maxRetries = 5);

        /// <summary>
        /// Checks if the Blueprint services are ready for operation.
        /// (Runs: GET /status)
        /// </summary>
        /// <param name="preAuthorizedKey">(optional) The pre-authorized key to use for authentication.  Defaults to a valid key.</param>
        /// <param name="expectedStatusCodes">(optional) Expected status codes for the request.  By default only 200 OK is expected.</param>
        /// <returns>A JSON structure containing the status of this service and its dependent services.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
        string GetStatus(string preAuthorizedKey = CommonConstants.PreAuthorizedKeyForStatus, List<HttpStatusCode> expectedStatusCodes = null);
    }
}
