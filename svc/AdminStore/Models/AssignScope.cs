using System.Collections.Generic;
using System.Linq;


using AdminStore.Models.Enums;
using ServiceLibrary.Models.Enums;

namespace AdminStore.Models
{
    public class AssignScope
    {
        public bool SelectAll { get; set; }

        public IEnumerable<KeyValuePair<int, UserType>> Members { get; set; }

        public bool IsEmpty()
        {
            if (!SelectAll && (Members == null || !Members.Any()))
            {
                return true;
            }
            if (SelectAll && Members == null)
            {
                return true;
            }
            return false;
        }
    }
}