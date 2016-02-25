using Common;
using Newtonsoft.Json;
using NUnit.Framework;
using System;
using System.Net;
using System.Collections.Generic;
using System.Globalization;
using Utilities;
using Utilities.Facades;

namespace Model.Impl
{
    public class ArtifactBase : IArtifactBase
    {
        #region Properties
        public BaseArtifactType BaseArtifactType { get; set; }
        public ItemTypePredefined BaseItemTypePredefined { get; set; }
        public int Id { get; set; }
        public string Name { get; set; }
        public int ProjectId { get; set; }
        public int Version { get; set; }
        public int ParentId { get; set; }
        public Uri BlueprintUrl { get; set; }
        public int ArtifactTypeId { get; set; }
        public string ArtifactTypeName { get; set; }
        public bool AreTracesReadOnly { get; set; }
        public bool AreAttachmentsReadOnly { get; set; }
        public bool AreDocumentReferencesReadOnly { get; set; }
        public string Address { get; set; }
        #endregion Properties
    }

    public class Artifact : ArtifactBase, IArtifact
    {
        public IArtifact AddArtifact(IArtifact artifact, IUser user, List<HttpStatusCode> expectedStatusCodes = null)
        {
            throw new NotImplementedException();
        }

        public IArtifactResult<IArtifact> DeleteArtifact(IArtifact artifact, IUser user, List<HttpStatusCode> expectedStatusCodes = null)
        {
            throw new NotImplementedException();
        }
    }

    public class OpenApiArtifact : ArtifactBase, IOpenApiArtifact
    {
        #region Constants
        private const string SVC_PATH = "api/v1/projects";
        private const string URL_ARTIFACTS = "artifacts";
        private const string URL_PUBLISH = "api/v1/vc/publish";
        private const string URL_DISCARD = "api/v1/vc/discard";
        private const string URL_COMMENTS = "comments";
        private const string URL_REPLIES = "replies";
        #endregion Constants

        #region Properties
        [JsonConverter(typeof (Deserialization.ConcreteListConverter<IOpenApiProperty, OpenApiProperty>))]
        public List<IOpenApiProperty> Properties { get; } = new List<IOpenApiProperty>();

        [JsonConverter(typeof (Deserialization.ConcreteListConverter<IOpenApiComment, OpenApiComment>))]
        public List<IOpenApiComment> Comments { get; } = new List<IOpenApiComment>();

        [JsonConverter(typeof (Deserialization.ConcreteListConverter<IOpenApiTrace, OpenApiTrace>))]
        public List<IOpenApiTrace> Traces { get; } = new List<IOpenApiTrace>();

        [JsonConverter(typeof (Deserialization.ConcreteListConverter<IOpenApiAttachment, OpenApiAttachment>))]
        public List<IOpenApiAttachment> Attachments { get; } = new List<IOpenApiAttachment>();
        #endregion Properties

        #region Constructors
        /// <summary>
        /// Constructor in order to use it as generic type
        /// </summary>
        public OpenApiArtifact()
        {
            //Required for deserializing OpenApiArtifact
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="address">The URI address of the artifact.</param>
        public OpenApiArtifact(string address)
        {
            ThrowIf.ArgumentNull(address, nameof(address));
            this.Address = address;
        }

        public OpenApiArtifact(string address, int id, int projectId)
        {
            ThrowIf.ArgumentNull(address, nameof(address));
            this.Address = address;
            this.Id = id;
            this.ProjectId = projectId;
        }
        #endregion Constructors

        #region Methods

        public IOpenApiArtifact AddArtifact(
            IOpenApiArtifact artifact,
            IUser user,
            List<HttpStatusCode> expectedStatusCodes = null)
        {
            ThrowIf.ArgumentNull(artifact, nameof(artifact));
            ThrowIf.ArgumentNull(user, nameof(user));
            Dictionary<string, string> additionalHeaders = new Dictionary<string, string>();

            string path = I18NHelper.FormatInvariant("{0}/{1}/{2}", SVC_PATH, artifact.ProjectId, URL_ARTIFACTS);

            if (expectedStatusCodes == null)
            {
                expectedStatusCodes = new List<HttpStatusCode> {HttpStatusCode.Created};
            }

            OpenApiArtifact artifactObject = (OpenApiArtifact)artifact;

            RestApiFacade restApi = new RestApiFacade(Address, user.Username, user.Password);
            IArtifactResult<IOpenApiArtifact> artifactResult = restApi.SendRequestAndDeserializeObject<OpenApiArtifactResult, OpenApiArtifact>(
                path, RestRequestMethod.POST, artifactObject, additionalHeaders: additionalHeaders, expectedStatusCodes: expectedStatusCodes);

            Logger.WriteDebug("Result Code: {0}", artifactResult.ResultCode);
            Logger.WriteDebug("POST {0} returned followings: Message: {1}, ResultCode: {2}", path, artifactResult.Message, artifactResult.ResultCode);
            Logger.WriteDebug("The Artifact Returned: {0}", artifactResult.Artifact);

            //TODO Assertion to check Message
            const string expectedMsg = "Success";
            Assert.That(artifactResult.Message == expectedMsg, "The returned Message was '{0}' but '{1}' was expected", artifactResult.Message, expectedMsg);

            Assert.That(artifactResult.ResultCode == ((int)HttpStatusCode.Created).ToString(CultureInfo.InvariantCulture),
                "The returned ResultCode was '{0}' but '{1}' was expected",
                artifactResult.ResultCode, ((int)HttpStatusCode.Created).ToString(CultureInfo.InvariantCulture));

            //add back address
            artifactResult.Artifact.Address = artifact.Address;

            return artifactResult.Artifact;
        }

        public List<IPublishArtifactResult> PublishArtifacts(
            List<IOpenApiArtifact> artifactList,
            IUser user,
            bool shouldKeepLock = false,
            List<HttpStatusCode> expectedStatusCodes = null)
        {
            ThrowIf.ArgumentNull(artifactList, nameof(artifactList));
            ThrowIf.ArgumentNull(user, nameof(user));
            Dictionary<string, string> additionalHeaders = new Dictionary<string, string>();

            if (shouldKeepLock)
            {
                additionalHeaders.Add("KeepLock", "true");
            }

            string path = URL_PUBLISH;

            if (expectedStatusCodes == null)
            {
                expectedStatusCodes = new List<HttpStatusCode> { HttpStatusCode.OK };
            }

            List<OpenApiArtifact> artifactObjectList = new List<OpenApiArtifact>();

            foreach (IOpenApiArtifact artifact in artifactList)
            {
                OpenApiArtifact artifactElement = new OpenApiArtifact(artifact.Address);
                artifactElement.Id = artifact.Id;
                artifactElement.ProjectId = artifact.ProjectId;
                artifactObjectList.Add(artifactElement);
            }

            RestApiFacade restApi = new RestApiFacade(Address, user.Username, user.Password);

            var artifactResults = restApi.SendRequestAndDeserializeObject<List<PublishArtifactResult>, List<OpenApiArtifact>>(
                path, RestRequestMethod.POST, artifactObjectList, additionalHeaders: additionalHeaders, expectedStatusCodes: expectedStatusCodes);

            return artifactResults.ConvertAll(o => (IPublishArtifactResult)o);
        }

        public IArtifactResult<IOpenApiArtifact> DeleteArtifact(
            IOpenApiArtifact artifact,
            IUser user,
            List<HttpStatusCode> expectedStatusCodes = null)
        {
            ThrowIf.ArgumentNull(artifact, nameof(artifact));
            ThrowIf.ArgumentNull(user, nameof(user));

            string path = I18NHelper.FormatInvariant("{0}/{1}/{2}/{3}", SVC_PATH, artifact.ProjectId, URL_ARTIFACTS, artifact.Id);

            RestApiFacade restApi = new RestApiFacade(Address, user.Username, user.Password);
            IArtifactResult<IOpenApiArtifact> artifactResult = restApi.SendRequestAndDeserializeObject<OpenApiArtifactResult>(
                path, RestRequestMethod.DELETE, expectedStatusCodes: expectedStatusCodes);

            Logger.WriteDebug("Result Code: {0}", artifactResult.ResultCode);
            Logger.WriteDebug("DELETE {0} returned followings: Message: {1}, ResultCode: {2}", path, artifactResult.Message, artifactResult.ResultCode);
            Logger.WriteDebug("The Artifact Returned: {0}", artifactResult.Artifact);

            return artifactResult;
        }

        #endregion Methods
    }
}
