using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ArtifactStore.Collections.Models
{
    public class RemoveArtifactsFromCollectionResult
    {
        public int Total { get; set; }

        public int RemovedCount { get; set; }
    }
}