using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AdminStore.Models
{
    public class TabularData
    {
        public Pagination Pagination { get; set; }
        public Sorting Sorting { get; set; }
        public string Search { get; set; }
    }
}