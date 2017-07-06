using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AdminStore.Models
{
    public class TabularPagination
    {
        public int Offset { get; set; }
        public int Limit { get; set; }
    }
}