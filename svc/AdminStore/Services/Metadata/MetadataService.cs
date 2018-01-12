using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using AdminStore.Repositories.Metadata;
using AdminStore.Services.Metadata;
using ServiceLibrary.Exceptions;
using ServiceLibrary.Helpers;
using ServiceLibrary.Models;

namespace ServiceLibrary.Repositories.Metadata
{
    public class MetadataService : IMetadataService
    {
        // private readonly IMetadataRepository _metadataRepository;
        private readonly ISqlItemTypeRepository _sqlItemTypeRepository;

        private const int ItemTypeIconSize = 32;

        public MetadataService()
            : this(
                new SqlItemTypeRepository())
                // new MetadataRepository())
        {
        }

        public MetadataService(ISqlItemTypeRepository sqlItemTypeRepository)
            // IMetadataRepository metadataRepository)
        {
            _sqlItemTypeRepository = sqlItemTypeRepository;
            // _metadataRepository = metadataRepository;
        }

        public async Task<ByteArrayContent> GetCustomItemTypeIcon(int itemTypeId, int revisionId = int.MaxValue)
        {
            var itemTypeInfo = await _sqlItemTypeRepository.GetItemTypeInfo(itemTypeId, revisionId);

            if (itemTypeInfo == null)
            {
                throw new ResourceNotFoundException("Artifact type not found.");
            }

            byte[] data = null;

            data = ImageHelper.ConvertBitmapImageToPng(itemTypeInfo.Icon.ToArray(), ItemTypeIconSize, ItemTypeIconSize);

            return ImageHelper.CreateByteArrayContent(data);
        }

        public void GetItemTypeIcon(int? typeId)
        {
            throw new NotImplementedException();
        }
    }
}
