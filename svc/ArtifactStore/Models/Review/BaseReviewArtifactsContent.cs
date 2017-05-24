using System.Collections.Generic;

namespace ArtifactStore.Models.Review
{
    public class BaseReviewArtifactsContent<T> where T : BaseReviewArtifact
    {
        public IEnumerable<T> Items { get; set; }
        public int Total { get; set; }
    }
}