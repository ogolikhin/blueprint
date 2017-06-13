using System.Collections.Generic;

namespace Model.NovaModel.Reviews
{
    // Taken from blueprint/svc/ArtifactStore/Models/Review/ReviewTableOfContent.cs
    public class ReviewTableOfContent
    {
        public IEnumerable<ReviewTableOfContentItem> Items { get; set; }
        public int Total { get; set; }
    }
}
