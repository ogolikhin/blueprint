using System.Xml.Linq;
using ServiceLibrary.Models;

namespace AdminStore.Repositories.Metadata
{
    public interface IMetadataRepository
    {
        byte[] GetSvgIconContent(ItemTypePredefined predefined, string color);
    }
}
