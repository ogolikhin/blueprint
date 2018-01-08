using SearchEngineLibrary.Model;
using ServiceLibrary.Models;
using ServiceLibrary.Models.Enums;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SearchEngineLibrary.Repository
{
    public interface ISearchEngineRepository
    {
        Task<SearchArtifactsResult> GetArtifactIds(int scopeId, Pagination pagination, bool includeDrafts, int userId);
    }
}
