using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net;
using System.Reflection;
using Common;
using Utilities;
using Utilities.Facades;

namespace Model.ArtifactModel.Impl
{
    public class ArtifactBase : IArtifactBase
    {
        #region Constants
        public const string URL_LOCK = "svc/shared/artifacts/lock";
        public const string URL_DISCUSSIONS = "/svc/components/RapidReview/artifacts/{0}/discussions";
        public const string URL_SEARCH = "/svc/shared/artifacts/search";
        public const string URL_NOVADISCARD = "/svc/shared/artifacts/discard";
        public const string URL_ARTIFACT_INFO = "/svc/components/storyteller/artifactInfo";
        public const string URL_DIAGRAM = "svc/components/RapidReview/diagram";
        public const string URL_USECASE = "svc/components/RapidReview/usecase";
        public const string URL_GLOSSARY = "svc/components/RapidReview/glossary";
        public const string URL_ARTIFACTPROPERTIES = "svc/components/RapidReview/artifacts/properties";
        private const string URL_NAVIGATION = "svc/shared/navigation";

        public const string SessionTokenCookieName = "BLUEPRINT_SESSION_TOKEN";

        #endregion Constants

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
        public IUser CreatedBy { get; set; }
        public bool IsPublished { get; set; }
        public bool IsSaved { get; set; }

        //TODO  Check if we can remove the setters and get rid of these warnings

        //TODO  Check if we can modify properties to do public List Attachments { get; } = new List(); instead of in constructor

        [SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        [JsonConverter(typeof(Deserialization.ConcreteConverter<List<OpenApiProperty>>))]
        public List<OpenApiProperty> Properties { get; set; }

        [SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        [JsonConverter(typeof(Deserialization.ConcreteConverter<List<OpenApiComment>>))]
        public List<OpenApiComment> Comments { get; set; }

        [SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        [JsonConverter(typeof(Deserialization.ConcreteConverter<List<OpenApiTrace>>))]
        public List<OpenApiTrace> Traces { get; set; }

        [SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        [JsonConverter(typeof(Deserialization.ConcreteConverter<List<OpenApiAttachment>>))]
        public List<OpenApiAttachment> Attachments { get; set; }

        #endregion Properties

        #region Constructors

        /// <summary>
        /// Constructor in order to use it as generic type
        /// </summary>
        public ArtifactBase()
        {
            IsSaved = false;
            IsPublished = false;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="address">The URI address of the artifact.</param>
        public ArtifactBase(string address) : this()
        {
            ThrowIf.ArgumentNull(address, nameof(address));
            Address = address;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="address">The base url of the API</param>
        /// <param name="id">The artifact id</param>
        /// <param name="projectId">The project containing the artifact</param>
        public ArtifactBase(string address, int id, int projectId) : this(address)
        {
            Id = id;
            ProjectId = projectId;
        }

        #endregion Constructors

        #region Public Methods

        public List<ArtifactReference> GetNavigation(
            IUser user,
            List<IArtifact> artifacts,
            List<HttpStatusCode> expectedStatusCodes = null
            )
        {
            return GetNavigation(Address, user, artifacts, expectedStatusCodes);
        }

        #endregion Public Methods

        #region Static Methods

        /// <summary>
        /// Get ArtifactReference list which is used to represent breadcrumb navigation
        /// </summary>
        /// <param name="address">The base url of the API</param>
        /// <param name="user">The user credentials for breadcrumb navigation</param>
        /// <param name="artifacts">The list of artifacts used for breadcrumb navigation</param>
        /// <param name="expectedStatusCodes">(optional) Expected status codes for the request</param>
        /// <returns>The List of ArtifactReferences after the get navigation call</returns>
        /// <exception cref="WebException">A WebException sub-class if request call triggers an unexpected HTTP status code.</exception>
        public static List<ArtifactReference> GetNavigation(
            string address,
            IUser user,
            List<IArtifact> artifacts,
            List<HttpStatusCode> expectedStatusCodes = null
            )
        {
            ThrowIf.ArgumentNull(user, nameof(user));
            ThrowIf.ArgumentNull(artifacts, nameof(artifacts));

            string tokenValue = user.Token?.AccessControlToken;

            //Get list of artifacts which were created.
            List<int> artifactIds = artifacts.Select(artifact => artifact.Id).ToList();

            var path = I18NHelper.FormatInvariant("{0}/{1}", URL_NAVIGATION, String.Join("/", artifactIds));

            var restApi = new RestApiFacade(address, user.Username, user.Password, tokenValue);

            var response = restApi.SendRequestAndDeserializeObject<List<ArtifactReference>>(
                path,
                RestRequestMethod.GET,
                expectedStatusCodes: expectedStatusCodes);

            return response;
        }

        /// <summary>
        /// Replace properties in an artifact with properties from another artifact
        /// </summary>
        /// <param name="sourceArtifactBase">The artifact that is the source of the properties</param>
        /// <param name="destinationArtifactBase">The artifact that is the detination of the properties</param>
        public static void ReplacePropertiesWithPropertiesFromSourceArtifact(IArtifactBase sourceArtifactBase, IArtifactBase destinationArtifactBase)
        {
            ThrowIf.ArgumentNull(sourceArtifactBase, nameof(sourceArtifactBase));
            ThrowIf.ArgumentNull(destinationArtifactBase, nameof(destinationArtifactBase));

            // List of properties not to be replaced by the source artifact properties
            var propertiesNotToBeReplaced = new List<string>
            {
                "CreatedBy",
                "Address"
            };

            foreach (PropertyInfo propertyInfo in sourceArtifactBase.GetType().GetProperties())
            {
                PropertyInfo destinationPropertyInfo = destinationArtifactBase.GetType().GetProperties().First(p => p.Name == propertyInfo.Name);

                if (destinationPropertyInfo != null && destinationPropertyInfo.CanWrite && !propertiesNotToBeReplaced.Contains(destinationPropertyInfo.Name))
                {
                    var value = propertyInfo.GetValue(sourceArtifactBase);
                    destinationPropertyInfo.SetValue(destinationArtifactBase, value);
                }
            }
        }

        #endregion Static Methods
    }

    public static class ArtifactValidationMessage
    {
        public static readonly string ArtifactAlreadyLocked = "The artifact is locked by other user.";
        public static readonly string ArtifactAlreadyPublished = "Artifact {0} is already published in the project";
    }
}