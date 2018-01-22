using System.Linq;
using System.Threading.Tasks;
using AdminStore.Repositories.Metadata;
using ServiceLibrary.Exceptions;
using ServiceLibrary.Models;
using ServiceLibrary.Repositories;
using ServiceLibrary.Services.Image;
using System;
using System.Globalization;
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

        public async Task<Icon> GetIcon(string type, int? typeId = null, string color = null)
        {
            var itemType = ItemTypePredefined.None;
            if (string.IsNullOrEmpty(type) || !Enum.TryParse(type, true, out itemType))
            {
                throw new BadRequestException("Unknown item type");
            }

            if (typeId != null)
            {
                var customIcon = await GetCustomItemTypeIcon(typeId.GetValueOrDefault());

                if (customIcon != null)
                {
                    return customIcon;
                }
            }

            var icon = GetItemTypeIcon(itemType, color);

            return icon;
        }

        private async Task<Icon> GetCustomItemTypeIcon(int itemTypeId, int revisionId = int.MaxValue)
        {
            var itemTypeInfo = await _sqlItemTypeRepository.GetItemTypeInfo(itemTypeId, revisionId);

            if (itemTypeInfo == null)
            {
                throw new ResourceNotFoundException("Artifact type not found.");
            }

            if (!itemTypeInfo.HasCustomIcon)
            {
                return null;
            }

            return new Icon
            {
                Content = _imageService.ConvertBitmapImageToPng(itemTypeInfo.Icon.ToArray(), ItemTypeIconSize, ItemTypeIconSize),
                IsSvg = false
            };
        }

        private Icon GetItemTypeIcon(ItemTypePredefined predefined, string color)
        {
            var iconContent = _metadataRepository.GetSvgIconContent(predefined, color);
            if (iconContent == null)
            {
                throw new ResourceNotFoundException("Artifact type icon Content not found.");
            }

            return new Icon
            {
                Content = iconContent,
                IsSvg = true
            };
        }

    }
}
