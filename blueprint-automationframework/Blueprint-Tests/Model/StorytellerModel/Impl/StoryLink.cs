using NUnit.Framework;
using Utilities;

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

        /// <summary>
        /// Assert that Story Links are equal
        /// </summary>
        /// <param name="link1">The first Story Link</param>
        /// <param name="link2">The Story Link being compared to the first</param>
        public static void AssertAreEqual(StoryLink link1, StoryLink link2)
        {
            ThrowIf.ArgumentNull(link1, nameof(link1));
            ThrowIf.ArgumentNull(link2, nameof(link2));

            Assert.AreEqual(link1.AssociatedReferenceArtifactId, link2.AssociatedReferenceArtifactId, "Link associated reference artifact ids do not match");
            Assert.AreEqual(link1.DestinationId, link2.DestinationId, "Link destinations do not match");
            Assert.AreEqual(link1.SourceId, link2.SourceId, "Link sources do not match");
            Assert.AreEqual(link1.Orderindex, link2.Orderindex, "Link order indexes do not match");
        }
    }
}