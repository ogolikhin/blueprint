using System.Collections.Generic;
using System.Linq;

namespace AdminStore.Models
{
    public class OperationScope
    {
        public bool SelectAll { get; set; }
        public IEnumerable<int> Ids { get; set; }

        public bool IsUseless()
        {
            return ((Ids == null || (!Ids.ToList().Any() && SelectAll == false)));
        }
    }
}