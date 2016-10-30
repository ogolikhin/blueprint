using System.Collections.Generic;

namespace Model.SearchServiceModel.Impl
{
    public class ItemSearchResult
    {
        public int PageItemCount { get; set; }

        public List<SearchItem> Items {get;} = new List<SearchItem>();
    }

    public class SearchItem
    {
        public int Id { get; set; }
        public int? ProjectId { get; set; }
        public int ParentId { get; set; }
        public int ItemTypeId { get; set; }
        public string Prefix { get; set; }
        public int PredefinedType { get; set; }
        public int Version { get; set; }
        public double OrderIndex { get; set; }
        public int ItemId { get; set; }

        //public int? ArtifactId { get; set; }

        
        //public string TypeName { get; set; }

        public string TypePrefix { get; set; }

        public string Name { get; set; }

        public bool ShouldSerializeItemTypeId()
        {
            return (ItemTypeId != 0);
        }

        public string Path { get; set; }
    }

    public class ProjectSearchResult
    {
        public List<SearchItem> Items { get; } = new List<SearchItem>();
    }
}
