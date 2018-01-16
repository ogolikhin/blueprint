using System.IO;
using ServiceLibrary.Models;

namespace AdminStore.Repositories.Metadata
{
    public interface IMetadataRepository
    {
        Stream GetSvgIcon(ItemTypePredefined predefined, string color);
    }
}
