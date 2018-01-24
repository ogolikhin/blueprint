using System.Threading.Tasks;
using AdminStore.Repositories.Metadata;
using ServiceLibrary.Exceptions;
using ServiceLibrary.Models;
using ServiceLibrary.Repositories;
using ServiceLibrary.Services.Image;
using System;
using System.Globalization;
using ServiceLibrary.Helpers.Cache;
using ServiceLibrary.Models.Enums;
using ServiceLibrary.Models.ItemType;

namespace AdminStore.Services.Metadata
{
    public class MetadataService : IMetadataService
    {
        private readonly ISqlItemTypeRepository _sqlItemTypeRepository;
        private readonly IMetadataRepository _metadataRepository;
        private readonly IImageService _imageService;
        private readonly IItemTypeIconCache _cache;

        private const int ItemTypeIconSize = 32;

        public MetadataService()
            : this(
                 new SqlItemTypeRepository(),
                 new MetadataRepository(),
                 new ImageService(),
                 ItemTypeIconCache.Instance)
        {
        }

        public MetadataService(ISqlItemTypeRepository sqlItemTypeRepository,
             IMetadataRepository metadataRepository,
             IImageService imageService,
             IItemTypeIconCache cache)
        {
            _sqlItemTypeRepository = sqlItemTypeRepository;
            _metadataRepository = metadataRepository;
            _imageService = imageService;
            _cache = cache;
        }

        public async Task<Icon> GetIcon(string type, int? typeId = null, string color = null)
        {


            var iconType = IconType.None;
            if (string.IsNullOrEmpty(type) || !Enum.TryParse(type, true, out iconType))
            {
                throw new BadRequestException("Unknown item type");
            }

            if ((typeId == null && iconType == IconType.Artifact) || iconType == IconType.None)
            {
                throw new BadRequestException("Unknown item type");
            }

            string hexColor = string.Format(CultureInfo.CurrentCulture, "#{0}", color);


            byte[] iconContent = null;

            if (_cache != null)
            {
                iconContent = _cache.GetValue(iconType, typeId, hexColor);
            }

            if (iconContent != null)
            {
                return new Icon
                {
                    Content = iconContent,
                    IsSvg = (typeId == null)
                };
            }
            var icon = new Icon();

            if (iconType == IconType.InstanceFolder)
            {
                icon = GetItemTypeIcon(ItemTypePredefined.PrimitiveFolder, hexColor);
            }
            else if (iconType == IconType.Project)
            {
                icon = GetItemTypeIcon(ItemTypePredefined.Project, hexColor);
            }
            else
            {
                var itemTypeInfo = await GetItemTypeInfo(typeId.GetValueOrDefault());

                if (itemTypeInfo.HasCustomIcon)
                {
                    icon = new Icon
                    {
                        Content = _imageService.ConvertBitmapImageToPng(itemTypeInfo.Icon, ItemTypeIconSize,
                            ItemTypeIconSize),
                        IsSvg = false,
                        ItemTypeId = itemTypeInfo.Id,
                        ItemTypePredefined = itemTypeInfo.Predefined
                    };
                }
                else
                {
                    icon = GetItemTypeIcon(itemTypeInfo.Predefined, hexColor);
                }
            }

            return icon;
        }

        private async Task<ItemTypeInfo> GetItemTypeInfo(int itemTypeId, int revisionId = int.MaxValue)
        {
            var itemTypeInfo = await _sqlItemTypeRepository.GetItemTypeInfo(itemTypeId, revisionId);

            if (itemTypeInfo == null)
            {
                throw new ResourceNotFoundException("Artifact type not found.");
            }

            return itemTypeInfo;
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
                IsSvg = true,

            };
        }

    }
}
