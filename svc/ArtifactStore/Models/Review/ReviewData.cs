namespace ArtifactStore.Models.Review
{
    public class ReviewData
    {
        public int Id { get; set; }

        public string ReviewPackageRawDataXml { get; set; }

        public string ReviewContentsXml { get; set; }

        public int? BaselineId { get; set; }
    }
}