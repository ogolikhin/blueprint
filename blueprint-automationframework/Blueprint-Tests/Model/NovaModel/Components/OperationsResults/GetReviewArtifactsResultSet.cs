using System.Collections.Generic;

namespace Model.ArtifactModel.Impl.OperationsResults
{
    // see blueprint-current/Source/BluePrintSys.RC.Business.Internal/Components/Nova/Models/GetReviewArtifactsResultSet.cs
    public class ReviewArtifact
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int ArtifactTypeId { get; set; }
        public string ArtifactTypeName { get; set; }
    }

    public class GetReviewArtifactsResultSet
    {
        public List<ReviewArtifact> Items { get; set; }
        public int Total { get; set; }
    }
}
