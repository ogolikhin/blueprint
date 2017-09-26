using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using SearchService.Models;

namespace SearchService.Helpers.SemanticSearch
{
    public class SqlSearchEngine: SearchEngine
    {
        protected SqlConnection Connection { get; }
        public SqlSearchEngine(string connectionString)
        {
            Connection = new SqlConnection(connectionString);
        }

        public override IEnumerable<ArtifactSearchResult> GetSemanticSearchSuggestions(int artifactId, bool isInstanceAdmin, HashSet<int> projectIds)
        {
            throw new NotImplementedException();
        }
    }
}