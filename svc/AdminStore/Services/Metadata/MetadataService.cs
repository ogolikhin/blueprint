using System.Linq;
using System.Threading.Tasks;
using AdminStore.Repositories.Metadata;
using ServiceLibrary.Exceptions;
using ServiceLibrary.Models;
using ServiceLibrary.Repositories;
using ServiceLibrary.Services.Image;
using System;
using System.Text;

namespace AdminStore.Services.Metadata
{
    public class MetadataService : IMetadataService
    {
        private readonly ISqlItemTypeRepository _sqlItemTypeRepository;
        private readonly IMetadataRepository _metadataRepository;
        private readonly IImageService _imageService;

        private const int ItemTypeIconSize = 32;

        public MetadataService()
            : this(
                 new SqlItemTypeRepository(),
                 new MetadataRepository(),
                 new ImageService())
        {
        }

        public MetadataService(ISqlItemTypeRepository sqlItemTypeRepository,
             IMetadataRepository metadataRepository,
             IImageService imageService)
        {
            _sqlItemTypeRepository = sqlItemTypeRepository;
            _metadataRepository = metadataRepository;
            _imageService = imageService;
        }

        private async Task<byte[]> GetCustomItemTypeIcon(int itemTypeId, int revisionId = int.MaxValue)
        {
            var itemTypeInfo = await _sqlItemTypeRepository.GetItemTypeInfo(itemTypeId, revisionId);

            if (itemTypeInfo == null)
            {
                throw new ResourceNotFoundException("Artifact type not found.");
            }

            var customIcon = _imageService.ConvertBitmapImageToPng(itemTypeInfo.Icon.ToArray(), ItemTypeIconSize, ItemTypeIconSize);

            return customIcon;
        }

        private byte[] GetItemTypeIcon(ItemTypePredefined predefined, string color)
        {
            var resourceDocument = _metadataRepository.GetSvgIcon(predefined, color);
            byte[] toBytes = Encoding.ASCII.GetBytes(resourceDocument.ToString());

            return toBytes;
        }

        public async Task<byte[]> GetIcon(string type, int? typeId = null, string color = null)
        {
            var itemType = ItemTypePredefined.None;
            if (string.IsNullOrEmpty(type) || !Enum.TryParse(type, true, out itemType))
            {
                throw new BadRequestException("Unknown item type");
            }

            if (typeId == null)
            {
                return GetItemTypeIcon(itemType, color);
            }
            return await GetCustomItemTypeIcon(typeId.GetValueOrDefault());
        }
    }
}
