using System.Collections.Generic;
namespace ArtifactStore.Models.Review
{
    public class ReviewArtifact
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int ArtifactTypeId { get; set; }
        public string ArtifactTypeName { get; set; }
    }

    public class ReviewContent
    {
        public IEnumerable<ReviewArtifact> Items { get; set; }
        public int Total { get; set; }
    }
}