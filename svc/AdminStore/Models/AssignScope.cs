using System.Collections.Generic;
using System.Linq;


using AdminStore.Models.Enums;

namespace AdminStore.Models
{
    public class AssignScope
    {
        public bool SelectAll { get; set; }

        public IEnumerable<KeyValuePair<int, UserType>> Members { get; set; }

        public bool IsEmpty()
        {
            return !SelectAll && (Members == null || !Members.Any());
        }
    }
}