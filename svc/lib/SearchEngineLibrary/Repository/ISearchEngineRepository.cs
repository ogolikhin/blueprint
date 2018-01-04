using ServiceLibrary.Models;
using ServiceLibrary.Models.Enums;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SearchEngineLibrary.Repository
{
    public interface ISearchEngineRepository
    {
        Task<IEnumerable<int>> GetArtifactIds(int scopeId, Pagination pagination, ScopeType scopeType, bool includeDraft, int userId);
    }
}
