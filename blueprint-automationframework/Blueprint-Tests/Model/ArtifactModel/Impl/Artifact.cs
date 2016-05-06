using System.Net;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Newtonsoft.Json;
using Utilities;
using NUnit.Framework;

namespace Model.ArtifactModel.Impl
{
    public class Artifact : ArtifactBase, IArtifact
    {

        #region Properties

        [SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        [JsonConverter(typeof(Deserialization.ConcreteConverter<List<Property>>))]
        public List<Property> Properties { get; set; }

        [SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        [JsonConverter(typeof(Deserialization.ConcreteConverter<List<Comment>>))]
        public List<Comment> Comments { get; set; }

        [SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        [JsonConverter(typeof(Deserialization.ConcreteConverter<List<Trace>>))]
        public List<Trace> Traces { get; set; }

        [SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        [JsonConverter(typeof(Deserialization.ConcreteConverter<List<Attachment>>))]
        public List<Attachment> Attachments { get; set; }

        #endregion Properties

        #region Constructors

        /// <summary>
        /// Constructor in order to use it as generic type
        /// </summary>
        public Artifact()
        {
            //Required for deserializing OpenApiArtifact
            Properties = new List<Property>();
            Comments = new List<Comment>();
            Traces = new List<Trace>();
            Attachments = new List<Attachment>();
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="address">The URI address of the artifact.</param>
        public Artifact(string address) : base(address)
        {
        }

        public Artifact(string address, int id, int projectId) : base(address, id, projectId)
        {
        }

        #endregion Constructors

        #region Methods

        public void Save(IUser user = null,
            List<HttpStatusCode> expectedStatusCodes = null,
            bool sendAuthorizationAsCookie = false)
        {
            if (user == null)
            {
                Assert.NotNull(CreatedBy, "No user is available to perform Save.");
                user = CreatedBy;
            }

            SaveArtifact(this, user, expectedStatusCodes, sendAuthorizationAsCookie);
        }

        public void Publish(IUser user = null,
            bool shouldKeepLock = false,
            List<HttpStatusCode> expectedStatusCodes = null,
            bool sendAuthorizationAsCookie = false)
        {
            if (user == null)
            {
                Assert.NotNull(CreatedBy, "No user is available to perform Publish.");
                user = CreatedBy;
            }

            var artifactToPublish = new List<IArtifactBase> { this };

            PublishArtifacts(artifactToPublish, Address, user, expectedStatusCodes, shouldKeepLock, sendAuthorizationAsCookie);
        }

        public List<IDiscardArtifactResult> Discard(IUser user = null,
            List<HttpStatusCode> expectedStatusCodes = null,
            bool sendAuthorizationAsCookie = false)
        {
            if (user == null)
            {
                Assert.NotNull(CreatedBy, "No user is available to perform Discard.");
                user = CreatedBy;
            }

            var artifactToDiscard = new List<IArtifactBase> { this };

            var discardArtifactResults = OpenApiArtifact.DiscardArtifacts(artifactToDiscard, Address, user, expectedStatusCodes, sendAuthorizationAsCookie);

            return discardArtifactResults;
        }

        public List<IDeleteArtifactResult> Delete(IUser user = null,
            List<HttpStatusCode> expectedStatusCodes = null,
            bool sendAuthorizationAsCookie = false,
            bool deleteChildren = false)
        {
            if (user == null)
            {
                Assert.NotNull(CreatedBy, "No user is available to perform Delete.");
                user = CreatedBy;
            }

            var deleteArtifactResults = DeleteArtifact(
                this,
                user,
                expectedStatusCodes,
                sendAuthorizationAsCookie,
                deleteChildren);

            return deleteArtifactResults;
        }

        public int GetVersion(IUser user = null,
            List<HttpStatusCode> expectedStatusCodes = null,
            bool sendAuthorizationAsCookie = false)
        {
            if (user == null)
            {
                Assert.NotNull(CreatedBy, "No user is available to perform GetVersion.");
                user = CreatedBy;
            }

            int artifactVersion = GetVersion(this, user, expectedStatusCodes, sendAuthorizationAsCookie);

            return artifactVersion;
        }

        #endregion Methods


        #region Static Methods

        public static void SaveArtifact(IArtifactBase artifactToSave,
            IUser user,
            List<HttpStatusCode> expectedStatusCodes = null,
            bool sendAuthorizationAsCookie = false)
        {
            OpenApiArtifact.SaveArtifact(artifactToSave, user, expectedStatusCodes, sendAuthorizationAsCookie);
        }

        /// <summary>
        /// Discard the added artifact(s) from Blueprint
        /// </summary>
        /// <param name="artifactsToDiscard">The artifact(s) to be discarded.</param>
        /// <param name="address">The base url of the Open API</param>
        /// <param name="user">The user to authenticate to Blueprint.</param>
        /// <param name="expectedStatusCodes">(optional) A list of expected status codes. If null, only OK: '200' is expected.</param>
        /// <param name="sendAuthorizationAsCookie">(optional) Flag to send authorization as a cookie rather than an HTTP header (Default: false)</param>
        /// <returns>The list of ArtifactResult objects created by the dicard artifacts request</returns>
        /// <exception cref="WebException">A WebException sub-class if request call triggers an unexpected HTTP status code.</exception>
        public static List<IDiscardArtifactResult> DiscardArtifacts(List<IArtifactBase> artifactsToDiscard,
            string address,
            IUser user,
            List<HttpStatusCode> expectedStatusCodes = null,
            bool sendAuthorizationAsCookie = false)
        {
            return OpenApiArtifact.DiscardArtifacts(
                artifactsToDiscard,
                address,
                user,
                expectedStatusCodes,
                sendAuthorizationAsCookie);
        }

        /// <summary>
        /// Publish Artifact(s) (Used when publishing a single artifact OR a list of artifacts)
        /// </summary>
        /// <param name="artifactsToPublish">The list of artifacts to publish</param>
        /// <param name="address">The base url of the Open API</param>
        /// <param name="user">The user credentials for the request</param>
        /// <param name="expectedStatusCodes">(optional) A list of expected status codes. If null, only OK: '200' is expected.</param>
        /// <param name="shouldKeepLock">(optional) Boolean parameter which defines whether or not to keep the lock after publishing the artfacts</param>
        /// <param name="sendAuthorizationAsCookie">(optional) Flag to send authorization as a cookie rather than an HTTP header (Default: false)</param>
        /// <returns>The list of PublishArtifactResult objects created by the publish artifacts request</returns>
        /// <exception cref="WebException">A WebException sub-class if request call triggers an unexpected HTTP status code.</exception>
        public static List<IPublishArtifactResult> PublishArtifacts(List<IArtifactBase> artifactsToPublish,
            string address,
            IUser user,
            List<HttpStatusCode> expectedStatusCodes = null,
            bool shouldKeepLock = false,
            bool sendAuthorizationAsCookie = false)
        {
            return OpenApiArtifact.PublishArtifacts(
                artifactsToPublish,
                address,
                user,
                expectedStatusCodes,
                shouldKeepLock,
                sendAuthorizationAsCookie);
        }

        public static List<IDeleteArtifactResult> DeleteArtifact(IArtifactBase artifactToDiscard,
            IUser user,
            List<HttpStatusCode> expectedStatusCodes = null,
            bool sendAuthorizationAsCookie = false,
            bool deleteChildren = false)
        {
            return OpenApiArtifact.DeleteArtifact(
                artifactToDiscard,
                user,
                expectedStatusCodes,
                sendAuthorizationAsCookie,
                deleteChildren);
        }

        public static int GetVersion(IArtifactBase artifact,
            IUser user = null,
            List<HttpStatusCode> expectedStatusCodes = null,
            bool sendAuthorizationAsCookie = false)
        {
            return OpenApiArtifact.GetVersion(artifact, user, expectedStatusCodes, sendAuthorizationAsCookie);
        }

        /// <summary>
        /// Get discussions for the specified artifact/subartifact
        /// </summary>
        /// <param name="address">server address</param>
        /// <param name="itemId">id of artifact/subartifact</param>
        /// <param name="includeDraft">false gets discussions for the last published version, true works with draft</param>
        /// <param name="user">The user credentials for the request</param>
        /// <param name="expectedStatusCodes">(optional) A list of expected status codes. If null, only OK: '200' is expected.</param>
        /// <param name="sendAuthorizationAsCookie">(optional) Flag to send authorization as a cookie rather than an HTTP header (Default: false)</param>
        /// <returns>Discussion for artifact/subartifact</returns>
        public static IDiscussion GetDiscussions(string address,
            int itemId,
            bool includeDraft,
            IUser user,
            List<HttpStatusCode> expectedStatusCodes = null,
            bool sendAuthorizationAsCookie = false)
        {
            return OpenApiArtifact.GetDiscussions(
                address,
                itemId,
                includeDraft,
                user,
                expectedStatusCodes,
                sendAuthorizationAsCookie);
        }

        /// <summary>
        /// Search artifact by a substring in its name on Blueprint server. Among published artifacts only.
        /// </summary>
        /// <param name="address">server address</param>
        /// <param name="user">The user to authenticate to Blueprint.</param>
        /// <param name="searchSubstring">The substring(case insensitive) to search.</param>
        /// <param name="sendAuthorizationAsCookie">(optional) Send session token as cookie instead of header</param>
        /// <param name="expectedStatusCodes">(optional) A list of expected status codes.</param>
        /// <returns>List of first 10 artifacts with name containing searchSubstring</returns>
        public static IList<IArtifactBase> SearchArtifactsByName(string address,
            IUser user,
            string searchSubstring,
            bool sendAuthorizationAsCookie = false,
            List<HttpStatusCode> expectedStatusCodes = null)
        {
            return OpenApiArtifact.SearchArtifactsByName(
                address,
                user,
                searchSubstring,
                sendAuthorizationAsCookie,
                expectedStatusCodes);
        }

        #endregion Static Methods
    }
}
