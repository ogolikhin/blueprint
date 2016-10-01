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
    }
}