using System.Linq;
using Model.ArtifactModel;
using Model.ArtifactModel.Enums;
using Model.ArtifactModel.Impl.PredefinedProperties;
using Model.Common.Enums;
using Newtonsoft.Json;
using NUnit.Framework;
using Utilities;

namespace Model.ModelHelpers
{
    public class DocumentArtifactWrapper : ArtifactWrapper, INovaArtifactDetails
    {
        #region Constructors

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="artifact">The artifact to wrap.</param>
        /// <param name="artifactStore">The ArtifactStore to use for REST calls.</param>
        /// <param name="svcShared">The SvcShared to use for REST calls.</param>
        /// <param name="project">The project where the artifact was created.</param>
        /// <param name="createdBy">The user who created the artifact.</param>
        /// <exception cref="AssertionException">If the Project ID of the artifact is different than the ID of the IProject, or if the artifact isn't a Document.</exception>
        public DocumentArtifactWrapper(INovaArtifactDetails artifact, IArtifactStore artifactStore, ISvcShared svcShared, IProject project, IUser createdBy)
            : base(artifact, artifactStore, svcShared, project, createdBy)
        {
            ThrowIf.ArgumentNull(artifact, nameof(artifact));
            ThrowIf.ArgumentNull(project, nameof(project));

            Assert.AreEqual((int)ItemTypePredefined.Document, artifact.PredefinedType, "The artifact being wrapped must be a Document artifact!");
        }

        #endregion Constructors

        /// <summary>
        /// Gets or sets the DocumentFile property for Artifact of Document type.
        /// TODO: replace this and GetActorInheritance function with generic function
        /// </summary>
        [JsonIgnore]
        public DocumentFileValue DocumentFile
        {
            get
            {
                // Finding DocumentFile among other properties
                var documentFileProperty = SpecificPropertyValues?.FirstOrDefault(
                    p => p.PropertyType == PropertyTypePredefined.DocumentFile);

                // Deserialization
                //string documentFilePropertyString = documentFileProperty.CustomPropertyValue.ToString();
                //var documentFilePropertyValue = JsonConvert.DeserializeObject<DocumentFileValue>(documentFilePropertyString);
                //CheckIsJsonChanged<DocumentFileValue>(documentFileProperty);

                return (DocumentFileValue) documentFileProperty?.CustomPropertyValue;
            }

            set
            {
                // Finding DocumentFile among other properties
                var documentFileProperty = SpecificPropertyValues?.FirstOrDefault(
                    p => p.PropertyType == PropertyTypePredefined.DocumentFile);

                if (documentFileProperty != null)   // TODO: Should this throw an exception instead?
                {
                    documentFileProperty.CustomPropertyValue = value;
                }
            }
        }
    }
}
