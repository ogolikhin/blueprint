using System.Collections.Generic;
using SearchService.Models;

namespace SearchService.Helpers.SemanticSearch
{
    public interface ISearchEngine
    {
        IEnumerable<ArtifactSearchResult> GetSemanticSearchSuggestions(int artifactId, bool isInstanceAdmin, HashSet<int> projectIds);
    }
    public abstract class SearchEngine: ISearchEngine
    {
        //protected string ConnectionString { get; }

        //protected SearchEngine(string connectionString)
        //{
        //    ConnectionString = connectionString;
        //}
        public abstract IEnumerable<ArtifactSearchResult> GetSemanticSearchSuggestions(int artifactId,
            bool isInstanceAdmin, HashSet<int> projectIds);

    }
}