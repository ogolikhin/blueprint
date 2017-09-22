using System.Collections.Generic;
using ServiceLibrary.Models;

namespace SearchService.Models
{
    public class SuggestionsSearchResult
    {
        public int SourceId { get; set; }

        public List<IArtifact> SuggestedArtifacts { get; } = new List<IArtifact>();
    }
}