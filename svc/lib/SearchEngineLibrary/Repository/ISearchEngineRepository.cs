using System.Collections.Generic;
using System.Threading.Tasks;

namespace SearchEngineLibrary.Repository
{
    public interface ISearchEngineRepository
    {
        Task<IEnumerable<int>> GetArtifactIds();
    }
}
