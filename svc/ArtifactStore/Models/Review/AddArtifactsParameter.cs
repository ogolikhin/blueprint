using System.Collections.Generic;

namespace ArtifactStore.Models.Review
{
    public class AddArtifactsParameter
    {
        public IEnumerable<int> ArtifactIds { get; set; }

        public bool AddChildren { get; set; }
    }
}
