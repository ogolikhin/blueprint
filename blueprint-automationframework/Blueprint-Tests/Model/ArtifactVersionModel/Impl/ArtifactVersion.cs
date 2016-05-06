using System;
using System.Collections.Generic;
using System.Net;
using Model.Factories;
using Model.ArtifactModel;
using Utilities;
using Utilities.Facades;

namespace Model.ArtifactVersionModel.Impl
{
    public enum LockResult
    {
        Success,
        AlreadyLocked,
        Failure
    }

    public enum DiscardResult
    {
        Success,
        Failure
    }

    public class ArtifactVersion : IArtifactVersion
    {
        private const string SVC_PATH = "svc/shared/artifacts/lock";
        private const string SessionTokenCookieName = "BLUEPRINT_SESSION_TOKEN";

        public string Address { get; }

        public List<IOpenApiArtifact> Artifacts { get; } = new List<IOpenApiArtifact>();

        #region Constructor

        public ArtifactVersion(string address)
        {
            ThrowIf.ArgumentNull(address, nameof(address));

            Address = address;
        }

        #endregion Constructor

        public IOpenApiArtifact CreateAndSaveProcessArtifact(IProject project, BaseArtifactType artifactType, IUser user, List<HttpStatusCode> expectedStatusCodes = null)
        {
            //Create an artifact with ArtifactType and populate all required values without properties
            var artifact = ArtifactFactory.CreateOpenApiArtifact(Address, user, project, artifactType);

            //Set to add in root of the project
            artifact.ParentId = artifact.ProjectId;

            //add the created artifact object into BP using OpenAPI call - assertions are inside of AddArtifact
            artifact.Save(user);

            // Add artifact to artifacts list
            Artifacts.Add(artifact);

            return artifact;
        }
        public List<LockResultInfo> LockArtifactIds(IUser user, List<int> artifactIds, List<HttpStatusCode> expectedStatusCodes = null, bool sendAuthorizationAsCookie = false)
        {
            ThrowIf.ArgumentNull(user, nameof(user));
            ThrowIf.ArgumentNull(artifactIds, nameof(artifactIds));

            string tokenValue = user.Token?.AccessControlToken;
            var cookies = new Dictionary<string, string>();

            if (sendAuthorizationAsCookie)
            {
                cookies.Add(SessionTokenCookieName, tokenValue);
                tokenValue = string.Empty;
            }

            var restApi = new RestApiFacade(Address, user.Username, user.Password, tokenValue);

            var response = restApi.SendRequestAndDeserializeObject<List<LockResultInfo>, List<int>>(
                SVC_PATH,
                RestRequestMethod.GET,
                additionalHeaders: null,
                queryParameters: null,
                jsonObject: artifactIds,
                expectedStatusCodes: expectedStatusCodes,
                cookies: cookies);

            return response;
        }

        public List<DiscardResultInfo> Discard(List<int> artifactIds)
        {
            throw new NotImplementedException();
        }
    }

    public class LockResultInfo
    {
        public LockResult Result { get; set; }

        public VersionInfo Info { get; set; }
    }

    public class VersionInfo
    {
        public int? ArtifactId { get; set;}
        public int ServerArtifactVersionId { get; set; }
        public DateTime? UtcLockedDateTime { get; set; }
        public string LockOwnerLogin { get; set; }
        public int ProjectId { get; set; }
    }

    public class DiscardResultInfo
    {
        public DiscardResult Result { get; set; }
        public string Message { get; set; }
    }
}
