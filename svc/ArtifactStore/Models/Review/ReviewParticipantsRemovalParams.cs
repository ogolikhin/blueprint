using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ArtifactStore.Models.Review
{
    public class ReviewParticipantsRemovalParams
    {
        public IEnumerable<int> ItemIds { get; set; }
        public SelectionType SelectionType { get; set; }
    }
}