﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArtifactStore.Models.Review
{
    public class ReviewArtifactViewedInput
    {
        public IEnumerable<int> ArtifactIds { get; set; }
        public bool? Viewed { get; set; }
    }
}
