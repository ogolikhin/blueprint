using System.Collections.Generic;
using ServiceLibrary.Models;

namespace ArtifactStore.Models.Review
{
    public class AssignArtifactsApprovalParameter : ItemsRemovalParams
    {
        public bool ApprovalRequired { get; set; }
    }

}
