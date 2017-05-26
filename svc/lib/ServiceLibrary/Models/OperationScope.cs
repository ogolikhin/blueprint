using System.Collections.Generic;
using System.Linq;

namespace ServiceLibrary.Models
{
    public class OperationScope
    {
        public bool SelectAll { get; set; }

        public IEnumerable<int> Ids { get; set; }

        public bool IsEmpty()
        {
            return !SelectAll && (Ids == null || !Ids.Any());
        }
    }
}
