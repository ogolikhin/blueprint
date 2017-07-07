using System.Collections.Generic;
using System.Linq;

namespace ServiceLibrary.Models
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
