using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SearchService.Models
{
    public class ItemSearchResult
    {        
        /// <summary>
        /// Number of items contained in this Page
        /// </summary>
        public int PageItemCount { get; set; }

        public IEnumerable<ItemSearchResultItem> SearchItems { get; set; }
    }
}