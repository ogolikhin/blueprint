using ServiceLibrary.Models;
using System.Threading.Tasks;

namespace AdminStore.Services.Metadata
{
    public interface IMetadataService
    {

        Task<Icon> GetIconAsync(string type, int? typeId = null, string color = null, int? imageId = null);
    }
}
