using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Model.ArtifactModel.Impl;

namespace Model.SearchServiceModel.Impl
{
    public class ItemSearchResult
    {
        public int PageItemCount { get; set; }

        public List<SearchItem> Items {get;} = new List<SearchItem>();
    }

    public class SearchItem
    {
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public int? Id { get; set; }

        public int? ProjectId { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public int? ParentId { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public int? ItemTypeId { get; set; }

        public string Prefix { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public int? ItemTypeIconId { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public int? PredefinedType { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public int? Version { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public double? OrderIndex { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public bool? HasChildren { get; set; }
        
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public int? Permissions { get; set; }
        
        public Identification LockedByUser { get; set; }

        public DateTime? LockedDateTime { get; set; }

        public List<SearchItem> Children { get; } = new List<SearchItem>();
        public bool ShouldSerializeChildren()
        {
            return Children.Count > 0;
        }

        public int ItemId { get; set; }

        public string Name { get; set; }

        public string Path { get; set; }
    }

    public class ProjectSearchResult
    {
        public List<SearchItem> Items { get; } = new List<SearchItem>();
    }
}
