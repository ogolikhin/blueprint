using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ArtifactStore.ArtifactList.Models;

namespace ArtifactStore.Collections.Models
{
    public class CollectionData
    {
        public CollectionArtifacts CollectionArtifacts { get; set; }

        public ProfileColumns ProfileColumns { get; set; }
    }
}