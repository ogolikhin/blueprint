namespace ArtifactStore.Models.Review
{
    public class ParticipantMeaningOfSignatureResult : IMeaningOfSignatureValue
    {
        public int ParticipantId { get; set; }
        public int GroupId { get; set; }
        public int RoleId { get; set; }
        public string RoleName { get; set; }
        public int RoleAssignmentId { get; set; }
        public int MeaningOfSignatureId { get; set; }
        public string MeaningOfSignatureValue { get; set; }
    }
}
