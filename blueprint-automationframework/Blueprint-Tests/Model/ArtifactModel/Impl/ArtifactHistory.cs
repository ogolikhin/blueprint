using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.ArtifactModel.Impl
{
    public class ArtifactHistory
    {
        public int ArtifactId { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public List<ArtifactHistoryVersion> artifactHistoryVersions { get; set; }
    }

    public class ArtifactHistoryVersion
    {
        public long versionId { get; set; }
        public int userId { get; set; }
        public string displayName { get; set; }
        public bool hasUserIcon { get; set; }
        public DateTime timestamp { get; set; }
        public ArtifactState artifactState { get; set; }
    }

    public enum ArtifactState
    {
        Published = 0,
        Draft = 1,
        Deleted = 2
    }
}
