using System.IO;
using ServiceLibrary.Models;
using System.Threading.Tasks;

namespace AdminStore.Services.Metadata
{
    public interface IMetadataService
    {

        Task<byte[]> GetIcon(string type, int? typeId = null, string color = null);
    }
}
