﻿using Common;
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
        private string _address = null;
        #endregion Constants

        [JsonConverter(typeof (Deserialization.ConcreteConverter<OpenApiProperty>))]
        public List<IOpenApiProperty> Properties { get; private set; }

        [JsonConverter(typeof (Deserialization.ConcreteConverter<OpenApiComment>))]
        public List<IOpenApiComment> Comments { get; private set; }

        [JsonConverter(typeof (Deserialization.ConcreteConverter<OpenApiTrace>))]
        public List<IOpenApiTrace> Traces { get; private set; }

        [JsonConverter(typeof (Deserialization.ConcreteConverter<OpenApiAttachment>))]
        public List<IOpenApiAttachment> Attachments { get; private set; }

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
            _address = address;
        }
        #endregion Constructors

        /// <summary>
        /// Adds the artifact to Blueprint.
        /// </summary>
        /// <param name="artifact">The artifact to add.</param>
        /// <param name="user">The user to authenticate to blueprint.</param>
        /// <param name="expectedStatusCodes">A list of expected status codes.  By default, only '201' is expected.</param>
        /// <returns>The artifact result after adding artifact.</returns>
        /// <exception cref="WebException">A WebException sub-class if request call triggers an unexpected HTTP status code.</exception>
        public IOpenApiArtifact AddArtifact(IOpenApiArtifact artifact, IUser user, List<HttpStatusCode> expectedStatusCodes = null)
        {
            ThrowIf.ArgumentNull(artifact, nameof(artifact));
            ThrowIf.ArgumentNull(user, nameof(user));

            string path = I18NHelper.FormatInvariant(SVC_PATH + "/{0}/" + URL_ARTIFACTS, artifact.ProjectId);

            if (expectedStatusCodes == null)
            {
                expectedStatusCodes = new List<HttpStatusCode> {HttpStatusCode.Created};
            }

            OpenApiArtifact artifactObject = (OpenApiArtifact)artifact;
            RestApiFacade restApi = new RestApiFacade(_address, user.Username, user.Password);
            IArtifactResult<IOpenApiArtifact> artifactResult = restApi.SendRequestAndDeserializeObject<OpenApiArtifactResult, OpenApiArtifact>(path, RestRequestMethod.POST, artifactObject, expectedStatusCodes: expectedStatusCodes);

            Logger.WriteDebug("Result Code: {0}", artifactResult.ResultCode);
            Logger.WriteDebug(I18NHelper.FormatInvariant("POST {0} returned followings: Message: {1}, ResultCode: {2}", path, artifactResult.Message, artifactResult.ResultCode));
            Logger.WriteDebug("The Artifact Returned: {0}", artifactResult.Artifact);

            //TODO Assertion to check Message
            Assert.That(artifactResult.Message == "Success", I18NHelper.FormatInvariant("The returned Message was '{0}' but '{1}' was expected", artifactResult.Message, "Success"));

            Assert.That(artifactResult.ResultCode == ((int)HttpStatusCode.Created).ToString(CultureInfo.InvariantCulture), I18NHelper.FormatInvariant("The returned ResultCode was '{0}' but '{1}' was expected", artifactResult.ResultCode, ((int)HttpStatusCode.Created).ToString(CultureInfo.InvariantCulture)));

            return artifactResult.Artifact;
        }

        public IArtifactResult<IOpenApiArtifact> DeleteArtifact(IOpenApiArtifact artifact, IUser user, List<HttpStatusCode> expectedStatusCodes = null)
        {
            ThrowIf.ArgumentNull(artifact, nameof(artifact));
            ThrowIf.ArgumentNull(user, nameof(user));

            string path = I18NHelper.FormatInvariant(SVC_PATH + "/{0}/" + URL_ARTIFACTS + "/{1}/", artifact.ProjectId, artifact.Id);

            RestApiFacade restApi = new RestApiFacade(_address, user.Username, user.Password);
            IArtifactResult<IOpenApiArtifact> artifactResult = restApi.SendRequestAndDeserializeObject<OpenApiArtifactResult>(path, RestRequestMethod.DELETE, expectedStatusCodes: expectedStatusCodes);

            Logger.WriteDebug("Result Code: {0}", artifactResult.ResultCode);
            Logger.WriteDebug(I18NHelper.FormatInvariant("DELETE {0} returned followings: Message: {1}, ResultCode: {2}", path, artifactResult.Message, artifactResult.ResultCode));
            Logger.WriteDebug("The Artifact Returned: {0}", artifactResult.Artifact);

            return artifactResult;
        }
    }
}
