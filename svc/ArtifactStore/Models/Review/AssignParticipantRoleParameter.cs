namespace ArtifactStore.Models.Review
{
    public class AssignParticipantRoleParameter
    {
        public int UserId { get; set; }

        public ReviewParticipantRole Role { get; set; }
    }
}