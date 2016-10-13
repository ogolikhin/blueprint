using System.Collections.Generic;

namespace Model.SearchServiceModel.Impl
{
    public class FullTextSearchCriteria
    {
        #region JSON serialzied properties

        /// <summary>
        /// The criteria for the search request
        /// </summary>
        public string Query { get; set; }

        /// <summary>
        /// The ids of the projects to include in the search scope.
        /// </summary>
        public IEnumerable<int> ProjectIds { get; set; }

        /// <summary>
        /// The ids of the artifact types to include in the search scope.
        /// </summary>
        public IEnumerable<int> ItemTypeIds { get; set; }

        #endregion JSON serialized properties

        public FullTextSearchCriteria()
        {
            // for deserialization
        }

        public FullTextSearchCriteria(string query, IEnumerable<int> projectIds, IEnumerable<int> itemTypeIds = null)
        {
            this.Query = query;
            this.ProjectIds = projectIds;
            this.ItemTypeIds = itemTypeIds;
        }

        public FullTextSearchCriteria(string query, int projectId, IEnumerable<int> itemTypeIds = null) :
            this(query, new List<int>() { projectId }, itemTypeIds)
        {
        }

        public FullTextSearchCriteria(string query, int projectId, int itemTypeId) :
            this(query, new List<int>() { projectId }, new List<int>() { itemTypeId })
        {
        }

        public FullTextSearchCriteria(string query, IEnumerable<int> projectIds, int itemTypeId) :
            this(query, projectIds, new List<int>() { itemTypeId })
        {
        }
    }
}