using System;
using System.Collections.Generic;
using System.Net;
using Logging;
using Utilities.Facades;

namespace Model.Impl
{
    public class ArtifactStore : IArtifactStore
    {
        private const string SVC_PATH = "api/v1/projects";
        private const string URL_ARTIFACTS = "artifacts";
        private const string URL_PUBLISH = "api/v1/vc/publish";
        private const string URL_DISCARD = "api/v1/vc/discard";
        private const string URL_COMMENTS = "comments";
        private const string URL_REPLIES = "replies";
        private string _address = null;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="address">The URI address of the ArtifactStore.</param>
        public ArtifactStore(string address)
        {
            if (address == null) { throw new ArgumentNullException("address"); }

            _address = address;
        }

        /// <summary>
        /// Adds the specified artifact to the ArtifactStore.
        /// </summary>
        /// <param name="artifact">The artifact to add.</param>
        /// <param name="user">The user to authenticate to the ArtifactStore.</param>
        /// <param name="expectedStatusCodes">A list of expected status codes.  By default, only '201' is expected.</param>
        /// <returns>The artifact that was created (including the artifact ID that ArtifactStore gave it).</returns>
        /// <exception cref="WebException">A WebException sub-class if ArtifactStore returned an unexpected HTTP status code.</exception>
        public IArtifact AddArtifact(IArtifact artifact, IUser user, List<HttpStatusCode> expectedStatusCodes = null)
        {
            if (artifact == null) { throw new ArgumentNullException("artifact"); }
            if (user == null) { throw new ArgumentNullException("user"); }
            string path = string.Format(SVC_PATH + "/{0}/" + URL_ARTIFACTS, artifact.ProjectId);
            if (expectedStatusCodes == null)
            {
                expectedStatusCodes = new List<HttpStatusCode>();
                expectedStatusCodes.Add(HttpStatusCode.Created);
            }
            Artifact artractObject = (Artifact)artifact;
            RestApiFacade restApi = new RestApiFacade(_address, user.Username, user.Password);
            ArtifactResult artifactResult = restApi.SendRequestAndDeserializeObject<ArtifactResult,Artifact>(path, RestRequestMethod.POST, artractObject, expectedStatusCodes: expectedStatusCodes);
            Logger.WriteDebug("Result Code: {0}", artifactResult.ResultCode);
            Logger.WriteDebug(string.Format("POST {0} returned followings: Message: {1}, ResultCode: {2}", path, artifactResult.Message, artifactResult.ResultCode));
            Logger.WriteDebug("The Artifact Returned: {0}", artifactResult.Artifact);
            return artifact;
        }
    }
}
