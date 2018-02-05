using System.Threading.Tasks;
using AdminStore.Repositories.Metadata;
using ServiceLibrary.Exceptions;
using ServiceLibrary.Models;
using ServiceLibrary.Repositories;
using ServiceLibrary.Services.Image;
using System;
using System.Globalization;
using System.Text.RegularExpressions;
using ServiceLibrary.Helpers;
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
        private readonly IAsyncCache _cache;
        private readonly Regex _hexColorRegex = new Regex("^([A-Fa-f0-9]{6}|[A-Fa-f0-9]{3})$", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        private readonly DateTimeOffset _defaultExpirationOffset = DateTimeOffset.UtcNow.AddDays(1);
        private readonly string _prefix = "IconCache";

        private const int ItemTypeIconSize = 32;

        public MetadataService()
            : this(
                 new SqlItemTypeRepository(),
                 new MetadataRepository(),
                 new ImageService(),
                 AsyncCache.Default)
        {
        }

        public MetadataService(ISqlItemTypeRepository sqlItemTypeRepository,
             IMetadataRepository metadataRepository,
             IImageService imageService,
             IAsyncCache cache)
        {
            _sqlItemTypeRepository = sqlItemTypeRepository;
            _metadataRepository = metadataRepository;
            _imageService = imageService;
            _cache = cache;
        }

        public Task<Icon> GetIconAsync(string type, int? typeId = null, string color = null, int? imageId = null)
        {
            var iconType = ValidateInputParameter(type, typeId, color);

            var cacheKey = GetIconCacheKey(type, typeId, color, imageId);
            return _cache.AddOrGetExistingAsync(cacheKey, () => GetIconAsync(iconType, typeId, color), _defaultExpirationOffset);
        }

        private async Task<Icon> GetIconAsync(IconType iconType, int? typeId = null, string color = null)
        {
            Icon icon;
            var hexColor = string.Format(CultureInfo.CurrentCulture, "#{0}", color);
            switch (iconType)
            {
                case IconType.InstanceFolder:
                    icon = GetDefaultIcon(ItemTypePredefined.PrimitiveFolder, hexColor);
                    break;
                case IconType.Project:
                    icon = GetDefaultIcon(ItemTypePredefined.Project, hexColor);
                    break;
                case IconType.InstanceSubartifact:
                    icon = GetDefaultIcon(ItemTypePredefined.SubArtifactGroup, hexColor);
                    break;
                default:
                    var itemTypeInfo = await GetItemTypeInfoAsync(typeId.GetValueOrDefault());

                    if (itemTypeInfo.HasCustomIcon)
                    {
                        icon = new Icon
                        {
                            Content = _imageService.ConvertBitmapImageToPng(itemTypeInfo.Icon, ItemTypeIconSize,
                                ItemTypeIconSize),
                            IsSvg = false
                        };
                    }
                    else
                    {
                        icon = GetDefaultIcon(itemTypeInfo.Predefined, hexColor);
                    }
                    break;
            }
            return icon;
        }

        private string GetIconCacheKey(string type, int? typeId = null, string color = null, int? imageId = null)
        {
            return string.Format(CultureInfo.CurrentCulture, _prefix + "{0}{1}{2}{3}", type, typeId.GetValueOrDefault().ToString(CultureInfo.InvariantCulture), color, imageId);
        }

        private IconType ValidateInputParameter(string type, int? typeId = null, string color = null)
        {
            IconType iconType;
            if (string.IsNullOrEmpty(type) || !Enum.TryParse(type, true, out iconType))
            {
                throw new BadRequestException("Unknown item type.", ErrorCodes.BadRequest);
            }

            if (typeId == null && iconType == IconType.Artifact)
            {
                throw new BadRequestException("Unknown item type.", ErrorCodes.BadRequest);
            }
            if (!string.IsNullOrEmpty(color) && !_hexColorRegex.IsMatch(color))
            {
                throw new BadRequestException("Color parameter should have hex presentation.", ErrorCodes.BadRequest);
            }
            return iconType;
        }

        private async Task<ItemTypeInfo> GetItemTypeInfoAsync(int itemTypeId, int revisionId = int.MaxValue)
        {
            var itemTypeInfo = await _sqlItemTypeRepository.GetItemTypeInfoAsync(itemTypeId, revisionId);

            if (itemTypeInfo == null)
            {
                throw new ResourceNotFoundException("Artifact type not found.", ErrorCodes.ItemTypeNotFound);
            }

            return itemTypeInfo;
        }

        private Icon GetDefaultIcon(ItemTypePredefined predefined, string color)
        {
            var iconContent = _metadataRepository.GetSvgIconContent(predefined, color);
            if (iconContent == null)
            {
                throw new ResourceNotFoundException("Artifact type icon Content not found.", ErrorCodes.ResourceNotFound);
            }

            return new Icon
            {
                Content = iconContent,
                IsSvg = true
            };
        }

    }
}
