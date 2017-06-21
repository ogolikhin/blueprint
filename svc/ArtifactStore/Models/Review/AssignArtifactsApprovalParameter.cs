using System.Collections.Generic;

namespace ArtifactStore.Models.Review
{
    public class AssignArtifactsApprovalParameter
    {
        public IEnumerable<int> ArtifactIds { get; set; }

        public bool ApprovalRequired { get; set; }
    }

}
