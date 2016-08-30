using System;
using System.Collections.Generic;
using System.Net;
using Common;
using Model.ArtifactModel.Impl;
using Utilities;
using Utilities.Facades;

namespace Model.Impl
{
    public class SvcShared : NovaServiceBase, ISvcShared
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="address">The base URI of the svc/shared service.</param>
        public SvcShared(string address)
        {
            ThrowIf.ArgumentNull(address, nameof(address));

            Address = address;
        }

        #region Members inherited from ISvcShared

        /// <seealso cref="ISvcShared.FindUserOrGroup(IUser, string, bool?, int?, bool?, List{HttpStatusCode})"/>
        public List<UserOrGroupInfo> FindUserOrGroup(IUser user, 
            string search = null,
            bool? allowEmptyEmail = null,
            int? limit = null,
            bool? includeGuests = null,
            List<HttpStatusCode> expectedStatusCodes = null)
        {
            // GET: <base_url>/svc/shared/users/search?search={search}&emailDiscussions={emailDiscussions}&limit={limit}&includeGuests={includeGuests}
            // See:  https://github.com/BlueprintSys/blueprint-current/blob/develop/Source/BluePrintSys.RC.Web.Internal/Shared/Metadata/UsersAndGroupsController.cs

            RestApiFacade restApi = new RestApiFacade(Address, user?.Token?.AccessControlToken);
            Dictionary<string, string> queryParams = new Dictionary<string, string>();

            if (search != null)
            {
                queryParams.Add("search", search);
            }

            if (allowEmptyEmail != null)
            {
                queryParams.Add("emailDiscussions", allowEmptyEmail.Value.ToString());
            }

            if (limit != null)
            {
                queryParams.Add("limit", limit.Value.ToStringInvariant());
            }

            if (includeGuests != null)
            {
                queryParams.Add("includeGuests", includeGuests.Value.ToString());
            }

            return restApi.SendRequestAndDeserializeObject<List<UserOrGroupInfo>>(
                RestPaths.Svc.Shared.Users.SEARCH,
                RestRequestMethod.GET,
                queryParameters: queryParams,
                expectedStatusCodes: expectedStatusCodes);
        }

        #endregion Members inherited from ISvcShared

        #region Members inherited from IDisposable

        private bool _isDisposed = false;

        /// <summary>
        /// Disposes this object by deleting all objects that were created.
        /// </summary>
        /// <param name="disposing">Pass true if explicitly disposing or false if called from the destructor.</param>
        protected virtual void Dispose(bool disposing)
        {
            Logger.WriteTrace("{0}.{1} called.", nameof(SvcShared), nameof(Dispose));

            if (_isDisposed)
            {
                return;
            }

            if (disposing)
            {
                // TODO: Delete anything created by this class.
            }

            _isDisposed = true;
        }

        /// <summary>
        /// Disposes this object by deleting all objects that were created.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion Members inherited from IDisposable
    }
}
