using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using ServiceLibrary.Models;
using ServiceLibrary.Models.Enums;
using SearchEngineLibrary.Model;

namespace SearchEngineLibrary.Service
{
    public interface ISearchEngineService
    {
        Task<SearchArtifactsResult> Search(int scopeId, Pagination pagination, ScopeType scopeType, bool includeDrafts, int userId, IDbTransaction transaction = null);
    }
}
