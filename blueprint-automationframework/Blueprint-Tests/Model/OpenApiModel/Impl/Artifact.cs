﻿using Common;
using Newtonsoft.Json;
using NUnit.Framework;
using System;
using System.Net;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using Utilities;
using Utilities.Facades;

namespace Model.OpenApiModel.Impl
{
    public class ArtifactBase : IArtifactBase
    {
        private const string SessionTokenCookieName = "BLUEPRINT_SESSION_TOKEN";
        private const string SearchPath = "/svc/shared/artifacts/search";

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

        public IList<IArtifactBase> SearchArtifactsByName(IUser user, string searchSubstring, bool sendAuthorizationAsCookie = false, List<HttpStatusCode> expectedStatusCodes = null)
        {
            ThrowIf.ArgumentNull(user, nameof(user));

            string tokenValue = user.Token?.AccessControlToken;
            var cookies = new Dictionary<string, string>();

            if (sendAuthorizationAsCookie)
            {
                cookies.Add(SessionTokenCookieName, tokenValue);
                tokenValue = string.Empty;
            }

            Dictionary<string, string> queryParameters = new Dictionary<string, string> {
                { "name", searchSubstring }
            };

            var restApi = new RestApiFacade(Address, user.Username, user.Password, tokenValue);

            var response = restApi.SendRequestAndDeserializeObject<List<ArtifactBase>>(
                SearchPath,
                RestRequestMethod.GET,
                queryParameters: queryParameters,
                expectedStatusCodes: expectedStatusCodes,
                cookies: cookies);
            Logger.WriteDebug("Response for search artifact by name: {0}", response);

            return response.ConvertAll(o => (IArtifactBase)o);
        }
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

        public IUser CreatedBy { get; set; }

        [SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        [JsonConverter(typeof (Deserialization.ConcreteConverter<List<OpenApiProperty>>))]
        public List<OpenApiProperty> Properties { get; set; }

        [SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        [JsonConverter(typeof (Deserialization.ConcreteConverter<List<OpenApiComment>>))]
        public List<OpenApiComment> Comments { get; set; }

        [SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        [JsonConverter(typeof (Deserialization.ConcreteConverter<List<OpenApiTrace>>))]
        public List<OpenApiTrace> Traces { get; set; }

        [SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        [JsonConverter(typeof (Deserialization.ConcreteConverter<List<OpenApiAttachment>>))]
        public List<OpenApiAttachment> Attachments { get; set; }
        
        #endregion Properties

        #region Constructors
        
        /// <summary>
        /// Constructor in order to use it as generic type
        /// </summary>
        public OpenApiArtifact()
        {
            //Required for deserializing OpenApiArtifact
            Properties = new List<OpenApiProperty>();
            Comments = new List<OpenApiComment>();
            Traces = new List<OpenApiTrace>();
            Attachments = new List<OpenApiAttachment>();
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="address">The URI address of the artifact.</param>
        public OpenApiArtifact(string address) : this()
        {
            ThrowIf.ArgumentNull(address, nameof(address));
            Address = address;
        }

        public OpenApiArtifact(string address, int id, int projectId) : this()
        {
            ThrowIf.ArgumentNull(address, nameof(address));
            Address = address;
            Id = id;
            ProjectId = projectId;
        }

        #endregion Constructors

        #region Methods

        public void Save(
            IUser user,
            List<HttpStatusCode> expectedStatusCodes = null)
        {
            ThrowIf.ArgumentNull(user, nameof(user));
            
            string path = I18NHelper.FormatInvariant("{0}/{1}/{2}", SVC_PATH, ProjectId, URL_ARTIFACTS);

            if (expectedStatusCodes == null)
            {
                expectedStatusCodes = new List<HttpStatusCode> { HttpStatusCode.Created };
            }

            OpenApiArtifact artifactObject = this;

            RestApiFacade restApi = new RestApiFacade(Address, user.Username, user.Password, user.Token.OpenApiToken);
            IArtifactResult<IOpenApiArtifact> artifactResult = restApi.SendRequestAndDeserializeObject<OpenApiArtifactResult, OpenApiArtifact>(
                path, RestRequestMethod.POST, artifactObject, expectedStatusCodes: expectedStatusCodes);

            Logger.WriteDebug("POST {0} returned followings: Message: {1}, ResultCode: {2}", path, artifactResult.Message, artifactResult.ResultCode);
            Logger.WriteDebug("The Artifact Returned: {0}", artifactResult.Artifact);

            Id = artifactResult.Artifact.Id;

            const string expectedMsg = "Success";
            Assert.That(artifactResult.Message == expectedMsg, "The returned Message was '{0}' but '{1}' was expected", artifactResult.Message, expectedMsg);

            Assert.That(artifactResult.ResultCode == ((int)HttpStatusCode.Created).ToString(CultureInfo.InvariantCulture),
                "The returned ResultCode was '{0}' but '{1}' was expected",
                artifactResult.ResultCode, ((int)HttpStatusCode.Created).ToString(CultureInfo.InvariantCulture));
        }

        public void Publish(IUser user,
            bool shouldKeepLock = false,
            List<HttpStatusCode> expectedStatusCodes = null)
        {
            ThrowIf.ArgumentNull(user, nameof(user));
            var additionalHeaders = new Dictionary<string, string>();

            if (shouldKeepLock)
            {
                additionalHeaders.Add("KeepLock", "true");
            }

            OpenApiArtifact artifactToPublish = new OpenApiArtifact
            {
                Id = Id,
                ProjectId = ProjectId
            };

            var artifactObjectList = new List<OpenApiArtifact> { artifactToPublish };

            RestApiFacade restApi = new RestApiFacade(Address, user.Username, user.Password, user.Token.OpenApiToken);
            var publishResultList = restApi.SendRequestAndDeserializeObject<List<PublishArtifactResult>, List<OpenApiArtifact>>(
                URL_PUBLISH, RestRequestMethod.POST, artifactObjectList, additionalHeaders: additionalHeaders, expectedStatusCodes: expectedStatusCodes);

            Logger.WriteDebug("Result Code for Publish artifact: {0}", publishResultList[0].ResultCode);

            Assert.That(publishResultList[0].ResultCode == ((int)HttpStatusCode.OK).ToString(CultureInfo.InvariantCulture),
                "The returned ResultCode was '{0}' but '{1}' was expected", publishResultList[0].ResultCode, ((int)HttpStatusCode.OK).ToString(CultureInfo.InvariantCulture));
        }

        public List<IPublishArtifactResult> PublishArtifacts(
            List<IOpenApiArtifact> artifactList,
            IUser user,
            bool shouldKeepLock = false,
            List<HttpStatusCode> expectedStatusCodes = null)
        {
            ThrowIf.ArgumentNull(artifactList, nameof(artifactList));
            ThrowIf.ArgumentNull(user, nameof(user));
            var additionalHeaders = new Dictionary<string, string>();

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
                OpenApiArtifact artifactElement = new OpenApiArtifact(artifact.Address)
                {
                    Id = artifact.Id,
                    ProjectId = artifact.ProjectId
                };
                artifactObjectList.Add(artifactElement);
            }

            RestApiFacade restApi = new RestApiFacade(Address, user.Username, user.Password, user.Token.OpenApiToken);

            var artifactResults = restApi.SendRequestAndDeserializeObject<List<PublishArtifactResult>, List<OpenApiArtifact>>(
                path, RestRequestMethod.POST, artifactObjectList, additionalHeaders: additionalHeaders, expectedStatusCodes: expectedStatusCodes);

            return artifactResults.ConvertAll(o => (IPublishArtifactResult)o);
        }

        public List<IDeleteArtifactResult> Delete(IUser user = null, List<HttpStatusCode> expectedStatusCodes = null, bool deleteChildren = false)
        {
            ThrowIf.ArgumentNull(user, nameof(user));

            string path = I18NHelper.FormatInvariant("{0}/{1}/{2}/{3}", SVC_PATH, ProjectId, URL_ARTIFACTS, Id);

            if (deleteChildren)
            {
                path = I18NHelper.FormatInvariant("{0}?Recursively=True", path);
            }

            RestApiFacade restApi = new RestApiFacade(Address, user.Username, user.Password, token: user.Token?.OpenApiToken);
            var artifactResults = restApi.SendRequestAndDeserializeObject<List<DeleteArtifactResult>>(
                path,
                RestRequestMethod.DELETE, 
                expectedStatusCodes: expectedStatusCodes);

            foreach (var deletedArtifact in artifactResults)
            {
                Logger.WriteDebug("DELETE {0} returned following: ArtifactId: {1} Message: {2}, ResultCode: {3}", path, deletedArtifact.ArtifactId, deletedArtifact.Message, deletedArtifact.ResultCode);
            }

            return artifactResults.ConvertAll(o => (IDeleteArtifactResult)o);
        }

        public bool IsArtifactPublished(IUser user, List<HttpStatusCode> expectedStatusCodes = null)
        {
            ThrowIf.ArgumentNull(user, nameof(user));

            RestApiFacade restApi = new RestApiFacade(Address, user.Username, user.Password);
            var path = I18NHelper.FormatInvariant("{0}/{1}/{2}/{3}", SVC_PATH, ProjectId, URL_ARTIFACTS, Id);
            var returnedArtifact = restApi.SendRequestAndDeserializeObject<OpenApiArtifact>(
                resourcePath: path, method: RestRequestMethod.GET, expectedStatusCodes: expectedStatusCodes);

            //for unpublished artifact Version is -1
            return (returnedArtifact.Version != -1);
        }

        #endregion Methods
    }
}
