using ServiceLibrary.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace AdminStore.Services.Metadata
{
    public interface IMetadataService
    {
        Task<ByteArrayContent> GetCustomItemTypeIcon(int itemTypeId, int revisionId = int.MaxValue);

        List<XElement> GetItemTypeIcon(ItemTypePredefined predefined);
    }
}
