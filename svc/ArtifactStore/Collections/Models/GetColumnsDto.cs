using System.Collections.Generic;

namespace ArtifactStore.Collections.Models
{
    public class GetColumnsDto
    {
        public IEnumerable<ArtifactListColumn> SelectedColumns { get; set; }

        public IEnumerable<ArtifactListColumn> OtherColumns { get; set; }
    }
}
