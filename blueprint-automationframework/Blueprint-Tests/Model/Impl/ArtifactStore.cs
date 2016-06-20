﻿using System;
using System.Collections.Generic;
using System.Net;
using Common;
using Utilities;
using Model.Factories;
using Utilities.Facades;
using Newtonsoft.Json;
using NUnit.Framework;
using Model.ArtifactModel.Impl;

namespace Model.Impl
{
    public class ArtifactStore : NovaServiceBase, IArtifactStore
    {
        private const string SVC_PATH = "svc/artifactstore";
        private const string TOKEN_HEADER = BlueprintToken.ACCESS_CONTROL_TOKEN_HEADER;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="address">The base URI of the ArtifactStore service.</param>
        public ArtifactStore(string address)
        {
            ThrowIf.ArgumentNull(address, nameof(address));

            Address = address;
        }

        #region Members inherited from IArtifactStore

        /// <seealso cref="IArtifactStore.GetStatus"/>
        public string GetStatus(string preAuthorizedKey = CommonConstants.PreAuthorizedKeyForStatus, List<HttpStatusCode> expectedStatusCodes = null)
        {
            return GetStatus(SVC_PATH, preAuthorizedKey, expectedStatusCodes);
        }

        /// <seealso cref="IArtifactStore.GetStatusUpcheck"/>
        public HttpStatusCode GetStatusUpcheck(List<HttpStatusCode> expectedStatusCodes = null)
        {
            return GetStatusUpcheck(SVC_PATH, expectedStatusCodes);
        }

        public List<ArtifactType> GetArtifactChildrenByProjectAndArtifactId(int projectId, int artifactId, IUser user = null, List<HttpStatusCode> expectedStatusCodes = null)
        {
            string path = I18NHelper.FormatInvariant("{0}/projects/{1}/artifacts/{2}/children", SVC_PATH, projectId, artifactId);
            ISession session = null;
            List<ArtifactType> artifactList = null;

            if (user != null)
                session = SessionFactory.CreateSessionWithToken(user);

            RestResponse response = GetResponseFromRequest(path, projectId, session, expectedStatusCodes);

            if (response.StatusCode == HttpStatusCode.OK)
            {
                artifactList = JsonConvert.DeserializeObject<List<ArtifactType>>(response.Content);
                Assert.IsNotNull(artifactList, "Object could not be deserialized properly.");
            }

            return artifactList;
        }

        public List<ArtifactType> GetProjectChildrenByProjectId(int id, IUser user = null, List<HttpStatusCode> expectedStatusCodes = null)
        {
            string path = I18NHelper.FormatInvariant("{0}/projects/{1}/children", SVC_PATH, id);
            ISession session = null;
            List <ArtifactType> artifactList = null;

            if (user != null)
                session = SessionFactory.CreateSessionWithToken(user);

            RestResponse response = GetResponseFromRequest(path, id, session, expectedStatusCodes);

            if (response.StatusCode == HttpStatusCode.OK)
            {
                artifactList = JsonConvert.DeserializeObject<List<ArtifactType>>(response.Content);
                Assert.IsNotNull(artifactList, "Object could not be deserialized properly.");
            }

            return artifactList;
        }

        private RestResponse GetResponseFromRequest(string path, int id, ISession session, List<HttpStatusCode> expectedStatusCodes)
        {
            RestApiFacade restApi;
            if (session != null)
                restApi = new RestApiFacade(Address, string.Empty);
            else
                restApi = new RestApiFacade(Address, null);

            Dictionary<string, string> queryParameters = new Dictionary<string, string> { { "id", id.ToString(System.Globalization.CultureInfo.InvariantCulture) } };
            Dictionary<string, string> additionalHeaders = null;

            if (session != null)
                additionalHeaders = new Dictionary<string, string> { { TOKEN_HEADER, session.SessionId } };

            try
            {
                Logger.WriteInfo("Getting artifact - " + id);
                return restApi.SendRequestAndGetResponse(path, RestRequestMethod.GET, additionalHeaders: additionalHeaders, queryParameters: queryParameters, expectedStatusCodes: expectedStatusCodes);
            }
            catch (WebException ex)
            {
                Logger.WriteError("Content = '{0}'", restApi.Content);
                Logger.WriteError("Error while getting response - {0}", ex.Message);
                throw;
            }
        }

        public List<ArtifactHistoryVersion> GetArtifactHistory(int artifactId, IUser user, 
            bool? sortByDateAsc = null, int? limit = null, int? offset = null, 
            List<HttpStatusCode> expectedStatusCodes = null)
        {
            ThrowIf.ArgumentNull(user, nameof(user));
            string path = I18NHelper.FormatInvariant("{0}/artifacts/{1}/version", SVC_PATH, artifactId);

            var restApi = new RestApiFacade(Address, token: user.Token.AccessControlToken);
            Dictionary<string, string> queryParameters = null;
            if (sortByDateAsc != null)
            {
                queryParameters = new Dictionary<string, string> { { "sortByDateAsc", sortByDateAsc.ToString() } };
            }
            if (limit != null)
            {
                if (queryParameters == null)
                { queryParameters = new Dictionary<string, string> { { "limit", limit.ToString() } }; }
                else
                { queryParameters.Add("limit", limit.ToString()); }
            }
            if (offset != null)
            {
                if (queryParameters == null)
                { queryParameters = new Dictionary<string, string> { { "offset", offset.ToString() } }; }
                else
                { queryParameters.Add("offset", offset.ToString()); }
            }

            var artifactHistory = restApi.SendRequestAndDeserializeObject<ArtifactHistory>(path, RestRequestMethod.GET,
                queryParameters: queryParameters, expectedStatusCodes: expectedStatusCodes);

            return artifactHistory.artifactHistoryVersions;
        }

        #endregion Members inherited from IArtifactStore

        #region Members inherited from IDisposable

        private bool _isDisposed = false;

        /// <summary>
        /// Disposes this object by deleting all artifacts that were created.
        /// </summary>
        /// <param name="disposing">Pass true if explicitly disposing or false if called from the destructor.</param>
        protected virtual void Dispose(bool disposing)
        {
            Logger.WriteTrace("{0}.{1} called.", nameof(ArtifactStore), nameof(Dispose));

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
        /// Disposes this object by deleting all artifacts that were created.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion Members inherited from IDisposable
    }
}
