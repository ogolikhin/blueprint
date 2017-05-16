using System.Collections.Generic;
using System.Linq;

namespace AdminStore.Models
{
    public class OperationScope
    {
        public bool SelectAll { get; set; }
        public IEnumerable<int> Ids { get; set; }
        public bool IsSelectionEmpty()
        {
            return !SelectAll && (Ids == null || !Ids.Any());
        }
    }
}