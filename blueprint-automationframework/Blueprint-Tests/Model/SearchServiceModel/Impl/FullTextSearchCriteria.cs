﻿using System.Collections.Generic;

namespace Model.SearchServiceModel.Impl
{
    public class SearchCriteria
    {
        /// <summary>
        /// The criteria for the search request
        /// </summary>
        public string Query { get; set; }
    }

    public class FullTextSearchCriteria : SearchCriteria
    {
        #region JSON serialzied properties
        
        /// <summary>
        /// The ids of the projects to include in the search scope.
        /// </summary>
        public IEnumerable<int> ProjectIds { get; set; }

        /// <summary>
        /// The ids of the base artifact types to include in the search scope.
        /// </summary>
        public IEnumerable<int> PredefinedTypeIds { get; set; }

        /// <summary>
        /// The ids of the artifact types to include in the search scope.
        /// </summary>
        public IEnumerable<int> ItemTypeIds { get; set; }

        /// <summary>
        /// Should return Project Path for the results.
        /// </summary>
        public bool? IncludeArtifactPath { get; set; }

        #endregion JSON serialized properties

        public FullTextSearchCriteria()
        {
            // for deserialization
        }

        public FullTextSearchCriteria(string query, IEnumerable<int> projectIds, IEnumerable<int> predefinedTypeIds = null, IEnumerable<int> itemTypeIds = null)
        {
            Query = query;
            ProjectIds = projectIds;
            PredefinedTypeIds = predefinedTypeIds;
            ItemTypeIds = itemTypeIds;
            IncludeArtifactPath = true;
        }

        public FullTextSearchCriteria(string query, IEnumerable<int> projectIds, IEnumerable<int> itemTypeIds = null)
        {
            Query = query;
            ProjectIds = projectIds;
            ItemTypeIds = itemTypeIds;
            IncludeArtifactPath = true;
        }

        public FullTextSearchCriteria(string query, int projectId, IEnumerable<int> itemTypeIds = null) :
            this(query, new List<int>() { projectId }, itemTypeIds)
        {
        }

        public FullTextSearchCriteria(string query, int projectId, int itemTypeId) :
            this(query, new List<int>() { projectId }, new List<int>() { itemTypeId })
        {
        }

        public FullTextSearchCriteria(string query, IEnumerable<int> projectIds, int predefinedTypeId) :
            this(query, projectIds, predefinedTypeIds: new List<int>() { predefinedTypeId })
        {
        }

        public FullTextSearchCriteria(string query, IEnumerable<int> projectIds)
        {
            Query = query;
            ProjectIds = projectIds;
        }
    }

    public class ItemNameSearchCriteria : SearchCriteria
    {
        #region JSON serialzied properties

        /// <summary>
        /// The ids of the projects to include in the search scope.
        /// </summary>
        public IEnumerable<int> ProjectIds { get; set; }

        /// <summary>
        /// The ids of the base artifact types to include in the search scope.
        /// </summary>
        public IEnumerable<int> PredefinedTypeIds { get; set; }

        /// <summary>
        /// Should return Project Path for the results.
        /// </summary>
        public bool? IncludeArtifactPath { get; set; }

        #endregion JSON serialized properties

        public ItemNameSearchCriteria()
        {
            // for deserialization
        }

        public ItemNameSearchCriteria(string query, IEnumerable<int> projectIds, IEnumerable<int> predefinedTypeIds = null)
        {
            Query = query;
            ProjectIds = projectIds;
            PredefinedTypeIds = predefinedTypeIds;
            IncludeArtifactPath = true;
        }
                
        public ItemNameSearchCriteria(string query, int projectId, IEnumerable<int> itemTypeIds = null) :
            this(query, new List<int>() { projectId }, itemTypeIds)
        {
        }

        public ItemNameSearchCriteria(string query, int projectId, int itemTypeId) :
            this(query, new List<int>() { projectId }, new List<int>() { itemTypeId })
        {
        }

        public ItemNameSearchCriteria(string query, IEnumerable<int> projectIds, int predefinedTypeId) :
            this(query, projectIds, predefinedTypeIds: new List<int>() { predefinedTypeId })
        {
        }

        public ItemNameSearchCriteria(string query, IEnumerable<int> projectIds)
        {
            Query = query;
            ProjectIds = projectIds;
        }
    }
}