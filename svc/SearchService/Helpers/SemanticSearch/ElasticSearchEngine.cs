using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using SearchService.Models;

namespace SearchService.Helpers.SemanticSearch
{
    public class ElasticSearchEngine: SearchEngine
    {
        protected const string IndexPrefix = "semanticsdb_";
        public string IndexName { get; }

        public ElasticSearchEngine(string connectionString, string tenantId)
        {
            IndexName = IndexPrefix + tenantId;
            // create ElasticClient using conneciton string
        }

        public override IEnumerable<ArtifactSearchResult> GetSemanticSearchSuggestions(int artifactId, bool isInstanceAdmin, HashSet<int> projectIds)
        {
            throw new NotImplementedException();
        }
    }
}