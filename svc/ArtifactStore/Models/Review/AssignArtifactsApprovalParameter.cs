using System.Collections.Generic;

namespace ArtifactStore.Models.Review
{
    public class AssignArtifactsApprovalParameter :  ReviewItemsRemovalParams
    {
        public bool ApprovalRequired { get; set; }
    }

}
