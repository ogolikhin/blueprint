using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.ArtifactModel.Impl
{
    public class ArtifactHistory
    {
        public int artifactId { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public List<ArtifactHistoryVersion> artifactHistoryVersions { get; set; }
    }

    public class ArtifactHistoryVersion
    {
        public long versionId { get; set; }
        public int userId { get; set; }
        public string displayName { get; set; }
        public bool hasUserIcon { get; set; }
        public string timestamp { get; set; }
        public int artifactState { get; set; }
    }
}
