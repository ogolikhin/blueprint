using System;
using System.Collections.Generic;
using System.Net;
using Common;
using Model.ArtifactModel.Impl;
using Utilities;
using Model.Factories;
using Utilities.Facades;
using Newtonsoft.Json;
using NUnit.Framework;

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

        /// <seealso cref="IArtifactStore.GetCustomArtifactTypes(IProject, IUser, List{HttpStatusCode})"/>
        public ProjectCustomArtifactTypesResult GetCustomArtifactTypes(IProject project, IUser user = null, List<HttpStatusCode> expectedStatusCodes = null)
        {
            ThrowIf.ArgumentNull(project, nameof(project));

            Logger.WriteInfo("Getting artifact types for project ID: {0}.", project.Id);

            string path = I18NHelper.FormatInvariant("{0}/projects/{1}/meta/customtypes", SVC_PATH, project.Id);
            var restApi = new RestApiFacade(Address, user?.Token?.AccessControlToken);

            var artifactTypes = restApi.SendRequestAndDeserializeObject<ProjectCustomArtifactTypesResult>(
                path,
                RestRequestMethod.GET,
                expectedStatusCodes: expectedStatusCodes);

            // Print all returned types for debugging.
            foreach (var artifactType in artifactTypes.ArtifactTypes)
            {
                Logger.WriteDebug("*** Artifact Type - Name: '{0}', BaseType: '{1}', Prefix: '{2}'", artifactType.Name, artifactType.BaseType, artifactType.Prefix);
            }

            foreach (var artifactType in artifactTypes.SubArtifactTypes)
            {
                Logger.WriteDebug("*** Sub-Artifact Type - Name: '{0}', BaseType: '{1}', Prefix: '{2}'", artifactType.Name, artifactType.BaseType, artifactType.Prefix);
            }

            foreach (var propertyType in artifactTypes.PropertyTypes)
            {
                Logger.WriteDebug("*** Property Type - Name: '{0}', BaseType: '{1}'", propertyType.Name, propertyType.PrimitiveType.ToString());
            }

            return artifactTypes;
        }

        /// <seealso cref="IArtifactStore.GetArtifactChildrenByProjectAndArtifactId(int, int, IUser, List{HttpStatusCode})"/>
        public List<Artifact> GetArtifactChildrenByProjectAndArtifactId(int projectId, int artifactId, IUser user = null, List<HttpStatusCode> expectedStatusCodes = null)
        {
            string path = I18NHelper.FormatInvariant("{0}/projects/{1}/artifacts/{2}/children", SVC_PATH, projectId, artifactId);
            ISession session = null;
            List<Artifact> artifactList = null;

            if (user != null)
                session = SessionFactory.CreateSessionWithToken(user);

            RestResponse response = GetResponseFromRequest(path, projectId, session, expectedStatusCodes);

            if (response.StatusCode == HttpStatusCode.OK)
            {
                artifactList = JsonConvert.DeserializeObject<List<Artifact>>(response.Content);
                Assert.IsNotNull(artifactList, "Object could not be deserialized properly.");
            }

            return artifactList;
        }

        /// <seealso cref="IArtifactStore.GetProjectChildrenByProjectId(int, IUser, List{HttpStatusCode})"/>
        public List<Artifact> GetProjectChildrenByProjectId(int id, IUser user = null, List<HttpStatusCode> expectedStatusCodes = null)
        {
            string path = I18NHelper.FormatInvariant("{0}/projects/{1}/children", SVC_PATH, id);
            ISession session = null;
            List <Artifact> artifactList = null;

            if (user != null)
                session = SessionFactory.CreateSessionWithToken(user);

            RestResponse response = GetResponseFromRequest(path, id, session, expectedStatusCodes);

            if (response.StatusCode == HttpStatusCode.OK)
            {
                artifactList = JsonConvert.DeserializeObject<List<Artifact>>(response.Content);
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
