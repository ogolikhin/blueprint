using ServiceLibrary.Models;
using System.Collections.Generic;
using System.Xml.Linq;

namespace AdminStore.Repositories.Metadata
{
    public interface IMetadataRepository
    {
        List<XElement> getSvgXaml(ItemTypePredefined predefined);
    }
}
