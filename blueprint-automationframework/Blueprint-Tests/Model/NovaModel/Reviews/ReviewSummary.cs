using Model.NovaModel.Reviews.Enums;		
		
namespace Model.NovaModel.Reviews
{		
    // see blueprint/svc/ArtifactStore/Models/Review/ReviewSummary.cs		
    public class ReviewSummary
    {		
        public int Id { get; set; }		
		
        public string Name { get; set; }		
		
        public string Prefix { get; set; }		
		
        public string ArtifactType { get; set; }		
		
        public string Description { get; set; }		
		
        public int TotalArtifacts { get; set; }		
		
        public ReviewType ReviewType { get; set; }		
		
        public ReviewParticipantRole? ReviewParticipantRole { get; set; }		
		
        public ReviewSource Source { get; set; }		
		
        public ReviewPackageStatus ReviewPackageStatus { get; set; }		
		
        public ReviewStatus Status { get; set; }		
		
        public ReviewArtifactsStatus ArtifactsStatus { get; set; }		
		
        public int RevisionId { get; set; }		
    }		
}