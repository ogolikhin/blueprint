
namespace Model.NovaModel.Impl
{
    // see blueprint/svc/ArtifactStore/Models/Review/ReviewContainer.cs
    public class ReviewContainer
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string Prefix { get; set; }

        public string ArtifactType { get; set; }

        public string Description { get; set; }

        public int TotalArtifacts { get; set; }

        public ReviewType ReviewType { get; set; }

        public ReviewSourceType SourceType { get; set; }

        public ReviewSource Source { get; set; }

        public ReviewPackageStatus ReviewPackageStatus { get; set; }

        public ReviewStatus Status { get; set; }

        public ReviewArtifactsStatus ArtifactsStatus { get; set; }

        public int RevisionId { get; set; }
    }

    public class ReviewSource
    {
        public int Id { get; set; }

        public string Prefix { get; set; }

        public string Name { get; set; }
    }

    public class ReviewArtifactsStatus
    {
        public int Approved { get; set; }

        public int Disapproved { get; set; }

        public int Viewed { get; set; }
    }

    public enum ReviewType
    {
        Informal = 0,
        Formal = 1
    }

    public enum ReviewSourceType
    {
        Baseline = 0,
        Collection = 1,
        LiveArtifacts = 2
    }

    public enum ReviewStatus
    {
        NotStarted = 0,
        InProgress = 1,
        Completed = 2
    }

    public enum ReviewPackageStatus
    {
        Draft = 0,
        Active = 1,
        Closed = 2
    }
}
