namespace ArtifactStore.Models.Review
{
    public class ReviewedArtifactMeaningOfSignature : IMeaningOfSignatureValue
    {
        public int ArtifactId { get; set; }
        public int MeaningOfSignatureId { get; set; }
        public string MeaningOfSignatureValue { get; set; }
        public int RoleId { get; set; }
        public string RoleName { get; set; }
    }
}
