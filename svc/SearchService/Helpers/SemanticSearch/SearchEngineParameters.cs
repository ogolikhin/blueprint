using System.Collections.Generic;

namespace SearchService.Helpers.SemanticSearch
{
    public class SearchEngineParameters
    {
        public int ArtifactId { get; }
        public int UserId { get; }
        public bool IsInstanceAdmin { get; }
        public HashSet<int> AccessibleProjectIds { get; }
        public int PageSize { get; }

        public SearchEngineParameters(
            int artifactId, 
            int userId, 
            bool isInstanceAdmin,
            HashSet<int> accessibleProjectIds,
            int pageSize = 10)
        {
            ArtifactId = artifactId;
            UserId = userId;
            IsInstanceAdmin = isInstanceAdmin;
            AccessibleProjectIds = accessibleProjectIds;
            PageSize = pageSize;
        }
    }
}