using System.Collections.Generic;
using System.Threading.Tasks;
using ServiceLibrary.Models;

namespace ServiceLibrary.Repositories
{
    public interface ISqlItemInfoRepository
    {
        Task<IEnumerable<ItemLabel>> GetItemsLabels(int userId, IEnumerable<int> itemIds, bool addDrafts = true, int revisionId = int.MaxValue);

        Task<IEnumerable<ItemDetails>> GetItemsDetails(int userId, IEnumerable<int> itemIds, bool addDrafts = true, int revisionId = int.MaxValue);

        Task<IEnumerable<ItemRawDataCreatedDate>> GetItemsRawDataCreatedDate(int userId, IEnumerable<int> itemIds, bool addDrafts = true, int revisionId = int.MaxValue);

        Task<int> GetRevisionId(int artifactId, int userId, int? versionId = null, int? baselineId = null);
    }
}