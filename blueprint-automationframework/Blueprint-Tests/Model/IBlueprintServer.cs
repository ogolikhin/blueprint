using System.Collections.Generic;
using System.Net;

namespace Model
{
    public interface IBlueprintServer
    {
        string Address { get; set; }


        /// <summary>
        /// Login to the Blueprint server using the Basic authorization method.
        /// </summary>
        /// <param name="user">The user whose credentials will be used to login.</param>
        /// <param name="maxRetries">(optional) The maximum number of times to retry the login in case we get socket timeouts.</param>
        /// <returns>The HttpWebResponse received from the server.</returns>
        HttpWebResponse LoginUsingBasicAuthorization(IUser user, uint maxRetries = 5);
    }
}
