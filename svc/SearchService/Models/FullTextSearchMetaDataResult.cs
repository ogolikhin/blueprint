using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Newtonsoft.Json;

namespace SearchService.Models
{
    public class FullTextSearchMetaDataResult
    {
        public int TotalCount { get; set; }
        public int TotalPages { get; set; }
        public int PageSize { get; set; }

        public IEnumerable<FullTextSearchTypeItem> FullTextSearchTypeItems { get; set; }
    }
}