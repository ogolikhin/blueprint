using System.Collections.Generic;

namespace AdminStore.Models
{
    public class OperationScope
    {
        public bool SelectAll { get; set; }
        public IEnumerable<int> Ids { get; set; }
    }
}