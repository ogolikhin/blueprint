using ServiceLibrary.Models;

namespace ArtifactStore.Models.Review
{
    public class AssignParticipantRoleParameter : ItemsRemovalParams
    {
        public ReviewParticipantRole Role { get; set; }
    }
}