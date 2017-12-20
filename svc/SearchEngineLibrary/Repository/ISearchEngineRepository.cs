using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SearchEngineLibrary.Model;

namespace SearchEngineLibrary.Repository
{
    public interface ISearchEngineRepository
    {
        Task<IEnumerable<int>> GetArtifactIdsFromSearchItems();
    }
}
