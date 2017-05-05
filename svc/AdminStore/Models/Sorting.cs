using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using AdminStore.Models.Enums;

namespace AdminStore.Models
{
    public class Sorting
    {
        public string Sort { get; set; }
        public SortOrder Order { get; set; }
    }
}