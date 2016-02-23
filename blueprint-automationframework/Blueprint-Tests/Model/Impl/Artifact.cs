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
        [JsonConverter(typeof (Deserialization.ConcreteConverter<OpenApiProperty>))]
        public List<IOpenApiProperty> Properties { get; private set; }

        [JsonConverter(typeof (Deserialization.ConcreteConverter<OpenApiComment>))]
        public List<IOpenApiComment> Comments { get; private set; }

        [JsonConverter(typeof (Deserialization.ConcreteConverter<OpenApiTrace>))]
        public List<IOpenApiTrace> Traces { get; private set; }

        [JsonConverter(typeof (Deserialization.ConcreteConverter<OpenApiAttachment>))]
        public List<IOpenApiAttachment> Attachments { get; private set; }
        #endregion Properties

        public void SetProperties(List<IOpenApiProperty> properties)
        {
            if (Properties == null)
            {
                Properties = new List<IOpenApiProperty>();
            }
            Properties = properties;
        }

        public void SetComments(List<IOpenApiComment> comments)
        {
            if (Comments == null)
            {
                Comments = new List<IOpenApiComment>();
            }
            Comments = comments;
        }

        public void SetTraces(List<IOpenApiTrace> traces)
        {
            if (Traces == null)
            {
                Traces = new List<IOpenApiTrace>();
            }
            Traces = traces;
        }

        public void SetAttachments(List<IOpenApiAttachment> attachments)
        {
            if (Attachments == null)
            {
                Attachments = new List<IOpenApiAttachment>();
            }
            Attachments = attachments;
        }

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
            Address = address;
        }
        #endregion Constructors

        #region Methods
        public IOpenApiArtifact AddArtifact(IOpenApiArtifact artifact, IUser user, List<HttpStatusCode> expectedStatusCodes = null)
        {
            ThrowIf.ArgumentNull(artifact, nameof(artifact));
            ThrowIf.ArgumentNull(user, nameof(user));
            Dictionary<string, string> additionalHeaders = new Dictionary<string, string>();
            additionalHeaders.Add("Accept", "application/json");

            string path = I18NHelper.FormatInvariant(SVC_PATH + "/{0}/" + URL_ARTIFACTS, artifact.ProjectId);

            if (expectedStatusCodes == null)
            {
                expectedStatusCodes = new List<HttpStatusCode> {HttpStatusCode.Created};
            }

            OpenApiArtifact artifactObject = (OpenApiArtifact)artifact;
            RestApiFacade restApi = new RestApiFacade(Address, user.Username, user.Password);
            IArtifactResult<IOpenApiArtifact> artifactResult = restApi.SendRequestAndDeserializeObject<OpenApiArtifactResult, OpenApiArtifact>(path, RestRequestMethod.POST, artifactObject, additionalHeaders: additionalHeaders, expectedStatusCodes: expectedStatusCodes);

            Logger.WriteDebug("Result Code: {0}", artifactResult.ResultCode);
            Logger.WriteDebug(I18NHelper.FormatInvariant("POST {0} returned followings: Message: {1}, ResultCode: {2}", path, artifactResult.Message, artifactResult.ResultCode));
            Logger.WriteDebug("The Artifact Returned: {0}", artifactResult.Artifact);

            //TODO Assertion to check Message
            Assert.That(artifactResult.Message == "Success", I18NHelper.FormatInvariant("The returned Message was '{0}' but '{1}' was expected", artifactResult.Message, "Success"));

            Assert.That(artifactResult.ResultCode == ((int)HttpStatusCode.Created).ToString(CultureInfo.InvariantCulture), I18NHelper.FormatInvariant("The returned ResultCode was '{0}' but '{1}' was expected", artifactResult.ResultCode, ((int)HttpStatusCode.Created).ToString(CultureInfo.InvariantCulture)));
            //add back address
            artifactResult.Artifact.Address = artifact.Address;

            return artifactResult.Artifact;
        }

        public List<PublishArtifactResult> PublishArtifacts(List<IOpenApiArtifact> artifactList, IUser user, bool isKeepLock = false, List<HttpStatusCode> expectedStatusCodes = null)
        {
            ThrowIf.ArgumentNull(artifactList, nameof(artifactList));
            ThrowIf.ArgumentNull(user, nameof(user));
            Dictionary<string, string> additionalHeaders = new Dictionary<string, string>();
            additionalHeaders.Add("Accept", "application/json");
            if (isKeepLock)
            {
                additionalHeaders.Add("KeepLock", "true");
            }

            string path = URL_PUBLISH;

            if (expectedStatusCodes == null)
            {
                expectedStatusCodes = new List<HttpStatusCode> { HttpStatusCode.OK };
            }

            OpenApiArtifact artifactElement;
            List<OpenApiArtifact> artifactObjectList = new List<OpenApiArtifact>();
            foreach (IOpenApiArtifact artifact in artifactList)
            {
                artifactElement = new OpenApiArtifact(artifact.Address);
                artifactElement.Id = artifact.Id;
                artifactElement.ProjectId = artifact.ProjectId;
                artifactObjectList.Add(artifactElement);
            }

            RestApiFacade restApi = new RestApiFacade(Address, user.Username, user.Password);

            List<PublishArtifactResult> artifactResult = restApi.SendRequestAndDeserializeObject<List<PublishArtifactResult>, List<OpenApiArtifact>>(path, RestRequestMethod.POST, artifactObjectList, additionalHeaders: additionalHeaders, expectedStatusCodes: expectedStatusCodes);
            return artifactResult;
        }

        public IArtifactResult<IOpenApiArtifact> DeleteArtifact(IOpenApiArtifact artifact, IUser user, List<HttpStatusCode> expectedStatusCodes = null)
        {
            ThrowIf.ArgumentNull(artifact, nameof(artifact));
            ThrowIf.ArgumentNull(user, nameof(user));

            string path = I18NHelper.FormatInvariant(SVC_PATH + "/{0}/" + URL_ARTIFACTS + "/{1}/", artifact.ProjectId, artifact.Id);

            RestApiFacade restApi = new RestApiFacade(Address, user.Username, user.Password);
            IArtifactResult<IOpenApiArtifact> artifactResult = restApi.SendRequestAndDeserializeObject<OpenApiArtifactResult>(path, RestRequestMethod.DELETE, expectedStatusCodes: expectedStatusCodes);

            Logger.WriteDebug("Result Code: {0}", artifactResult.ResultCode);
            Logger.WriteDebug(I18NHelper.FormatInvariant("DELETE {0} returned followings: Message: {1}, ResultCode: {2}", path, artifactResult.Message, artifactResult.ResultCode));
            Logger.WriteDebug("The Artifact Returned: {0}", artifactResult.Artifact);

            return artifactResult;
        }

        #endregion Methods
    }
}
