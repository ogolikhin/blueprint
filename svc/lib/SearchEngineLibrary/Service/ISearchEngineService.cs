using System.Data;
using System.Threading.Tasks;
using SearchEngineLibrary.Model;
using ServiceLibrary.Models;
using ServiceLibrary.Models.Enums;

namespace SearchEngineLibrary.Service
{
    public interface ISearchEngineService
    {
        Task<SearchArtifactsResult> Search(int scopeId, int projectId, Pagination pagination, ScopeType scopeType, bool includeDrafts, int userId, IDbTransaction transaction = null);
    }
}
