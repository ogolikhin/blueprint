using System.Collections.Generic;

namespace Model.NovaModel.Reviews
{
    // see blueprint/svc/ArtifactStore/Models/Review/AddArtifactsParameter.cs
    public class AddArtifactsParameter
    {
        public List<int> ArtifactIds { get; set; }

        public bool AddChildren { get; set; }
    }
}
