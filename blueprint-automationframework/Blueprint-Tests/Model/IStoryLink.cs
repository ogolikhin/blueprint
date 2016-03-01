using Newtonsoft.Json.Serialization;

namespace Model
{
    public interface IStoryLink
    {
        #region Properties

        /// <summary>
        /// The Artifact Id of referenced User Story
        /// </summary>
        int AssociatedReferenceArtifactId { get; set; }

        /// <summary>
        /// The Destination Id of the Story Link
        /// </summary>
        int DestinationId { get; set; }

        /// <summary>
        /// The vertical order index
        /// </summary>
        double Orderindex { get; set; }

        /// <summary>
        /// The Source Id of the Story Link
        /// </summary>
        int SourceId { get; set; }

        #endregion Properties
    }
}
