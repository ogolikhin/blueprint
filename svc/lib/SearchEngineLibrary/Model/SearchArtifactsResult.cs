using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SearchEngineLibrary.Model
{
    public class SearchArtifactsResult
    {
        public int Total { get; set; }
        public IEnumerable<int> ArtifactIds { get; set; }
    }
}
