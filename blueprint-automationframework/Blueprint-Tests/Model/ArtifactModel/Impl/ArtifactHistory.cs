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
        public List<ArtifactHistoryVersion> ArtifactHistoryVersions { get; } = new List<ArtifactHistoryVersion>();
    }

    public class ArtifactHistoryVersion
    {
        public long VersionId { get; set; }
        public int UserId { get; set; }
        public string DisplayName { get; set; }
        public bool HasUserIcon { get; set; }
        public DateTime Timestamp { get; set; }
        public ArtifactState ArtifactState { get; set; }
    }

    public enum ArtifactState
    {
        Published = 0,
        Draft = 1,
        Deleted = 2
    }
}
