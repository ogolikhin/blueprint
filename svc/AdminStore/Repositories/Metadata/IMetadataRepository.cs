using System.Xml.Linq;
using ServiceLibrary.Models;

namespace AdminStore.Repositories.Metadata
{
    public interface IMetadataRepository
    {
        XDocument GetSvgIcon(ItemTypePredefined predefined, string color);
    }
}
