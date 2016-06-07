using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ArtifactStore.Models
{
    public class ArtifactHistoryResultSet
    {
        public int ArtifactId;
        public IEnumerable<ArtifactHistoryVersion> ArtifactHistoryVersions;
    }
    public class ArtifactHistoryVersion
    {
        public string DisplayName { get; set; }
        public int VersionNumber { get; set; }
        public DateTime Timestamp { get; set; }
        public bool HasUserIcon { get; set; }
    }
}