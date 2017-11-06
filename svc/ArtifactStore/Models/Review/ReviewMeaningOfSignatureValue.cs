namespace ArtifactStore.Models.Review
{
    public class ReviewMeaningOfSignatureValue : IMeaningOfSignatureValue
    {
        public int Id { get; set; }
        public int MeaningOfSignatureId { get; set; }
        public string MeaningOfSignatureValue { get; set; }
        public int RoleId { get; set; }
        public string RoleName { get; set; }
    }
}
