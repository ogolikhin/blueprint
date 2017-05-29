using System.Collections.Generic;
using Newtonsoft.Json;

namespace Model.NovaModel.Reviews
{
    // see blueprint/svc/ArtifactStore/Models/Review/ReviewContent.cs
    public class ReviewArtifact : BaseReviewArtifact
    {
        public bool IsApprovalRequired { get; set; }
        public int Pending { get; set; }
        public int Approved { get; set; }
        public int Disapproved { get; set; }
        public int Viewed { get; set; }
        public int Unviewed { get; set; }
    }
}
