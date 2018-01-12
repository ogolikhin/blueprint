using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace AdminStore.Services.Metadata
{
    public interface IMetadataService
    {
        Task<ByteArrayContent> GetCustomItemTypeIcon(int itemTypeId, int revisionId = int.MaxValue);

        void GetItemTypeIcon(int? typeId);
    }
}
