using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ArtifactStore.Models
{
    public class GetArtifactHistoryResult
    {
        public int ArtifactId;
        public int Offset;
        public IEnumerable<ArtifactHistoryVersion> ArtifactHistoryVersions;
    }
    public class ArtifactHistoryVersion
    {
        public string DisplayName { get; set; }
        public int VersionNumber { get; set; }
        public DateTime Edit { get; set; }
        public DateTime Created { get; set; }
    }
}