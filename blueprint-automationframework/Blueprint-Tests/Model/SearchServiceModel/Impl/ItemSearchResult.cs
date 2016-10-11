using System.Collections.Generic;

namespace Model.SearchServiceModel.Impl
{
    public class ItemSearchResult
    {
        public int PageItemCount { get; set; }

        public List<SearchItem> SearchItems {get;} = new List<SearchItem>();
    }

    public class SearchItem
    {
        public int ProjectId { get; set; }

        public int ArtifactId { get; set; }

        public int ItemId { get; set; }

        public string Name { get; set; }

        public int ItemTypeId { get; set; }

        public string TypeName { get; set; }

        public string TypePrefix { get; set; }
    }
}
