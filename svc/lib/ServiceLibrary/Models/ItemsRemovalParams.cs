using System.Collections.Generic;

namespace ServiceLibrary.Models
{
    public enum SelectionType
    {
        Selected,
        Excluded
    }
    public class ItemsRemovalParams
    {
        public IEnumerable<int> ItemIds { get; set; }
        public SelectionType SelectionType { get; set; }
    }
}