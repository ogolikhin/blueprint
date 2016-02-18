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
    public class Artifact : IArtifact
    {
        // TODO update the const strings based on new api artifact specification
        private const string SVC_PATH = "api/v1/projects";
        private const string URL_ARTIFACTS = "artifacts";
        private const string URL_PUBLISH = "api/v1/vc/publish";
        private const string URL_DISCARD = "api/v1/vc/discard";
        private const string URL_COMMENTS = "comments";
        private const string URL_REPLIES = "replies";

        #region Properties
        public BaseArtifactType ArtifactType { get; set; }
        public int Id { get; set; }
        public string Name { get; set; }
        public int ProjectId { get; set; }
        public int Version { get; set; }
        public int ParentId { get; set; }
        public Uri BlueprintUrl { get; set; }
        public int ArtifactTypeId { get; set; }
        #endregion Properties
    }

    public class OpenApiArtifact : IOpenApiArtifact
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

        #region Properties
        public BaseArtifactType ArtifactType { get; set; }
        public int Id { get; set; }
        public string Name { get; set; }
        public int ProjectId { get; set; }
        public int Version { get; set; }
        public int ParentId { get; set; }
        public Uri BlueprintUrl { get; set; }
        public int ArtifactTypeId { get; set; }
        public string ArtifactTypeName { get; set; }
        public string BaseArtifactType { get; set; }
        public bool AreTracesReadOnly { get; set; }
        public bool AreAttachmentsReadOnly { get; set; }
        public bool AreDocumentReferencesReadOnly { get; set; }
        [JsonConverter(typeof(Deserialization.ConcreteConverter<OpenApiProperty>))]
        public List<IOpenApiProperty> Properties { get; private set; }
        [JsonConverter(typeof(Deserialization.ConcreteConverter<OpenApiComment>))]
        public List<IOpenApiComment> Comments { get; }
        [JsonConverter(typeof(Deserialization.ConcreteConverter<OpenApiTrace>))]
        public List<IOpenApiTrace> Traces { get; }
        [JsonConverter(typeof(Deserialization.ConcreteConverter<OpenApiAttachment>))]
        public List<IOpenApiAttachment> Attachments { get; }
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
            _address = address;
        }
        #endregion Constructors

        #region Methods
        /// <summary>
        /// Set Properties for the artifact object
        /// </summary>
        /// <param name="properties">The property list</param>
        /// 
        public void SetProperties(List<IOpenApiProperty> properties)
        {
            if (this.Properties == null)
            {
                Properties = new List<IOpenApiProperty>();
            }
            Properties = properties;
        }

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
                expectedStatusCodes = new List<HttpStatusCode>();
                expectedStatusCodes.Add(HttpStatusCode.Created);
            }

            OpenApiArtifact artifactObject = (OpenApiArtifact)artifact;
            RestApiFacade restApi = new RestApiFacade(_address, user.Username, user.Password);
            OpenApiArtifactResult artifactResult = restApi.SendRequestAndDeserializeObject<OpenApiArtifactResult, OpenApiArtifact>(path, RestRequestMethod.POST, artifactObject, expectedStatusCodes: expectedStatusCodes);

            Logger.WriteDebug("Result Code: {0}", artifactResult.ResultCode);
            Logger.WriteDebug(I18NHelper.FormatInvariant("POST {0} returned followings: Message: {1}, ResultCode: {2}", path, artifactResult.Message, artifactResult.ResultCode));
            Logger.WriteDebug("The Artifact Returned: {0}", artifactResult.Artifact);

            //TODO Assertion to check Message
            Assert.That(artifactResult.Message == "Success", I18NHelper.FormatInvariant("The returned Message was '{0}' but '{1}' was expected", artifactResult.Message, "Success"));

            Assert.That(artifactResult.ResultCode == ((int)HttpStatusCode.Created).ToString(CultureInfo.InvariantCulture), I18NHelper.FormatInvariant("The returned ResultCode was '{0}' but '{1}' was expected", artifactResult.ResultCode, ((int)HttpStatusCode.Created).ToString(CultureInfo.InvariantCulture)));

            return artifactResult.Artifact;
        }

        /// <summary>
        /// Delete the artifact to Blueprint.
        /// </summary>
        /// <param name="artifact">The artifact to delete.</param>
        /// <param name="user">The user to authenticate to Blueprint.</param>
        /// <param name="expectedStatusCodes">A list of expected status codes.</param>
        /// <returns>The artifactResult after delete artifact call</returns>
        /// <exception cref="WebException">A WebException sub-class if request call triggers an unexpected HTTP status code.</exception>
        public IOpenApiArtifactResult DeleteArtifact(IOpenApiArtifact artifact, IUser user, List<HttpStatusCode> expectedStatusCodes = null)
        {
            ThrowIf.ArgumentNull(artifact, nameof(artifact));
            ThrowIf.ArgumentNull(user, nameof(user));

            string path = I18NHelper.FormatInvariant(SVC_PATH + "/{0}/" + URL_ARTIFACTS + "/{1}/", artifact.ProjectId, artifact.Id);

            RestApiFacade restApi = new RestApiFacade(_address, user.Username, user.Password);
            OpenApiArtifactResult artifactResult = restApi.SendRequestAndDeserializeObject<OpenApiArtifactResult>(path, RestRequestMethod.DELETE, expectedStatusCodes: expectedStatusCodes);

            Logger.WriteDebug("Result Code: {0}", artifactResult.ResultCode);
            Logger.WriteDebug(I18NHelper.FormatInvariant("DELETE {0} returned followings: Message: {1}, ResultCode: {2}", path, artifactResult.Message, artifactResult.ResultCode));
            Logger.WriteDebug("The Artifact Returned: {0}", artifactResult.Artifact);

            return artifactResult;
        }
        #endregion Methods
    }
}
