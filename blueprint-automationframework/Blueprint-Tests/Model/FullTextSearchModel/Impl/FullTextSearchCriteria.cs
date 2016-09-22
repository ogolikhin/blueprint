using System.Collections.Generic;

namespace Model.FullTextSearchModel.Impl
{
    public class FullTextSearchCriteria
    {
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
    }
}