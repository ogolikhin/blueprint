using System.Collections.Generic;

namespace Model.NovaModel.Reviews
{
    public class AddArtifactsParameter
    {
        public List<int> ArtifactIds { get; set; }

        public bool AddChildren { get; set; }
    }
}
