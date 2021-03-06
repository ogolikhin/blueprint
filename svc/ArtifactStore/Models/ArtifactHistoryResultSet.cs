﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ArtifactStore.Models
{
    public enum ArtifactState
    {
        Published = 0,
        Draft = 1,
        Deleted = 2
    }
    public class ArtifactHistoryResultSet
    {
        public int ArtifactId;
        public IEnumerable<ArtifactHistoryVersionWithUserInfo> ArtifactHistoryVersions;
    }
    public class ArtifactHistoryVersionWithUserInfo
    {
        public int VersionId { get; set; }
        public int UserId { get; set; }
        public string DisplayName { get; set; }
        public bool HasUserIcon { get; set; }
        public DateTime? Timestamp { get; set; }
        public ArtifactState ArtifactState { get; set; }
    }

    public class ArtifactHistoryVersion
    {
        public int VersionId { get; set; }
        public int UserId { get; set; }
        public DateTime? Timestamp { get; set; }
        public ArtifactState ArtifactState { get; set; }
    }

}