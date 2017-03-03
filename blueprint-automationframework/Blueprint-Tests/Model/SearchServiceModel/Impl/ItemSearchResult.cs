using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Model.ArtifactModel.Impl;

namespace Model.SearchServiceModel.Impl
{
    // see blueprint/svc/SearchService/Models/ProjectSearchResultSet.cs and blueprint/svc/SearchService/Models/ItemNameSearchResult.cs 
    public class SearchResult
    {
        public int ItemId { get; set; }
        
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string Name { get; set; }
    }

    // see blueprint/svc/SearchService/Models/ProjectSearchResultSet.cs and blueprint/svc/SearchService/Models/ItemNameSearchResult.cs 
    public class ItemNameSearchResult : SearchResult
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

        public List<ItemNameSearchResult> Children { get; } = new List<ItemNameSearchResult>();
        public bool ShouldSerializeChildren()
        {
            return Children.Count > 0;
        }

        public List<string> Path { get; } = new List<string>();

        public bool ShouldSerializePath()
        {
            return Path.Count > 0;
        }

        public List<int> IdPath { get; } = new List<int>();

        public bool ShouldSerializeIdPath()
        {
            return Path.Count > 0;
        }
    }

    // see blueprint/svc/SearchService/Models/ProjectSearchResultSet.cs and blueprint/svc/SearchService/Models/ItemNameSearchResult.cs 
    public class ProjectSearchResult : SearchResult
    {
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string Path { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string Description { get; set; }
    }

    // see blueprint/svc/SearchService/Models/ProjectSearchResultSet.cs and blueprint/svc/SearchService/Models/ItemNameSearchResult.cs 
    public class ItemNameSearchResultSet
    {
        public int PageItemCount { get; set; }

        public List<ItemNameSearchResult> Items { get; } = new List<ItemNameSearchResult>();
    }

    // see blueprint/svc/SearchService/Models/ProjectSearchResultSet.cs and blueprint/svc/SearchService/Models/ItemNameSearchResult.cs 
    public class ProjectSearchResultSet
    {
        public List<ProjectSearchResult> Items { get; } = new List<ProjectSearchResult>();
    }
}
