using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AdminStore.Models
{
    public class ChangeSet
    {
        public IEnumerable<int> Removed { get; set; }
    }
}