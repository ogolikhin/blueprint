using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Threading.Tasks;
using ArtifactStore.ArtifactList;
using ArtifactStore.ArtifactList.Helpers;
using ArtifactStore.ArtifactList.Models;
using ArtifactStore.Collections.Helpers;
using ArtifactStore.Collections.Models;
using SearchEngineLibrary.Model;
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
        private const string ChoiceValueFrame = "\"";
        private const string ChoiceValueSeparator = ", ";
        private const int ColumnLimit = 101;

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

        public async Task<CollectionData> GetArtifactsInCollectionAsync(
            int collectionId, int userId, Pagination pagination, ProfileColumns profileColumns)
        {
            var collection = await GetCollectionAsync(collectionId, userId);

            var searchArtifactsResult = await _searchEngineService.Search(
                collection.Id, null, ScopeType.Contents, true, userId);

            var collectionArtifacts = new CollectionArtifacts();

            var validProfileColumnsAndColumnValidation = await ValidateProfileColumns(userId, profileColumns, searchArtifactsResult);

            var validatedColumns = validProfileColumnsAndColumnValidation.Item1;
            collectionArtifacts.ColumnValidation = validProfileColumnsAndColumnValidation.Item2;

            var artifactIds = searchArtifactsResult.ArtifactIds.Skip((int)pagination.Offset).Take((int)pagination.Limit).ToList();

            var artifactsWithPropertyValues = await _collectionsRepository.GetArtifactsWithPropertyValuesAsync(
                userId, artifactIds, validatedColumns.Items);

            var populatedArtifacts = PopulateArtifactsProperties(artifactIds, artifactsWithPropertyValues, validatedColumns.Items);

            collectionArtifacts.ItemsCount = searchArtifactsResult.Total;
            collectionArtifacts.Items = populatedArtifacts.Item1;
            collectionArtifacts.ArtifactListSettings = populatedArtifacts.Item2;

            var collectionData = new CollectionData
            {
                ProfileColumns = validatedColumns,
                CollectionArtifacts = collectionArtifacts
            };

            return collectionData;
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

            Func<IDbTransaction, long, Task> action = async (transaction, transactionId) =>
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

            Func<IDbTransaction, long, Task> action = async (transaction, transactionId) =>
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
                    Total = removalParams.SelectionType == SelectionType.Selected ?
                            removalParams.ItemIds.Count() :
                            searchArtifactsResult.ArtifactIds.Except(removalParams.ItemIds).Count()
                };
            };

            await _sqlHelper.RunInTransactionAsync(ServiceConstants.RaptorMain, action);

            return result;
        }

        public async Task<GetColumnsDto> GetColumnsAsync(int collectionId, int userId, string search = null)
        {
            var collection = await GetCollectionAsync(collectionId, userId);
            var artifacts = await GetArtifactItemsDetailsAsync(collectionId, userId);

            if (artifacts.IsEmpty())
            {
                return new GetColumnsDto
                {
                    SelectedColumns = ProfileColumns.Default.Items,
                    UnselectedColumns = Enumerable.Empty<ProfileColumn>()
                };
            }

            var propertyTypeInfos = await GetPropertyTypeInfosAsync(artifacts, search);

            var profileColumns = (await _artifactListService.GetProfileSettingsAsync(collection.Id, userId))?.Columns ??
                                 ProfileColumns.Default;

            var selectedColumns = GetSelectedColumns(propertyTypeInfos, profileColumns, search).ToList();

            int? countOfNeededColumns = string.IsNullOrEmpty(search) ? ColumnLimit - selectedColumns.Count : (int?)null;

            return new GetColumnsDto
            {
                SelectedColumns = selectedColumns,
                UnselectedColumns = GetUnselectedColumns(propertyTypeInfos, profileColumns, countOfNeededColumns)
            };
        }

        public async Task<bool> SaveProfileColumnsAsync(int collectionId, ProfileColumns profileColumns, int userId)
        {
            if (userId < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(userId));
            }

            var collection = await GetCollectionAsync(collectionId, userId);
            var artifacts = await GetArtifactItemsDetailsAsync(collectionId, userId);
            var propertyTypeInfos = await GetPropertyTypeInfosAsync(artifacts);

            var propertyTypes = GetUnselectedColumns(propertyTypeInfos);
            var invalidColumns = profileColumns.GetInvalidAndValidColumns(propertyTypes).Item2.ToList();

            if (invalidColumns.Any())
            {
                throw ArtifactListExceptionHelper.InvalidColumnsException(invalidColumns);
            }

            var savingProfileColumnsTuple = profileColumns.ToValidColumns(propertyTypeInfos);

            await _artifactListService.SaveProfileColumnsAsync(collection.Id,
                savingProfileColumnsTuple.Item1, userId);

            return savingProfileColumnsTuple.Item2;
        }

        private async Task<IReadOnlyList<ItemDetails>> GetArtifactItemsDetailsAsync(int collectionId, int userId)
        {
            var artifactIds = await _collectionsRepository.GetContentArtifactIdsAsync(collectionId, userId);

            var artifactItems = await GetArtifactItemsDetailsAsync(artifactIds, userId);

            return artifactItems;
        }


        private async Task<IReadOnlyList<ItemDetails>> GetArtifactItemsDetailsAsync(IEnumerable<int> artifactIds, int userId)
        {
            var artifactsInCollection = await _itemInfoRepository.GetItemsDetails(userId, artifactIds);

            var artifactItems = artifactsInCollection
                .Where(artifact => artifact.EndRevision == int.MaxValue || artifact.EndRevision == 1)
                .ToList();

            return artifactItems;
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

        private static IEnumerable<ProfileColumn> GetSelectedColumns(
            IReadOnlyList<PropertyTypeInfo> propertyTypeInfos, ProfileColumns profileColumns, string search)
        {
            var validProfileColumns = profileColumns.ToValidColumns(propertyTypeInfos).Item1;

            return validProfileColumns.Items
                .Where(column => column.ExistsIn(propertyTypeInfos) && column.NameMatches(search))
                .Select(column => new ProfileColumn(
                    column.PropertyName, column.Predefined, column.PrimitiveType, column.PropertyTypeId));
        }

        private static IEnumerable<ProfileColumn> GetUnselectedColumns(
            IEnumerable<PropertyTypeInfo> propertyTypeInfos, ProfileColumns profileColumns = null, int? count = null)
        {
            return propertyTypeInfos
                .Select(info => info.IsCustom ?
                    CreateCustomPropertyColumn(info, profileColumns) :
                    CreateSystemPropertyColumn(info, profileColumns))
                .Where(column => column != null).TakeIfNotNull(count)
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

        private static Tuple<IEnumerable<ArtifactDto>, ArtifactListSettings> PopulateArtifactsProperties(IEnumerable<int> artifactIds, IReadOnlyList<ArtifactPropertyInfo> artifacts, IEnumerable<ProfileColumn> profileColumns)
        {
            var artifactDtos = new List<ArtifactDto>();

            foreach (var id in artifactIds)
            {
                var artifactProperties = artifacts.Where(x => x.ArtifactId == id).ToList();

                var propertyInfos = new List<PropertyValueInfo>();
                int? itemTypeId = null;
                int? predefinedType = null;
                int? itemTypeIconId = null;
                var filledMultiValueProperties = new List<Tuple<int, int?, PropertyTypePredefined, PropertyPrimitiveType>>();

                foreach (var artifactProperty in artifactProperties)
                {
                    var propertyInfo = new PropertyValueInfo();
                    var propertyTypePredefined = (PropertyTypePredefined)artifactProperty.PropertyTypePredefined;
                    var primitiveType = artifactProperty.PrimitiveType;

                    propertyInfo.PropertyTypeId = artifactProperty.PropertyTypeId;
                    propertyInfo.Predefined = artifactProperty.PropertyTypePredefined;
                    propertyInfo.IsRichText = artifactProperty.IsRichText ? true : (bool?)null; // Set null to ignore the property in final output.

                    if (propertyTypePredefined == PropertyTypePredefined.ID)
                    {
                        itemTypeId = artifactProperty.ItemTypeId;
                        predefinedType = artifactProperty.PrimitiveItemTypePredefined;
                        itemTypeIconId = artifactProperty.ItemTypeIconId;
                    }

                    bool systemColumn = propertyTypePredefined != PropertyTypePredefined.CustomGroup;
                    bool multiValue = primitiveType == PropertyPrimitiveType.Choice || primitiveType == PropertyPrimitiveType.User;

                    propertyInfo.Value = systemColumn
                            && !multiValue // Fill multi value properties below
                        ? artifactProperty.PredefinedPropertyValue
                        : primitiveType == PropertyPrimitiveType.Date
                        ? artifactProperty.DateTimeValue?.ToString("d", CultureInfo.InvariantCulture)
                        : primitiveType == PropertyPrimitiveType.Number
                        ? artifactProperty.DecimalValue?.ToString("0.#############################", CultureInfo.InvariantCulture)
                        : multiValue
                        ? null // Fill multi value properties below
                        : artifactProperty.IsRichText
                        ? artifactProperty.HtmlTextValue
                        : artifactProperty.FullTextValue;

                    propertyInfo.Value =
                        propertyTypePredefined == PropertyTypePredefined.ID
                        ? artifactProperty.Prefix + propertyInfo.Value
                        : propertyInfo.Value;

                    var multiValueTuple = new Tuple<int, int?, PropertyTypePredefined, PropertyPrimitiveType>(
                        artifactProperty.ArtifactId,
                        artifactProperty.PropertyTypeId,
                        (PropertyTypePredefined)artifactProperty.PropertyTypePredefined,
                        (PropertyPrimitiveType)artifactProperty.PrimitiveType);

                    if (!multiValue)
                    {
                        propertyInfos.Add(propertyInfo);
                    }
                    else if (!filledMultiValueProperties.Contains(multiValueTuple))
                    {
                        var values = artifactProperties
                            .Where(x =>
                                artifactProperty.PrimitiveType == x.PrimitiveType
                                && artifactProperty.ArtifactId == x.ArtifactId
                                && artifactProperty.PropertyTypeId == x.PropertyTypeId
                                && artifactProperty.PropertyTypePredefined == x.PropertyTypePredefined)
                            .Select(x => new
                            {
                                Value = systemColumn ? x.PredefinedPropertyValue : x.FullTextValue,
                                x.ValueId // ValueId is necessary for deduplication
                            })
                            .Distinct();

                        if (values.Count() > 1)
                        {
                            values = values
                                .Select(x => new
                                {
                                    Value = x.Value.Contains(ChoiceValueSeparator)
                                        ? ChoiceValueFrame + x.Value + ChoiceValueFrame
                                        : x.Value,
                                    x.ValueId
                                });

                        }
                        propertyInfo.Value = string.Join(ChoiceValueSeparator, values.Select(x => x.Value));

                        propertyInfos.Add(propertyInfo);
                        filledMultiValueProperties.Add(multiValueTuple);
                    }
                }

                artifactDtos.Add(new ArtifactDto
                {
                    ArtifactId = id,
                    ItemTypeId = itemTypeId,
                    PredefinedType = predefinedType,
                    ItemTypeIconId = itemTypeIconId,
                    PropertyInfos = propertyInfos.OrderBy(x => x.PropertyTypeId).ToList()
                });
            }

            return new Tuple<IEnumerable<ArtifactDto>, ArtifactListSettings>(artifactDtos, new ArtifactListSettings
            {
                Columns = profileColumns
            });
        }

        private async Task LockAsync(Collection collection, int userId, IDbTransaction transaction = null)
        {
            if (collection.LockedByUserId == null)
            {
                if (!await _lockArtifactsRepository.LockArtifactAsync(collection.Id, userId, transaction))
                {
                    throw CollectionsExceptionHelper.LockFailedException(collection.Id, userId);
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

        /// <summary>
        /// Valid profile columns according to database properties and consider count of invalid columns.
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="profileColumns"></param>
        /// <param name="searchArtifactsResult"></param>
        /// <returns> Valid profile columns and count of invalid columns with status. </returns>
        private async Task<Tuple<ProfileColumns, ColumnValidation>> ValidateProfileColumns(int userId,
            ProfileColumns profileColumns, SearchArtifactsResult searchArtifactsResult)
        {
            ProfileColumns validProfileColumns;
            var columnValidation = new ColumnValidation();

            if (profileColumns != null)
            {
                var artifactItems = await GetArtifactItemsDetailsAsync(searchArtifactsResult.ArtifactIds, userId);

                var propertyTypeInfos = await GetPropertyTypeInfosAsync(artifactItems);

                var propertyTypes = GetUnselectedColumns(propertyTypeInfos);

                var validatedProfileColumns = profileColumns.ToValidColumns(propertyTypeInfos).Item1;

                var invalidValidColumns = validatedProfileColumns.GetInvalidAndValidColumns(propertyTypes);

                var validColumns = invalidValidColumns.Item1;
                var invalidColumns = invalidValidColumns.Item2;

                var invalidColumnsCount = invalidColumns.Count();
                var validColumnsCount = validColumns.Count();

                if (validColumnsCount < 1 && invalidColumnsCount > 0)
                {
                    columnValidation.Number = invalidColumnsCount;
                    columnValidation.Status = ColumnValidationStatus.AllInvalid;
                    validProfileColumns = new ProfileColumns(ProfileColumns.Default.Items);
                }
                else if (invalidColumnsCount > 0)
                {
                    columnValidation.Number = invalidColumnsCount;
                    columnValidation.Status = ColumnValidationStatus.SomeValid;
                    validProfileColumns = new ProfileColumns(validColumns);
                }
                else
                {
                    validProfileColumns = new ProfileColumns(validatedProfileColumns.Items);
                }
            }
            else
            {
                validProfileColumns = new ProfileColumns(ProfileColumns.Default.Items);
            }

            return new Tuple<ProfileColumns, ColumnValidation>(validProfileColumns, columnValidation);
        }
    }
}
