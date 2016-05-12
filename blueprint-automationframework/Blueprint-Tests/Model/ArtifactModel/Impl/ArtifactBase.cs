using System;
using Utilities;

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
    }
}