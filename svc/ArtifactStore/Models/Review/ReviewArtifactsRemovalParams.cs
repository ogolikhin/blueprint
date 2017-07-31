using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ArtifactStore.Models.Review
{
    public enum SelectionType
    {
        Selected,
        Excluded
    }
    public class ReviewArtifactsRemovalParams
    {
        public IEnumerable<int> artifactIds { get; set; }
        public SelectionType SelectionType { get; set; }
    }
}