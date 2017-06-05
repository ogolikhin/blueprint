using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using AdminStore.Models.Enums;

namespace AdminStore.Models
{
    public class AssignScope
    {
        public bool SelectAll { get; set; }
        public IEnumerable<KeyValuePair<int, UserType>> Types { get; set; }

        public bool IsEmpty()
        {
            return !SelectAll && (Types == null || !Types.Any());
        }
    }
}