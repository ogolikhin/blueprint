using System.Collections.Generic;

namespace ArtifactStore.Models.Review
{
    public class AssignArtifactsApprovalParameter
    {
        public IEnumerable<ArtifactApprovalRequiredInfo> ArtifactsApprovalRequiredInfo { get; set; }
    }

    public class ArtifactApprovalRequiredInfo
    {
        public int Id { get; set; }

        public bool ApprovalRequired { get; set; }
    }


}
