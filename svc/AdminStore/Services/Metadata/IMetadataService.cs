using System.IO;
using ServiceLibrary.Models;
using System.Threading.Tasks;

namespace AdminStore.Services.Metadata
{
    public interface IMetadataService
    {
        Task<byte[]> GetCustomItemTypeIcon(int itemTypeId, int revisionId = int.MaxValue);

        Stream GetItemTypeIcon(ItemTypePredefined predefined, string color);
    }
}
