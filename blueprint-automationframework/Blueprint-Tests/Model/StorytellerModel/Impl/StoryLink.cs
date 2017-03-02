namespace Model.StorytellerModel.Impl
{
    public class StoryLink
    {
        /// <summary>
        /// The Artifact Id of referenced User Story
        /// </summary>
        public int AssociatedReferenceArtifactId { get; set; }

        /// <summary>
        /// The Destination Id of the Story Link
        /// </summary>
        public int DestinationId { get; set; }

        /// <summary>
        /// The vertical order index
        /// </summary>
        public double Orderindex { get; set; }

        /// <summary>
        /// The Source Id of the Story Link
        /// </summary>
        public int SourceId { get; set; }

        /// <summary>
        /// Storylink Constructor
        /// </summary>
        /// <param name="sourceId">The source id of the story link</param>
        /// <param name="destinationId">The destination id of the story link</param>
        /// <param name="orderIndex">The vertical order index</param>
        /// <param name="associatedReferenceId">The artifact id of referenced user story</param>
        public StoryLink(int sourceId, int destinationId, double orderIndex, int associatedReferenceId)
        {
            AssociatedReferenceArtifactId = associatedReferenceId;
            DestinationId = destinationId;
            Orderindex = orderIndex;
            SourceId = sourceId;
        }
    }
}