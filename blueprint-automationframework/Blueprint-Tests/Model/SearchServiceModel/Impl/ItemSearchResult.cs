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

        public bool ShouldSerializeId()
        {
            return (Id != 0);
        }

        public int? ProjectId { get; set; }

        public int ParentId { get; set; }

        public bool ShouldSerializeParentId()
        {
            return (ParentId != 0);
        }

        public int ItemTypeId { get; set; }

        public bool ShouldSerializeItemTypeId()
        {
            return (ItemTypeId != 0);
        }

        public string Prefix { get; set; }

        public int PredefinedType { get; set; }

        public bool ShouldSerializePredefinedType()
        {
            return (PredefinedType != 0);
        }

        public int Version { get; set; }

        public bool ShouldSerializeVersion()
        {
            return (Version != 0);
        }

        public double OrderIndex { get; set; }

        public bool ShouldSerializeOrderIndex()
        {
            return (OrderIndex != 0);
        }

        public int Permissions { get; set; }

        public bool ShouldSerializePermissions()
        {
            return (Permissions != 0);
        }

        public int ItemId { get; set; }

        public string TypePrefix { get; set; }

        public string Name { get; set; }

        public string Path { get; set; }
    }

    public class ProjectSearchResult
    {
        public List<SearchItem> Items { get; } = new List<SearchItem>();
    }
}
