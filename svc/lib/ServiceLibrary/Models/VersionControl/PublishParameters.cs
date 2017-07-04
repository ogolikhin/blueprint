using System.Collections.Generic;

namespace ServiceLibrary.Models.VersionControl
{
    public class PublishParameters
    {
        public int UserId { get; set; }
        public bool? All { get; set; }
        public IEnumerable<int> ArtifactIds { get; set; }
        public ISet<int> AffectedArtifactIds { get; } = new HashSet<int>();
    }
}
