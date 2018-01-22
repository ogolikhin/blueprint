﻿using System;
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

        private async Task<ArtifactBasicDetails> GetCollectionBasicDetailsAsync(
            int collectionId, int userId, IDbTransaction transaction = null)
        {
            var collection = await _artifactRepository.GetArtifactBasicDetails(collectionId, userId, transaction);

            if (collection == null || collection.DraftDeleted || collection.LatestDeleted)
            {
                throw CollectionsExceptionHelper.NotFoundException(collectionId);
            }

            if (collection.PrimitiveItemTypePredefined != (int)ItemTypePredefined.ArtifactCollection)
            {
                throw CollectionsExceptionHelper.InvalidTypeException(collectionId);
            }

            if (!await _artifactPermissionsRepository.HasReadPermissions(collectionId, userId))
            {
                throw CollectionsExceptionHelper.NoAccessException(collectionId);
            }

            return collection;
        }

        public async Task<CollectionArtifacts> GetArtifactsInCollectionAsync(
            int collectionId, Pagination pagination, int userId)
        {
            var collection = await GetCollectionBasicDetailsAsync(collectionId, userId);

            var searchArtifactsResult = await _searchEngineService.Search(
                collection.ArtifactId, pagination, ScopeType.Contents, true, userId);

            var artifacts = await _collectionsRepository.GetArtifactsWithPropertyValuesAsync(
                userId, searchArtifactsResult.ArtifactIds);

            var populatedArtifacts = PopulateArtifactsProperties(artifacts);
            populatedArtifacts.ItemsCount = searchArtifactsResult.Total;

            return populatedArtifacts;
        }

        public async Task<AddArtifactsResult> AddArtifactsToCollectionAsync(
            int collectionId, ISet<int> artifactIds, int userId)
        {
            if (collectionId < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(collectionId));
            }

            if (userId < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(userId));
            }

            AddArtifactsResult result = null;

            Func<IDbTransaction, Task> action = async transaction =>
            {
                var collection = await GetCollectionBasicDetailsAsync(collectionId, userId, transaction);

                if (!await _artifactPermissionsRepository.HasEditPermissions(
                    collection.ArtifactId, userId, transaction: transaction))
                {
                    throw CollectionsExceptionHelper.NoEditPermissionException(collection.ArtifactId);
                }

                await LockAsync(collection, userId, transaction);

                await _collectionsRepository.RemoveDeletedArtifactsFromCollectionAsync(
                    collection.ArtifactId, userId, transaction);

                var artifactsWithReadPermissions = await GetAccessibleArtifactIdsAsync(
                    artifactIds, collection, userId, transaction);

                var addedCount = await _collectionsRepository.AddArtifactsToCollectionAsync(
                    collection.ArtifactId, artifactsWithReadPermissions, userId, transaction);

                result = new AddArtifactsResult
                {
                    AddedCount = addedCount,
                    Total = artifactIds.Count
                };
            };

            await _sqlHelper.RunInTransactionAsync(ServiceConstants.RaptorMain, action);

            return result;
        }

        public async Task<GetColumnsDto> GetColumnsAsync(int collectionId, int userId, string search = null)
        {
            var collection = await GetCollectionBasicDetailsAsync(collectionId, userId);
            var columnSettings = await _artifactListService.GetColumnSettingsAsync(collection.ArtifactId, userId);

            return new GetColumnsDto
            {
                SelectedColumns = GetSelectedColumns(columnSettings, search),
                OtherColumns = await GetOtherColumnsAsync(collection.ArtifactId, userId, columnSettings, search)
            };
        }

        public async Task SaveColumnSettingsAsync(int collectionId, ProfileColumnsSettings columnSettings, int userId)
        {
            var collection = await GetCollectionBasicDetailsAsync(collectionId, userId);

            await _artifactListService.SaveColumnsSettingsAsync(collection.ArtifactId, columnSettings, userId);
        }

        private static IEnumerable<ArtifactListColumn> GetSelectedColumns(
            ProfileColumnsSettings columnSettings, string search)
        {
            if (columnSettings == null || columnSettings.Items.IsEmpty())
            {
                return null;
            }

            return columnSettings.Items
                .Where(column => string.IsNullOrEmpty(search) ||
                                 column.PropertyName.IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0)
                .Select(column => new ArtifactListColumn
                {
                    PropertyName = column.PropertyName,
                    PropertyTypeId = column.PropertyTypeId,
                    Predefined = column.Predefined,
                    PrimitiveType = default(int) // TODO: Either remove or add this property
                })
                .ToList();
        }

        private async Task<IEnumerable<ArtifactListColumn>> GetOtherColumnsAsync(
            int collectionId, int userId, ProfileColumnsSettings columnSettings, string search)
        {
            var columns = new List<ArtifactListColumn>();

            var artifactIds = await _collectionsRepository.GetContentArtifactIdsAsync(collectionId, userId);
            var artifacts = await _itemInfoRepository.GetItemsDetails(userId, artifactIds);
            var itemTypeIds = artifacts.Select(a => a.ItemTypeId).Distinct();
            var propertyTypeInfos = await _collectionsRepository.GetPropertyTypeInfosForItemTypesAsync(
                itemTypeIds, search);

            PopulateSystemPropertyColumns(columns, columnSettings, propertyTypeInfos);
            PopulateCustomPropertyColumns(columns, columnSettings, propertyTypeInfos);

            return columns;
        }

        private static void PopulateSystemPropertyColumns(
            ICollection<ArtifactListColumn> columns, ProfileColumnsSettings columnSettings,
            IEnumerable<PropertyTypeInfo> propertyTypeInfos)
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

            var systemPropertyTypeInfos = propertyTypeInfos.Where(i => systemPredefineds.Contains(i.Predefined));

            foreach (var propertyTypeInfo in systemPropertyTypeInfos)
            {
                if (columnSettings != null && columnSettings.Contains(propertyTypeInfo.Predefined))
                {
                    continue;
                }

                columns.Add(new ArtifactListColumn
                {
                    PropertyName = propertyTypeInfo.Name,
                    PropertyTypeId = propertyTypeInfo.Id,
                    Predefined = (int)propertyTypeInfo.Predefined,
                    PrimitiveType = (int)propertyTypeInfo.PrimitiveType
                });
            }
        }

        private static void PopulateCustomPropertyColumns(
            ICollection<ArtifactListColumn> columns, ProfileColumnsSettings columnSettings,
            IEnumerable<PropertyTypeInfo> propertyTypeInfos)
        {
            var customPropertyTypeInfos = propertyTypeInfos
                .Where(i => i.Predefined == PropertyTypePredefined.CustomGroup);

            foreach (var propertyTypeInfo in customPropertyTypeInfos)
            {
                if (columnSettings != null && columnSettings.Contains(propertyTypeInfo.Id))
                {
                    continue;
                }

                columns.Add(new ArtifactListColumn
                {
                    PropertyName = propertyTypeInfo.Name,
                    PropertyTypeId = propertyTypeInfo.Id,
                    Predefined = (int)propertyTypeInfo.Predefined,
                    PrimitiveType = (int)propertyTypeInfo.PrimitiveType
                });
            }
        }

        private static CollectionArtifacts PopulateArtifactsProperties(IReadOnlyList<CollectionArtifact> artifacts)
        {
            var artifactIdsResult = artifacts.Select(x => x.ArtifactId).Distinct().ToList();

            var artifactDtos = new List<ArtifactDto>();
            var settingsColumns = new List<ArtifactListColumn>();
            var areColumnsPopulated = false;

            foreach (var id in artifactIdsResult)
            {
                var artifactProperties = artifacts.Where(x => x.ArtifactId == id).ToList();

                var propertyInfos = new List<PropertyValueInfo>();
                int? itemTypeId = null;

                foreach (var artifactProperty in artifactProperties)
                {
                    ArtifactListColumn artifactListColumn = null;
                    var propertyInfo = new PropertyValueInfo();

                    if (!areColumnsPopulated)
                    {
                        artifactListColumn = new ArtifactListColumn
                        {
                            PropertyName = artifactProperty.PropertyName
                        };
                    }

                    if (artifactProperty.PropertyTypeId == null)
                    {
                        int fakeId;

                        if (!ServiceConstants.PropertyTypePredefineds.TryGetValue(
                            (PropertyTypePredefined)artifactProperty.PropertyTypePredefined, out fakeId))
                        {
                            continue;
                        }

                        if (!areColumnsPopulated)
                        {
                            artifactListColumn.PropertyTypeId = fakeId;
                        }

                        propertyInfo.PropertyTypeId = fakeId;

                        if (fakeId == ServiceConstants.IdPropertyFakeId)
                        {
                            propertyInfo.Value = I18NHelper.FormatInvariant("{0}{1}", artifactProperty.Prefix,
                                artifactProperty.ArtifactId);

                            itemTypeId = artifactProperty.ItemTypeId;
                        }
                        else
                        {
                            propertyInfo.Value = artifactProperty.PropertyValue;
                        }
                    }
                    else
                    {
                        if (!areColumnsPopulated)
                        {
                            artifactListColumn.PropertyTypeId = artifactProperty.PropertyTypeId;
                        }

                        propertyInfo.PropertyTypeId = artifactProperty.PropertyTypeId;
                        propertyInfo.Value = artifactProperty.PropertyValue;
                    }

                    if (!areColumnsPopulated)
                    {
                        settingsColumns.Add(artifactListColumn);
                    }

                    propertyInfos.Add(propertyInfo);
                }

                areColumnsPopulated = true;

                artifactDtos.Add(new ArtifactDto
                {
                    ArtifactId = id,
                    ItemTypeId = itemTypeId,
                    PropertyInfos = propertyInfos
                });
            }

            return new CollectionArtifacts
            {
                Items = artifactDtos,
                ArtifactListSettings = new ArtifactListSettings
                {
                    Columns = settingsColumns
                }
            };
        }

        private async Task LockAsync(ArtifactBasicDetails collection, int userId, IDbTransaction transaction = null)
        {
            if (collection.LockedByUserId == null)
            {
                if (!await _lockArtifactsRepository.LockArtifactAsync(collection.ArtifactId, userId, transaction))
                {
                    throw ExceptionHelper.ArtifactNotLockedException(collection.ArtifactId, userId);
                }
            }
            else if (collection.LockedByUserId != userId)
            {
                throw CollectionsExceptionHelper.LockedByAnotherUserException(collection.ArtifactId, userId);
            }
        }

        private async Task<IReadOnlyList<int>> GetAccessibleArtifactIdsAsync(
            IEnumerable<int> artifactIds, ArtifactBasicDetails collection, int userId,
            IDbTransaction transaction = null)
        {
            var artifacts = await _itemInfoRepository.GetItemsDetails(userId, artifactIds, transaction: transaction);

            var validArtifacts = artifacts.Where(i => CanAddArtifactToCollection(i, collection)).ToList();

            var permissions = await _artifactPermissionsRepository.GetArtifactPermissions(
                validArtifacts.Select(i => i.HolderId), userId, transaction: transaction);

            return permissions
                .Where(p => p.Value.HasFlag(RolePermissions.Read))
                .Select(p => p.Key)
                .ToList();
        }

        private static bool CanAddArtifactToCollection(ItemDetails artifact, ArtifactBasicDetails collection)
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