using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ArtifactStore.Models.Review
{
    public enum SelectionType
    {
        Selected,
        Excluded
    }
    public class ReviewItemsRemovalParams
    {
        public IEnumerable<int> ItemIds { get; set; }
        public SelectionType SelectionType { get; set; }
    }
}