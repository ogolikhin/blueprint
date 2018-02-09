using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using ArtifactStore.ArtifactList;
using ArtifactStore.ArtifactList.Models;
using ArtifactStore.Collections.Helpers;
using ArtifactStore.Collections.Models;
using SearchEngineLibrary.Service;
using ServiceLibrary.Helpers;
using ServiceLibrary.Models;
using ServiceLibrary.Models.Enums;
using ServiceLibrary.Models.ProjectMeta;
using ServiceLibrary.Repositories;

namespace ArtifactStore.Collections
{
    public class CollectionsService : ICollectionsService
    {
        private readonly ICollectionsRepository _collectionsRepository;
        private readonly ILockArtifactsRepository _lockArtifactsRepository;
        private readonly IItemInfoRepository _itemInfoRepository;
        private readonly IArtifactPermissionsRepository _artifactPermissionsRepository;
        private readonly IArtifactRepository _artifactRepository;
        private readonly ISqlHelper _sqlHelper;
        private readonly ISearchEngineService _searchEngineService;
        private readonly IArtifactListService _artifactListService;

        public CollectionsService() : this(
            new SqlCollectionsRepository(),
            new SqlArtifactRepository(),
            new SqlLockArtifactsRepository(),
            new SqlItemInfoRepository(),
            new SqlArtifactPermissionsRepository(),
            new SqlHelper(),
            new SearchEngineService(),
            new ArtifactListService())
        {
        }

        public CollectionsService(
            ICollectionsRepository collectionsRepository,
            IArtifactRepository artifactRepository,
            ILockArtifactsRepository lockArtifactsRepository,
            IItemInfoRepository itemInfoRepository,
            IArtifactPermissionsRepository artifactPermissionsRepository,
            ISqlHelper sqlHelper,
            ISearchEngineService searchEngineService,
            IArtifactListService artifactListService)
        {
            _collectionsRepository = collectionsRepository;
            _artifactRepository = artifactRepository;
            _lockArtifactsRepository = lockArtifactsRepository;
            _itemInfoRepository = itemInfoRepository;
            _artifactPermissionsRepository = artifactPermissionsRepository;
            _sqlHelper = sqlHelper;
            _searchEngineService = searchEngineService;
            _artifactListService = artifactListService;
        }

        private async Task<Collection> GetCollectionAsync(
            int collectionId, int userId, IDbTransaction transaction = null)
        {
            var basicDetails = await _artifactRepository.GetArtifactBasicDetails(collectionId, userId, transaction);

            if (basicDetails == null || basicDetails.DraftDeleted || basicDetails.LatestDeleted)
            {
                throw CollectionsExceptionHelper.NotFoundException(collectionId);
            }

            if (basicDetails.PrimitiveItemTypePredefined != (int)ItemTypePredefined.ArtifactCollection)
            {
                throw CollectionsExceptionHelper.InvalidTypeException(collectionId);
            }

            var permissions = await _artifactPermissionsRepository.GetArtifactPermissions(
                collectionId, userId, transaction: transaction);

            RolePermissions collectionPermissions;

            if (!permissions.TryGetValue(collectionId, out collectionPermissions) ||
                !collectionPermissions.HasFlag(RolePermissions.Read))
            {
                throw CollectionsExceptionHelper.NoAccessException(collectionId);
            }

            return new Collection(
                basicDetails.ArtifactId,
                basicDetails.ProjectId,
                basicDetails.LockedByUserId,
                collectionPermissions);
        }

        public async Task<CollectionArtifacts> GetArtifactsInCollectionAsync(
            int collectionId, Pagination pagination, int userId)
        {
            var collection = await GetCollectionAsync(collectionId, userId);

            var searchArtifactsResult = await _searchEngineService.Search(
                collection.Id, pagination, ScopeType.Contents, true, userId);

            var artifacts = await _collectionsRepository.GetArtifactsWithPropertyValuesAsync(
                userId, searchArtifactsResult.ArtifactIds);

            var populatedArtifacts = PopulateArtifactsProperties(artifacts);
            populatedArtifacts.ItemsCount = searchArtifactsResult.Total;

            return populatedArtifacts;
        }

        public async Task<AddArtifactsToCollectionResult> AddArtifactsToCollectionAsync(
            int collectionId, IEnumerable<int> artifactIds, int userId)
        {
            if (collectionId < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(collectionId));
            }

            if (userId < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(userId));
            }

            AddArtifactsToCollectionResult result = null;

            Func<IDbTransaction, Task> action = async transaction =>
            {
                var collection = await ValidateCollectionAsync(collectionId, userId, transaction);

                var artifactIdsToAdd = artifactIds.ToList();
                var accessibleArtifacts = await GetAccessibleArtifactsAsync(artifactIdsToAdd, userId, transaction);
                var validArtifacts = accessibleArtifacts.Where(i => CanAddArtifactToCollection(i, collection)).ToList();
                var validArtifactIds = validArtifacts.Select(a => a.HolderId);

                var addedCount = await _collectionsRepository.AddArtifactsToCollectionAsync(
                    collection.Id, validArtifactIds, userId, transaction);

                result = new AddArtifactsToCollectionResult
                {
                    AddedCount = addedCount,
                    Total = artifactIdsToAdd.Count
                };
            };

            await _sqlHelper.RunInTransactionAsync(ServiceConstants.RaptorMain, action);

            return result;
        }

        public async Task<RemoveArtifactsFromCollectionResult> RemoveArtifactsFromCollectionAsync(
            int collectionId, ItemsRemovalParams removalParams, int userId)
        {
            if (collectionId < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(collectionId));
            }

            if (userId < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(userId));
            }

            RemoveArtifactsFromCollectionResult result = null;

            Func<IDbTransaction, Task> action = async transaction =>
            {
                var collection = await ValidateCollectionAsync(collectionId, userId, transaction);

                var searchArtifactsResult = await _searchEngineService.Search(
                    collection.Id, null, ScopeType.Contents, true, userId, transaction);

                var artifactsToRemove = removalParams.SelectionType == SelectionType.Selected ?
                    searchArtifactsResult.ArtifactIds.Intersect(removalParams.ItemIds).ToList() :
                    searchArtifactsResult.ArtifactIds.Except(removalParams.ItemIds).ToList();

                var accessibleArtifacts = await GetAccessibleArtifactsAsync(artifactsToRemove, userId, transaction);
                var accessibleArtifactIds = accessibleArtifacts.Select(a => a.HolderId);

                var removedCount = await _collectionsRepository.RemoveArtifactsFromCollectionAsync(
                    collection.Id, accessibleArtifactIds, userId, transaction);

                result = new RemoveArtifactsFromCollectionResult
                {
                    RemovedCount = removedCount,
                    Total = removalParams.ItemIds.Count()
                };
            };

            await _sqlHelper.RunInTransactionAsync(ServiceConstants.RaptorMain, action);

            return result;
        }

        public async Task<GetColumnsDto> GetColumnsAsync(int collectionId, int userId, string search = null)
        {
            var collection = await GetCollectionAsync(collectionId, userId);
            var artifacts = await GetContentArtifactDetailsAsync(collectionId, userId);

            if (artifacts.IsEmpty())
            {
                return new GetColumnsDto
                {
                    SelectedColumns = ProfileColumns.Default.Items,
                    UnselectedColumns = Enumerable.Empty<ProfileColumn>()
                };
            }

            var propertyTypeInfos = await GetPropertyTypeInfosAsync(artifacts, search);
            var profileColumns = await _artifactListService.GetProfileColumnsAsync(
                collection.Id, userId, ProfileColumns.Default);

            return new GetColumnsDto
            {
                SelectedColumns = GetSelectedColumns(propertyTypeInfos, profileColumns, search),
                UnselectedColumns = GetUnselectedColumns(propertyTypeInfos, profileColumns)
            };
        }

        public async Task SaveProfileColumnsAsync(int collectionId, ProfileColumns profileColumns, int userId)
        {
            if (userId < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(userId));
            }

            var collection = await GetCollectionAsync(collectionId, userId);

            var validColumns = await GetValidColumnsAsync(collectionId, userId, profileColumns);
            await _artifactListService.SaveProfileColumnsAsync(collection.Id, validColumns, userId);
        }

        public async Task<ProfileColumns> GetNoLongerApplicableProperties(int collectionId, int userId, ProfileColumnsDto profileColumnsDto)
        {
            var validColumns = await GetColumnsAsync(collectionId, userId);
            var validItems = validColumns.SelectedColumns.Union(validColumns.UnselectedColumns);

            return new ProfileColumns(ExcludeValidColumns(profileColumnsDto.Items, validItems));
        }

        private static IEnumerable<ProfileColumn> ExcludeValidColumns(IEnumerable<ProfileColumn> allProfileColumns,
            IEnumerable<ProfileColumn> validProfileColumns)
        {
            foreach (var allProfileColumn in allProfileColumns)
            {
                if (!validProfileColumns.Any(q => ((q.PropertyName == allProfileColumn.PropertyName) &&
                                                   (q.Predefined == allProfileColumn.Predefined) &&
                                                   (q.PrimitiveType == allProfileColumn.PrimitiveType) &&
                                                   (q.PropertyTypeId == allProfileColumn.PropertyTypeId))))
                    yield return allProfileColumn;
            }
        }

        private async Task<IReadOnlyList<ItemDetails>> GetContentArtifactDetailsAsync(int collectionId, int userId)
        {
            var artifactIds = await _collectionsRepository.GetContentArtifactIdsAsync(collectionId, userId);
            var artifactsInCollection = await _itemInfoRepository.GetItemsDetails(userId, artifactIds);

            return artifactsInCollection
                .Where(artifact => artifact.EndRevision == int.MaxValue || artifact.EndRevision == 1)
                .ToList();
        }

        private async Task<Collection> ValidateCollectionAsync(int collectionId, int userId, IDbTransaction transaction)
        {
            var collection = await GetCollectionAsync(collectionId, userId, transaction);

            if (!collection.Permissions.HasFlag(RolePermissions.Edit))
            {
                throw CollectionsExceptionHelper.NoEditPermissionException(collectionId);
            }

            await LockAsync(collection, userId, transaction);

            await _collectionsRepository.RemoveDeletedArtifactsFromCollectionAsync(collection.Id, userId, transaction);

            return collection;
        }

        private async Task<IReadOnlyList<PropertyTypeInfo>> GetPropertyTypeInfosAsync(
            IEnumerable<ItemDetails> artifacts, string search = null)
        {
            var itemTypeIds = artifacts.Select(artifact => artifact.ItemTypeId).Distinct();

            return await _collectionsRepository.GetPropertyTypeInfosForItemTypesAsync(itemTypeIds, search);
        }

        private async Task<ProfileColumns> GetValidColumnsAsync(
            int collectionId, int userId, ProfileColumns profileColumns)
        {
            var artifacts = await GetContentArtifactDetailsAsync(collectionId, userId);
            var propertyTypeInfos = await GetPropertyTypeInfosAsync(artifacts);

            return new ProfileColumns(
                profileColumns.Items
                    .Where(column => column.ExistsIn(propertyTypeInfos)));
        }

        private static IEnumerable<ProfileColumn> GetSelectedColumns(
            IReadOnlyList<PropertyTypeInfo> propertyTypeInfos, ProfileColumns profileColumns, string search)
        {
            return profileColumns.Items
                .Where(column => column.ExistsIn(propertyTypeInfos) && column.NameMatches(search))
                .Select(column => new ProfileColumn(
                    column.PropertyName, column.Predefined, column.PrimitiveType, column.PropertyTypeId));
        }

        private static IEnumerable<ProfileColumn> GetUnselectedColumns(
            IEnumerable<PropertyTypeInfo> propertyTypeInfos, ProfileColumns profileColumns = null)
        {
            return propertyTypeInfos
                .Select(info => info.IsCustom ?
                    CreateCustomPropertyColumn(info, profileColumns) :
                    CreateSystemPropertyColumn(info, profileColumns))
                .Where(column => column != null)
                .ToList();
        }

        private static ProfileColumn CreateSystemPropertyColumn(
            PropertyTypeInfo propertyTypeInfo, ProfileColumns profileColumns = null)
        {
            var systemPredefineds = new HashSet<PropertyTypePredefined>
            {
                PropertyTypePredefined.ID,
                PropertyTypePredefined.Name,
                PropertyTypePredefined.Description,
                PropertyTypePredefined.CreatedBy,
                PropertyTypePredefined.CreatedOn,
                PropertyTypePredefined.LastEditedBy,
                PropertyTypePredefined.LastEditedOn,
                PropertyTypePredefined.ArtifactType
            };

            if (!propertyTypeInfo.PredefinedMatches(systemPredefineds) ||
                profileColumns != null && profileColumns.PredefinedMatches(propertyTypeInfo.Predefined))
            {
                return null;
            }

            return new ProfileColumn(
                propertyTypeInfo.Name,
                propertyTypeInfo.Predefined,
                propertyTypeInfo.PrimitiveType);
        }

        private static ProfileColumn CreateCustomPropertyColumn(
            PropertyTypeInfo propertyTypeInfo, ProfileColumns profileColumns = null)
        {
            if (profileColumns != null && profileColumns.PropertyTypeIdMatches(propertyTypeInfo.Id.Value))
            {
                return null;
            }

            return new ProfileColumn(
                propertyTypeInfo.Name,
                propertyTypeInfo.Predefined,
                propertyTypeInfo.PrimitiveType,
                propertyTypeInfo.Id);
        }

        private static CollectionArtifacts PopulateArtifactsProperties(IReadOnlyList<CollectionArtifact> artifacts)
        {
            var artifactIdsResult = artifacts.Select(x => x.ArtifactId).Distinct().ToList();

            var artifactDtos = new List<ArtifactDto>();
            var settingsColumns = new List<ProfileColumn>();
            var areColumnsPopulated = false;

            foreach (var id in artifactIdsResult)
            {
                var artifactProperties = artifacts.Where(x => x.ArtifactId == id).ToList();

                var propertyInfos = new List<PropertyValueInfo>();
                int? itemTypeId = null;
                int? predefinedType = null;
                int? itemTypeIconId = null;

                foreach (var artifactProperty in artifactProperties)
                {
                    ProfileColumn profileColumn = null;
                    var propertyInfo = new PropertyValueInfo();

                    if (!areColumnsPopulated)
                    {
                        profileColumn = new ProfileColumn
                        {
                            PropertyName = artifactProperty.PropertyName,
                            Predefined = (PropertyTypePredefined)artifactProperty.PropertyTypePredefined,
                            PrimitiveType = artifactProperty.PrimitiveType.HasValue
                                ? (PropertyPrimitiveType)artifactProperty.PrimitiveType.Value
                                : (PropertyTypePredefined)artifactProperty.PropertyTypePredefined ==
                                  PropertyTypePredefined.ID
                                    ? PropertyPrimitiveType.Number
                                    : (PropertyTypePredefined)artifactProperty.PropertyTypePredefined ==
                                      PropertyTypePredefined.ArtifactType
                                        ? PropertyPrimitiveType.Choice
                                        : 0,
                            PropertyTypeId = artifactProperty.PropertyTypeId
                        };
                    }

                    propertyInfo.PropertyTypeId = artifactProperty.PropertyTypeId;
                    propertyInfo.Predefined = artifactProperty.PropertyTypePredefined;

                    if ((PropertyTypePredefined)artifactProperty.PropertyTypePredefined == PropertyTypePredefined.ID)
                    {
                        propertyInfo.Value = I18NHelper.FormatInvariant("{0}{1}", artifactProperty.Prefix,
                            artifactProperty.ArtifactId);

                        itemTypeId = artifactProperty.ItemTypeId;
                        predefinedType = artifactProperty.PredefinedType;
                        itemTypeIconId = artifactProperty.ItemTypeIconId;
                    }
                    else
                    {
                        propertyInfo.Value = artifactProperty.PropertyValue;
                    }

                    if (!areColumnsPopulated)
                    {
                        settingsColumns.Add(profileColumn);
                    }

                    propertyInfos.Add(propertyInfo);
                }

                areColumnsPopulated = true;

                artifactDtos.Add(new ArtifactDto
                {
                    ArtifactId = id,
                    ItemTypeId = itemTypeId,
                    PredefinedType = predefinedType,
                    ItemTypeIconId = itemTypeIconId,
                    PropertyInfos = propertyInfos.OrderBy(x => x.PropertyTypeId)
                });
            }

            if (!settingsColumns.Any())
            {
                settingsColumns = ProfileColumns.Default.Items.ToList();
            }

            return new CollectionArtifacts
            {
                Items = artifactDtos,
                ArtifactListSettings = new ArtifactListSettings
                {
                    Columns = settingsColumns.OrderBy(
                        x => Array.IndexOf(
                            ProfileColumns.Default.Items.Select(column => column.Predefined).ToArray(),
                            x.Predefined))
                }
            };
        }

        private async Task LockAsync(Collection collection, int userId, IDbTransaction transaction = null)
        {
            if (collection.LockedByUserId == null)
            {
                if (!await _lockArtifactsRepository.LockArtifactAsync(collection.Id, userId, transaction))
                {
                    throw ExceptionHelper.ArtifactNotLockedException(collection.Id, userId);
                }
            }
            else if (collection.LockedByUserId != userId)
            {
                throw CollectionsExceptionHelper.LockedByAnotherUserException(collection.Id, userId);
            }
        }

        private async Task<IReadOnlyList<ItemDetails>> GetAccessibleArtifactsAsync(
            List<int> artifactIds, int userId, IDbTransaction transaction = null)
        {
            var permissions = await _artifactPermissionsRepository.GetArtifactPermissions(
                artifactIds, userId, transaction: transaction);

            var artifacts = await _itemInfoRepository.GetItemsDetails(userId, artifactIds, transaction: transaction);

            return artifacts
                .Where(a => permissions.ContainsKey(a.HolderId) && permissions[a.HolderId].HasFlag(RolePermissions.Read))
                .ToList();
        }

        private static bool CanAddArtifactToCollection(ItemDetails artifact, Collection collection)
        {
            return (artifact.PrimitiveItemTypePredefined & (int)ItemTypePredefined.PrimitiveArtifactGroup) != 0 &&
                   (artifact.PrimitiveItemTypePredefined & (int)ItemTypePredefined.BaselineArtifactGroup) == 0 &&
                   (artifact.PrimitiveItemTypePredefined & (int)ItemTypePredefined.CollectionArtifactGroup) == 0 &&
                   artifact.PrimitiveItemTypePredefined != (int)ItemTypePredefined.Project &&
                   artifact.PrimitiveItemTypePredefined != (int)ItemTypePredefined.Baseline &&
                   artifact.VersionProjectId == collection.ProjectId &&
                   (artifact.EndRevision == int.MaxValue || artifact.EndRevision == 1);
        }
    }
}
