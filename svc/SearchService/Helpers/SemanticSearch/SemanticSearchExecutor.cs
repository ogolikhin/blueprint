using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using SearchService.Models;

namespace SearchService.Helpers.SemanticSearch
{
    public delegate Task<IEnumerable<int>> GetSemanticSearchSuggestionsDelegate(int artifactId);
    public interface ISemanticSearchExecutor
    {
        Task<IEnumerable<ArtifactSearchResult>> GetSemanticSearchSuggestions(int artifactId);
    }
    public class SemanticSearchExecutor: ISemanticSearchExecutor
    {
        private static readonly Lazy<SemanticSearchExecutor> _instance = new Lazy<SemanticSearchExecutor>(() => new SemanticSearchExecutor());
        public static SemanticSearchExecutor Instance => _instance.Value;

        public SemanticSearchExecutor()
        {
            
        }
        public Task<IEnumerable<ArtifactSearchResult>> GetSemanticSearchSuggestions(int artifactId)
        {
            throw new NotImplementedException();
        }
    }
}