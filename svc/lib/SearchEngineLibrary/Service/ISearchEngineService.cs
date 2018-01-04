using System.Collections.Generic;
using System.Threading.Tasks;
using ServiceLibrary.Models;
using ServiceLibrary.Models.Enums;

namespace SearchEngineLibrary.Service
{
    public interface ISearchEngineService
    {
        Task<IEnumerable<int>> SearchArtifactIds(int scopeId, Pagination pagination, ScopeType scopeType, bool includeDraft, int userId);
    }
}
