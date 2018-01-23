using ServiceLibrary.Models;
using System.Threading.Tasks;

namespace AdminStore.Services.Metadata
{
    public interface IMetadataService
    {

        Task<Icon> GetIcon(string type, int? typeId = null, string color = null);
    }
}
