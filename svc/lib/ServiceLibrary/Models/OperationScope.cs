using System.Collections.Generic;
using System.Linq;

namespace ServiceLibrary.Models
{
    public class OperationScope
    {
        public bool SelectAll { get; set; }

        public IEnumerable<int> Ids { get; set; } = Enumerable.Empty<int>();

        public bool IsEmpty()
        {
            return !SelectAll && !Ids.Any();
        }
    }
}
