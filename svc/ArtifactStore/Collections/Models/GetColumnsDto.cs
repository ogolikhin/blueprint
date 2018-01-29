using System.Collections.Generic;
using ArtifactStore.ArtifactList.Models;

namespace ArtifactStore.Collections.Models
{
    public class GetColumnsDto
    {
        public IEnumerable<ProfileColumn> SelectedColumns { get; set; }

        public IEnumerable<ProfileColumn> UnselectedColumns { get; set; }
    }
}
