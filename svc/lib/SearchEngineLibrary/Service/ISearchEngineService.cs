using System.Collections.Generic;
using System.Threading.Tasks;

namespace SearchEngineLibrary.Service
{
    public interface ISearchEngineService
    {
        Task<IEnumerable<int>> GetArtifactIds();
    }
}
