using System.Data;
using System.Threading.Tasks;
using SearchEngineLibrary.Model;
using ServiceLibrary.Models;

namespace SearchEngineLibrary.Repository
{
    public interface ISearchEngineRepository
    {
        Task<SearchArtifactsResult> GetCollectionContentSearchArtifactResults(int scopeId, int projectId, Pagination pagination, bool includeDrafts, int userId, IDbTransaction transaction = null);
    }
}
