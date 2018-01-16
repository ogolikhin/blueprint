using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AdminStore.Repositories.Metadata;
using ServiceLibrary.Exceptions;
using ServiceLibrary.Helpers;
using ServiceLibrary.Models;
using ServiceLibrary.Repositories;

namespace AdminStore.Services.Metadata
{
    public class MetadataService : IMetadataService
    {
        private readonly ISqlItemTypeRepository _sqlItemTypeRepository;
        private readonly IMetadataRepository _metadataRepository;


        private const int ItemTypeIconSize = 32;

        public MetadataService()
            : this(
                new SqlItemTypeRepository(),
                 new MetadataRepository())
        {
        }

        public MetadataService(ISqlItemTypeRepository sqlItemTypeRepository,
             IMetadataRepository metadataRepository)
        {
            _sqlItemTypeRepository = sqlItemTypeRepository;
            _metadataRepository = metadataRepository;
        }

        public async Task<byte[]> GetCustomItemTypeIcon(int itemTypeId, int revisionId = int.MaxValue)
        {
            var itemTypeInfo = await _sqlItemTypeRepository.GetItemTypeInfo(itemTypeId, revisionId);

            if (itemTypeInfo == null)
            {
                throw new ResourceNotFoundException("Artifact type not found.");
            }
            return ImageHelper.ConvertBitmapImageToPng(itemTypeInfo.Icon.ToArray(), ItemTypeIconSize, ItemTypeIconSize);
        }

        public Stream GetItemTypeIcon(ItemTypePredefined predefined, string color)
        {
            return _metadataRepository.GetSvgIcon(predefined, color);
        }
    }
}
