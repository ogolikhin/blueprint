using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ArtifactStore.ArtifactList;
using ArtifactStore.ArtifactList.Models;
using ArtifactStore.Collections.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using SearchEngineLibrary.Model;
using SearchEngineLibrary.Service;
using ServiceLibrary.Exceptions;
using ServiceLibrary.Helpers;
using ServiceLibrary.Models;
using ServiceLibrary.Models.Enums;
using ServiceLibrary.Models.ProjectMeta;
using ServiceLibrary.Repositories;

namespace ArtifactStore.Collections
{
    [TestClass]
    public class CollectionsServiceTests
    {
        private CollectionsService _collectionService;

        private ISqlHelper _sqlHelperMock;
        private Mock<ICollectionsRepository> _collectionsRepository;
        private Mock<IArtifactRepository> _artifactRepository;
        private Mock<ILockArtifactsRepository> _lockArtifactsRepository;
        private Mock<IItemInfoRepository> _itemInfoRepository;
        private Mock<IArtifactPermissionsRepository> _artifactPermissionsRepository;
        private Mock<ISearchEngineService> _searchEngineService;
        private Mock<IArtifactListService> _artifactListService;

        private int _userId = 1;
        private ISet<int> _artifactIds;
        private int _collectionId;
        private ProfileColumns _profileColumnsSettings;
        private ProfileSettingsParams _profileSettings;
        private ArtifactBasicDetails _collectionDetails;
        private List<ItemDetails> _artifacts;
        private List<PropertyTypeInfo> _propertyTypeInfos;
        private List<PropertyTypeInfo> _propertyTypeInfosForGetCollection;
        private ProfileColumns _profileColumns;
        private ItemsRemovalParams _reviewItemsRemovalParams;
        private Dictionary<int, RolePermissions> _artifactPermissions;
        private Dictionary<int, RolePermissions> _collectionPermissions;
        private CollectionArtifacts _collectionArtifacts;
        private List<ArtifactPropertyInfo> _artifactPropertyInfos;
        private List<int> _artifactIdsForCollection;

        [TestInitialize]
        public void Initialize()
        {
            _sqlHelperMock = new SqlHelperMock();
            _collectionsRepository = new Mock<ICollectionsRepository>();
            _artifactRepository = new Mock<IArtifactRepository>();
            _lockArtifactsRepository = new Mock<ILockArtifactsRepository>();
            _itemInfoRepository = new Mock<IItemInfoRepository>();
            _artifactPermissionsRepository = new Mock<IArtifactPermissionsRepository>();
            _searchEngineService = new Mock<ISearchEngineService>();
            _artifactListService = new Mock<IArtifactListService>();
            _collectionService = new CollectionsService(
                _collectionsRepository.Object,
                _artifactRepository.Object,
                _lockArtifactsRepository.Object,
                _itemInfoRepository.Object,
                _artifactPermissionsRepository.Object,
                _sqlHelperMock,
                _searchEngineService.Object,
                _artifactListService.Object);


            _artifactIds = new HashSet<int> { 1, 2, 3 };
            _collectionId = 1;

            _profileSettings = new ProfileSettingsParams();

            _profileSettings.Columns = new ProfileColumns(
                new List<ProfileColumn>
                {
                    new ProfileColumn("Custom", PropertyTypePredefined.CustomGroup, PropertyPrimitiveType.Number, 2)
                });

            _collectionPermissions = new Dictionary<int, RolePermissions>
            {
                { _collectionId, RolePermissions.Read | RolePermissions.Edit }
            };

            _collectionDetails = new ArtifactBasicDetails
            {
                ArtifactId = _collectionId,
                ProjectId = 1,
                DraftDeleted = false,
                PrimitiveItemTypePredefined = (int)ItemTypePredefined.ArtifactCollection,
                LockedByUserId = _userId
            };

            _artifacts = new List<ItemDetails>
            {
                new ItemDetails
                {
                    Name = "Artifact1",
                    ItemTypeId = 2,
                    VersionProjectId = 1,
                    EndRevision = int.MaxValue,
                    PrimitiveItemTypePredefined = (int)ItemTypePredefined.Actor
                }
            };

            _artifactPermissions = new Dictionary<int, RolePermissions>();

            foreach (var itemDetails in _artifacts)
            {
                _artifactPermissions.Add(itemDetails.ItemTypeId, RolePermissions.Read);
            }

            _profileColumns = new ProfileColumns(
                new List<ProfileColumn>
                {
                    new ProfileColumn("Custom", PropertyTypePredefined.CustomGroup, PropertyPrimitiveType.Text, 2)
                });

            // _propertyTypeInfos = new List<PropertyTypeInfo>
            // {
            //    new PropertyTypeInfo
            //    {
            //        Id = 2,
            //        Predefined = PropertyTypePredefined.CustomGroup,
            //        Name = "Custom",
            //        PrimitiveType = PropertyPrimitiveType.Number
            //    }
            // };

            _artifactRepository
                .Setup(r => r.GetArtifactBasicDetails(_collectionId, _userId, null))
                .ReturnsAsync(_collectionDetails);

            _artifactPermissionsRepository
                .Setup(r => r.GetArtifactPermissions(_collectionId, _userId, It.IsAny<bool>(), It.IsAny<int>(),
                    It.IsAny<bool>(), null))
                .ReturnsAsync(_collectionPermissions);

            _artifactPermissionsRepository
                .Setup(r => r.GetArtifactPermissions(It.IsAny<IEnumerable<int>>(), _userId, It.IsAny<bool>(),
                    It.IsAny<int>(), It.IsAny<bool>(), null))
                .ReturnsAsync(_artifactPermissions);

            _collectionsRepository
                .Setup(r => r.GetContentArtifactIdsAsync(_collectionId, _userId, It.IsAny<bool>()))
                .ReturnsAsync(_artifactIds.ToList());

            _collectionsRepository
                .Setup(r => r.RemoveArtifactsFromCollectionAsync(_collectionId, It.IsAny<IEnumerable<int>>(), _userId,
                    null))
                .ReturnsAsync(_artifacts.Count);

            _itemInfoRepository
                .Setup(r => r.GetItemsDetails(It.IsAny<int>(), It.IsAny<List<int>>(), It.IsAny<bool>(), It.IsAny<int>(),
                    null))
                .ReturnsAsync(_artifacts);

            // InitializeProfileColumnsAndPropertyTypeInfos(_profileSettings, _propertyTypeInfos);

            _reviewItemsRemovalParams =
                new ItemsRemovalParams
                {
                    ItemIds = new List<int> { 1, 2, 3 },
                    SelectionType = SelectionType.Selected
                };

            _collectionArtifacts = new CollectionArtifacts
            {
                ItemsCount = 2,
                ArtifactListSettings = new ArtifactListSettings
                {
                    Columns = new List<ProfileColumn>
                    {
                        new ProfileColumn
                        {
                            Predefined = PropertyTypePredefined.CustomGroup,
                            PrimitiveType = PropertyPrimitiveType.Text,
                            PropertyName = "Test1",
                            PropertyTypeId = 80
                        },
                         new ProfileColumn
                         {
                            Predefined = PropertyTypePredefined.CustomGroup,
                            PrimitiveType = PropertyPrimitiveType.Text,
                            PropertyName = "Test2",
                            PropertyTypeId = 81
                         }
                    },
                    Filters = new List<ArtifactListFilter>()
                },
                Items = new List<ArtifactDto>
                {
                    new ArtifactDto
                    {
                        ArtifactId = 7545,
                        ItemTypeId = null,
                        PredefinedType = null,
                        PropertyInfos = new List<PropertyValueInfo>
                        {
                            new PropertyValueInfo
                            {
                                Predefined = (int)PropertyTypePredefined.CustomGroup,
                                PropertyTypeId = 80,
                                Value = "Value_Name"
                            },
                             new PropertyValueInfo
                             {
                                Predefined = (int)PropertyTypePredefined.CustomGroup,
                                PropertyTypeId = 81,
                                Value = "Value_Description"
                             }
                        }
                    },
                     new ArtifactDto
                     {
                        ArtifactId = 7551,
                        ItemTypeId = null,
                        PredefinedType = null,
                        PropertyInfos = new List<PropertyValueInfo>()
                     }
                }
            };

            _propertyTypeInfosForGetCollection = new List<PropertyTypeInfo>();

            foreach (var column in _collectionArtifacts.ArtifactListSettings.Columns)
            {
                _propertyTypeInfosForGetCollection.Add(new PropertyTypeInfo
                {
                    Id = column.PropertyTypeId,
                    Name = column.PropertyName,
                    Predefined = column.Predefined,
                    PrimitiveType = column.PrimitiveType
                });
            }

            _artifactIdsForCollection = new List<int>();
            var artifactDtos = _collectionArtifacts.Items.ToList();
            for (int i = 0; i < artifactDtos.Count; i++)
            {
                var artifact = artifactDtos[i];
                _artifactIdsForCollection.Add(artifact.ArtifactId);
            }

            _artifactPropertyInfos = new List<ArtifactPropertyInfo>();
            var firstArtifact = artifactDtos[0];
            var firstColumn = _collectionArtifacts.ArtifactListSettings.Columns.ToList()[0];
            foreach (var propertyInfo in firstArtifact.PropertyInfos)
            {
                _artifactPropertyInfos.Add(
                    new ArtifactPropertyInfo
                    {
                        ArtifactId = firstArtifact.ArtifactId,
                        FullTextValue = propertyInfo.Value,
                        IsRichText = false,
                        ItemTypeId = firstArtifact.ItemTypeId,
                        PredefinedType = firstArtifact.PredefinedType,
                        PrimitiveItemTypePredefined = (int)ItemTypePredefined.Actor,
                        PrimitiveType = PropertyPrimitiveType.Text,
                        PropertyName = firstColumn.PropertyName,
                        PropertyTypeId = propertyInfo.PropertyTypeId,
                        PropertyTypePredefined = propertyInfo.Predefined
                    });
            }

            _searchEngineService.Setup(s => s.Search(It.IsAny<int>(), It.IsAny<Pagination>(), It.IsAny<ScopeType>(), It.IsAny<bool>(), It.IsAny<int>(), null))
                .ReturnsAsync(new SearchArtifactsResult
                {
                    ArtifactIds = _artifactIdsForCollection,
                    Total = _artifactIdsForCollection.Count
                });

            _collectionsRepository
                .Setup(r => r.GetPropertyTypeInfosForItemTypesAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<string>()))
                .ReturnsAsync(_propertyTypeInfosForGetCollection);

            _collectionsRepository
                .Setup(r => r.GetArtifactsWithPropertyValuesAsync(It.IsAny<int>(), It.IsAny<IEnumerable<int>>(), It.IsAny<IEnumerable<ProfileColumn>>()))
                .ReturnsAsync(_artifactPropertyInfos);
        }

        #region AddArtifactsToCollectionAsync

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public async Task AddArtifactsToCollectionAsync_InvalidUserId_ThrowArgumentOutOfRangeException()
        {
            _userId = 0;

            await _collectionService.AddArtifactsToCollectionAsync(_collectionId, _artifactIds, _userId);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public async Task AddArtifactsToCollectionAsync_InvalidCollectionId_ThrowArgumentOutOfRangeException()
        {
            _collectionId = 0;

            await _collectionService.AddArtifactsToCollectionAsync(_collectionId, _artifactIds, _userId);
        }

        #endregion AddArtifactsToCollectionAsync

        #region GetArtifactsInCollectionAsync

        [TestMethod]
        public async Task GetArtifactsInCollectionAsync_EmptyAndNotEmptyArtifactsInCollection_Success()
        {
            // Setup:
            var profileColumns = new ProfileColumns(_collectionArtifacts.ArtifactListSettings.Columns);

            // Execute:
            var actual = await _collectionService.GetArtifactsInCollectionAsync(_collectionId, _userId, new Pagination { Limit = 10, Offset = 0 }, profileColumns);

            // Verify:
            Assert.IsNotNull(actual);
            Assert.AreEqual(_collectionArtifacts.ItemsCount, actual.CollectionArtifacts.ItemsCount);
            Assert.AreEqual(_collectionArtifacts.Items.Count(), actual.CollectionArtifacts.Items.Count());
            Assert.AreEqual(_collectionArtifacts.Items.ToJSON(), actual.CollectionArtifacts.Items.ToJSON());
        }

        [TestMethod]
        public async Task GetArtifactsInCollectionAsync_ColumnValidationIsAllValid_Success()
        {
            // Setup:
            var profileColumns = new ProfileColumns(_collectionArtifacts.ArtifactListSettings.Columns);

            // Execute:
            var actual = await _collectionService.GetArtifactsInCollectionAsync(_collectionId, _userId, new Pagination { Limit = 10, Offset = 0 }, profileColumns);

            // Verify:
            Assert.IsNotNull(actual);
            Assert.AreEqual(actual.CollectionArtifacts.ColumnValidation.Status, ColumnValidationStatus.AllValid);
            Assert.AreEqual(actual.CollectionArtifacts.ColumnValidation.Number, 0);
        }

        [TestMethod]
        public async Task GetArtifactsInCollectionAsync_ColumnValidationIsSomeInvalid_Success()
        {
            // Setup:
            var invalidProfileColumns = new List<ProfileColumn>
            {
                new ProfileColumn
                {
                    PropertyName = "InvalidProperty",
                    PropertyTypeId = 1,
                    Predefined = PropertyTypePredefined.CustomGroup,
                    PrimitiveType = PropertyPrimitiveType.Image
                }
            };

            var profileColumns = new ProfileColumns(invalidProfileColumns.Union(_collectionArtifacts.ArtifactListSettings.Columns));

            // Execute:
            var actual = await _collectionService.GetArtifactsInCollectionAsync(_collectionId, _userId, new Pagination { Limit = 10, Offset = 0 }, profileColumns);

            // Verify:
            Assert.IsNotNull(actual);
            Assert.AreEqual(actual.CollectionArtifacts.ColumnValidation.Status, ColumnValidationStatus.SomeValid);
            Assert.AreEqual(actual.CollectionArtifacts.ColumnValidation.Number, invalidProfileColumns.Count);
        }

        [TestMethod]
        public async Task GetArtifactsInCollectionAsync_ColumnValidationIsAllInvalid_Success()
        {
            // Setup:
            var invalidProfileColumns = new List<ProfileColumn>
            {
                new ProfileColumn
                {
                    PropertyName = "InvalidProperty",
                    PropertyTypeId = 1,
                    Predefined = PropertyTypePredefined.CustomGroup,
                    PrimitiveType = PropertyPrimitiveType.Image
                },
                new ProfileColumn
                {
                    PropertyName = "InvalidProperty2",
                    PropertyTypeId = 2,
                    Predefined = PropertyTypePredefined.CustomGroup,
                    PrimitiveType = PropertyPrimitiveType.Choice
                }
            };

            var profileColumns = new ProfileColumns(invalidProfileColumns);

            // Execute:
            var actual = await _collectionService.GetArtifactsInCollectionAsync(_collectionId, _userId, new Pagination { Limit = 10, Offset = 0 }, profileColumns);

            // Verify:
            Assert.IsNotNull(actual);
            Assert.AreEqual(actual.CollectionArtifacts.ColumnValidation.Status, ColumnValidationStatus.AllInvalid);
            Assert.AreEqual(actual.CollectionArtifacts.ColumnValidation.Number, invalidProfileColumns.Count);
        }

        [TestMethod]
        public async Task GetArtifactsInCollectionAsync_ColumnValidationIsAllValid_ProfileIsEmpty_Success()
        {
            // Setup:

            // Execute:
            var actual = await _collectionService.GetArtifactsInCollectionAsync(_collectionId, _userId, new Pagination { Limit = 10, Offset = 0 }, null);

            // Verify:
            Assert.IsNotNull(actual);
            Assert.AreEqual(actual.CollectionArtifacts.ColumnValidation.Status, ColumnValidationStatus.AllValid);
            Assert.AreEqual(actual.CollectionArtifacts.ColumnValidation.Number, 0);
        }

        [TestMethod]
        [ExpectedException(typeof(ResourceNotFoundException))]
        public async Task GetArtifactsInCollection_CollectionNotFound_ArtifactBasicDetailsIsNull_ThrowResourceNotFoundException()
        {
            ArtifactBasicDetails artifactBasicDetails = null;
            _artifactRepository.Setup(q => q.GetArtifactBasicDetails(_collectionId, _userId, null)).ReturnsAsync(artifactBasicDetails);

            await _collectionService.GetArtifactsInCollectionAsync(_collectionId, _userId, new Pagination(), new ProfileColumns(new List<ProfileColumn>()));
        }

        [TestMethod]
        [ExpectedException(typeof(ResourceNotFoundException))]
        public async Task GetArtifactsInCollection_CollectionNotFound_DraftDeletedInArtifactBasicDetailsIsTrue_ThrowResourceNotFoundException()
        {
            var artifactBasicDetails = new ArtifactBasicDetails { DraftDeleted = true };
            _artifactRepository.Setup(q => q.GetArtifactBasicDetails(_collectionId, _userId, null)).ReturnsAsync(artifactBasicDetails);

            await _collectionService.GetArtifactsInCollectionAsync(_collectionId, _userId, new Pagination(), new ProfileColumns(new List<ProfileColumn>()));
        }

        [TestMethod]
        [ExpectedException(typeof(ResourceNotFoundException))]
        public async Task GetArtifactsInCollection_CollectionNotFound_LatestDeletedInArtifactBasicDetailsIsTrue_ThrowResourceNotFoundException()
        {
            var artifactBasicDetails = new ArtifactBasicDetails { LatestDeleted = true };
            _artifactRepository.Setup(q => q.GetArtifactBasicDetails(_collectionId, _userId, null)).ReturnsAsync(artifactBasicDetails);

            await _collectionService.GetArtifactsInCollectionAsync(_collectionId, _userId, new Pagination(), new ProfileColumns(new List<ProfileColumn>()));
        }

        [TestMethod]
        [ExpectedException(typeof(BadRequestException))]
        public async Task GetArtifactsInCollection_FoundArtifactIsNotCollection_ThrowInvalidTypeException()
        {
            var artifactBasicDetails = new ArtifactBasicDetails { PrimitiveItemTypePredefined = (int)ItemTypePredefined.Actor, DraftDeleted = false, LatestDeleted = false };
            _artifactRepository.Setup(q => q.GetArtifactBasicDetails(_collectionId, _userId, null)).ReturnsAsync(artifactBasicDetails);

            await _collectionService.GetArtifactsInCollectionAsync(_collectionId, _userId, new Pagination(), new ProfileColumns(new List<ProfileColumn>()));
        }

        #endregion

        #region RemoveArtifactsFromCollectionAsync

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public async Task RemoveArtifactsFromCollectionAsync_InvalidUserId_ThrowArgumentOutOfRangeException()
        {
            _userId = 0;

            await _collectionService.RemoveArtifactsFromCollectionAsync(_collectionId, _reviewItemsRemovalParams, _userId);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public async Task RemoveArtifactsFromCollectionAsync_InvalidCollectionId_ThrowArgumentOutOfRangeException()
        {
            _collectionId = 0;

            await _collectionService.RemoveArtifactsFromCollectionAsync(_collectionId, _reviewItemsRemovalParams, _userId);
        }

        [TestMethod]
        public async Task RemoveArtifactsFromCollectionAsync_AllParametersAreValid_Success()
        {
            var result = await _collectionService.RemoveArtifactsFromCollectionAsync(_collectionId, _reviewItemsRemovalParams, _userId);

            Assert.IsNotNull(result);
            Assert.AreEqual(_artifacts.Count, result.RemovedCount);
        }

        [TestMethod]
        [ExpectedException(typeof(AuthorizationException))]
        public async Task RemoveArtifactsFromCollectionAsync_ValidateCollection_NoEditPermissionArtifact_NoEditPermissionException()
        {
            _collectionPermissions[_collectionId] = RolePermissions.Read;

            await _collectionService.RemoveArtifactsFromCollectionAsync(_collectionId, _reviewItemsRemovalParams, _userId);
        }

        [TestMethod]
        [ExpectedException(typeof(ConflictException))]
        public async Task RemoveArtifactsFromCollectionAsync_LockAsync_ArtifactLockedByAnotherUser_LockedByAnotherUserException()
        {
            _collectionDetails.LockedByUserId = 2;

            await _collectionService.RemoveArtifactsFromCollectionAsync(_collectionId, _reviewItemsRemovalParams, _userId);
        }

        [TestMethod]
        [ExpectedException(typeof(ConflictException))]
        public async Task RemoveArtifactsFromCollectionAsync_LockAsync_ArtifactNotLocked_ArtifactNotLockedException()
        {
            _collectionDetails.LockedByUserId = null;
            _lockArtifactsRepository
                .Setup(r => r.LockArtifactAsync(_collectionId, _userId, null))
                .ReturnsAsync(false);

            await _collectionService.RemoveArtifactsFromCollectionAsync(_collectionId, _reviewItemsRemovalParams, _userId);
        }

        [TestMethod]
        [ExpectedException(typeof(ResourceNotFoundException))]
        public async Task RemoveArtifactsFromCollectionAsync_GetCollectionBasicDetailsAsync_ArtifactIsNotFound_ResourceNotFoundException()
        {
            _collectionDetails.DraftDeleted = true;

            await _collectionService.RemoveArtifactsFromCollectionAsync(_collectionId, _reviewItemsRemovalParams, _userId);
        }

        [TestMethod]
        [ExpectedException(typeof(BadRequestException))]
        public async Task RemoveArtifactsFromCollectionAsync_GetCollectionBasicDetailsAsync_ArtifactIsNotArtifactCollection_InvalidTypeException()
        {
            _collectionDetails.PrimitiveItemTypePredefined = (int)ItemTypePredefined.Actor;

            await _collectionService.RemoveArtifactsFromCollectionAsync(_collectionId, _reviewItemsRemovalParams, _userId);
        }

        #endregion

        #region SaveProfileColumnsAsync

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public async Task SaveProfileColumnsAsync_InvalidUserId_ThrowArgumentOutOfRangeException()
        {
            _userId = 0;

            await _collectionService.SaveProfileColumnsAsync(_collectionId, _profileColumns, _userId);
        }

        [TestMethod]
        [ExpectedException(typeof(BadRequestException))]
        public async Task SaveProfileColumnsAsync_InvalidColumnsForSaving_InvalidName_ThrowInvalidColumnsException()
        {
            var propertyTypeInfos = new List<PropertyTypeInfo>();
            _profileColumns = new ProfileColumns(
                new List<ProfileColumn>
                {
                    new ProfileColumn("System", PropertyTypePredefined.ArtifactType, PropertyPrimitiveType.Text, 1),
                    new ProfileColumn("Custom", PropertyTypePredefined.CustomGroup, PropertyPrimitiveType.Text, 2)
                });

            foreach (var column in _profileColumns.Items)
            {
                propertyTypeInfos.Add(new PropertyTypeInfo()
                {
                    Id = column.PropertyTypeId,
                    Name = column.PropertyName + DateTime.Now.ToLongDateString(),
                    Predefined = column.Predefined,
                    PrimitiveType = column.PrimitiveType
                });
            }

            _collectionsRepository
                .Setup(q => q.GetPropertyTypeInfosForItemTypesAsync(
                    It.IsAny<IEnumerable<int>>(), null))
                .ReturnsAsync(propertyTypeInfos);

            await _collectionService.SaveProfileColumnsAsync(_collectionId, _profileColumns, _userId);
        }

        [TestMethod]
        [ExpectedException(typeof(BadRequestException))]
        public async Task SaveProfileColumnsAsync_InvalidColumnsForSaving_InvalidPredefined_ThrowInvalidColumnsException()
        {
            var propertyTypeInfos = new List<PropertyTypeInfo>();
            _profileColumns = new ProfileColumns(
                new List<ProfileColumn>
                {
                    new ProfileColumn("System", PropertyTypePredefined.ArtifactType, PropertyPrimitiveType.Text, 1),
                    new ProfileColumn("Custom", PropertyTypePredefined.CustomGroup, PropertyPrimitiveType.Text, 2)
                });

            foreach (var column in _profileColumns.Items)
            {
                propertyTypeInfos.Add(new PropertyTypeInfo()
                {
                    Id = column.PropertyTypeId,
                    Name = column.PropertyName,
                    Predefined = PropertyTypePredefined.BackgroundColor,
                    PrimitiveType = column.PrimitiveType
                });
            }

            _collectionsRepository
                .Setup(q => q.GetPropertyTypeInfosForItemTypesAsync(
                    It.IsAny<IEnumerable<int>>(), null))
                .ReturnsAsync(propertyTypeInfos);

            await _collectionService.SaveProfileColumnsAsync(_collectionId, _profileColumns, _userId);
        }

        [TestMethod]
        [ExpectedException(typeof(BadRequestException))]
        public async Task SaveProfileColumnsAsync_InvalidColumnsForSaving_InvalidPrimitiveType_ThrowInvalidColumnsException()
        {
            var propertyTypeInfos = new List<PropertyTypeInfo>();
            _profileColumns = new ProfileColumns(
                new List<ProfileColumn>
                {
                    new ProfileColumn("System", PropertyTypePredefined.ArtifactType, PropertyPrimitiveType.Text, 1),
                    new ProfileColumn("Custom", PropertyTypePredefined.CustomGroup, PropertyPrimitiveType.Text, 2)
                });

            foreach (var column in _profileColumns.Items)
            {
                propertyTypeInfos.Add(new PropertyTypeInfo()
                {
                    Id = column.PropertyTypeId,
                    Name = column.PropertyName,
                    Predefined = column.Predefined,
                    PrimitiveType = PropertyPrimitiveType.Date
                });
            }

            _collectionsRepository
                .Setup(q => q.GetPropertyTypeInfosForItemTypesAsync(
                    It.IsAny<IEnumerable<int>>(), null))
                .ReturnsAsync(propertyTypeInfos);

            await _collectionService.SaveProfileColumnsAsync(_collectionId, _profileColumns, _userId);
        }

        [TestMethod]
        [ExpectedException(typeof(BadRequestException))]
        public async Task SaveProfileColumnsAsync_InvalidColumnsForSaving_InvalidPropertyTypeId_ThrowInvalidColumnsException()
        {
            var propertyTypeInfos = new List<PropertyTypeInfo>();
            _profileColumns = new ProfileColumns(
                new List<ProfileColumn>
                {
                    new ProfileColumn("System", PropertyTypePredefined.ArtifactType, PropertyPrimitiveType.Text, 1),
                    new ProfileColumn("Custom", PropertyTypePredefined.CustomGroup, PropertyPrimitiveType.Text, 2)
                });

            foreach (var column in _profileColumns.Items)
            {
                propertyTypeInfos.Add(new PropertyTypeInfo()
                {
                    Id = column.PropertyTypeId.GetValueOrDefault() + 1,
                    Name = column.PropertyName,
                    Predefined = column.Predefined,
                    PrimitiveType = column.PrimitiveType
                });
            }

            _collectionsRepository
                .Setup(q => q.GetPropertyTypeInfosForItemTypesAsync(
                    It.IsAny<IEnumerable<int>>(), null))
                .ReturnsAsync(propertyTypeInfos);

            await _collectionService.SaveProfileColumnsAsync(_collectionId, _profileColumns, _userId);
        }

        [TestMethod]
        public async Task SaveProfileColumnsAsync_AllDataValid_SuccessResult()
        {
            var propertyTypeInfos = new List<PropertyTypeInfo>();
            _profileColumns = new ProfileColumns(
                new List<ProfileColumn>
                {
                    new ProfileColumn("System", PropertyTypePredefined.ArtifactType, PropertyPrimitiveType.Text),
                    new ProfileColumn("Custom", PropertyTypePredefined.CustomGroup, PropertyPrimitiveType.Text, 2)
                });

            foreach (var column in _profileColumns.Items)
            {
                propertyTypeInfos.Add(new PropertyTypeInfo()
                {
                    Id = column.PropertyTypeId,
                    Name = column.PropertyName,
                    Predefined = column.Predefined,
                    PrimitiveType = column.PrimitiveType
                });
            }

            _collectionsRepository
                .Setup(q => q.GetPropertyTypeInfosForItemTypesAsync(
                    It.IsAny<IEnumerable<int>>(), null))
                .ReturnsAsync(propertyTypeInfos);

            await _collectionService.SaveProfileColumnsAsync(_collectionId, _profileColumns, _userId);
        }

        [TestMethod]
        public async Task SaveProfileColumnsAsync_ChangedCustomPropertyNames_SuccessResult_ReturnedTrue()
        {
            var propertyTypeInfos = new List<PropertyTypeInfo>();

            foreach (var column in _profileColumns.Items)
            {
                propertyTypeInfos.Add(new PropertyTypeInfo()
                {
                    Id = column.PropertyTypeId,
                    Name = column.PropertyName + DateTime.Now.ToLongTimeString(),
                    Predefined = column.Predefined,
                    PrimitiveType = column.PrimitiveType
                });
            }

            _collectionsRepository
                .Setup(q => q.GetPropertyTypeInfosForItemTypesAsync(
                    It.IsAny<IEnumerable<int>>(), null))
                .ReturnsAsync(propertyTypeInfos);

            var result = await _collectionService.SaveProfileColumnsAsync(_collectionId, _profileColumns, _userId);

            Assert.IsTrue(result);
        }

        [TestMethod]
        public async Task SaveProfileColumnsAsync_ChangedCustomPropertyPrimitiveTypes_SuccessResult_ReturnedTrue()
        {
            var propertyTypeInfos = new List<PropertyTypeInfo>();

            foreach (var column in _profileColumns.Items)
            {
                propertyTypeInfos.Add(new PropertyTypeInfo()
                {
                    Id = column.PropertyTypeId,
                    Name = column.PropertyName,
                    Predefined = column.Predefined,
                    PrimitiveType = PropertyPrimitiveType.User
                });
            }

            _collectionsRepository
                .Setup(q => q.GetPropertyTypeInfosForItemTypesAsync(
                    It.IsAny<IEnumerable<int>>(), null))
                .ReturnsAsync(propertyTypeInfos);

            var result = await _collectionService.SaveProfileColumnsAsync(_collectionId, _profileColumns, _userId);

            Assert.IsTrue(result);
        }

        [TestMethod]
        public async Task SaveProfileColumnsAsync_NotChangedCustomProperties_SuccessResult_ReturnedFalse()
        {
            var propertyTypeInfos = new List<PropertyTypeInfo>();

            foreach (var column in _profileColumns.Items)
            {
                propertyTypeInfos.Add(new PropertyTypeInfo()
                {
                    Id = column.PropertyTypeId,
                    Name = column.PropertyName,
                    Predefined = column.Predefined,
                    PrimitiveType = column.PrimitiveType
                });
            }

            _collectionsRepository
                .Setup(q => q.GetPropertyTypeInfosForItemTypesAsync(
                    It.IsAny<IEnumerable<int>>(), null))
                .ReturnsAsync(propertyTypeInfos);

            var result = await _collectionService.SaveProfileColumnsAsync(_collectionId, _profileColumns, _userId);

            Assert.IsFalse(result);
        }

        #endregion SaveColumnSettingsAsync

        #region GetColumnsAsync

        [TestMethod]
        [ExpectedException(typeof(AuthorizationException))]
        public async Task GetColumnsAsync_NoPermissions_ThrowsAuthorizationException()
        {
            _collectionPermissions[_collectionId] = RolePermissions.None;

            var result = await _collectionService.GetColumnsAsync(_collectionId, _userId);
        }

        [TestMethod]
        public async Task GetColumnsAsync_ProfileColumnsSettingsAreEmptySelectedColumnsEmpty_Success()
        {
            _profileColumnsSettings = new ProfileColumns(new List<ProfileColumn>());

            _propertyTypeInfos = new List<PropertyTypeInfo>
             {
                new PropertyTypeInfo
                {
                    Id = 2,
                    Predefined = PropertyTypePredefined.CustomGroup,
                    Name = "Custom",
                    PrimitiveType = PropertyPrimitiveType.Number
                }
             };

            InitializeProfileColumnsAndPropertyTypeInfos(_profileSettings, _propertyTypeInfos);

            var result = await _collectionService.GetColumnsAsync(_collectionId, _userId);

            Assert.IsNotNull(result);
            // Assert.IsFalse(result.SelectedColumns.Any()); It will be fixed in STOR-9775
        }

        [TestMethod]
        public async Task GetColumnsAsync_PropertyTypeInfosAreEmptySelectedColumnsEmpty_Success()
        {
            _propertyTypeInfos = new List<PropertyTypeInfo>();

            InitializeProfileColumnsAndPropertyTypeInfos(_profileSettings, _propertyTypeInfos);

            var result = await _collectionService.GetColumnsAsync(_collectionId, _userId);

            Assert.IsNotNull(result);
            Assert.IsFalse(result.SelectedColumns.Any());
        }

        [TestMethod]
        public async Task GetColumnsAsync_PropertyTypeInfosExistInProfileColumnsSettingsSelectedColumnsNotEmpty_Success()
        {
            InitializeProfileColumnsAndPropertyTypeInfos(_profileSettings, _propertyTypeInfos);

            var result = await _collectionService.GetColumnsAsync(_collectionId, _userId);

            Assert.IsNotNull(result);
            Assert.IsTrue(result.SelectedColumns.Any());
        }

        [TestMethod]
        public async Task GetColumnsAsync_SearchByNameMatchesSelectedColumnsNotEmpty_Success()
        {
            const string searchWildCard = "Cust";

            var result = await _collectionService.GetColumnsAsync(_collectionId, _userId, searchWildCard);

            Assert.IsNotNull(result);
            Assert.IsTrue(result.SelectedColumns.Any());
        }

        [TestMethod]
        public async Task GetColumnsAsync_SearchByNameDoesNotMatchSelectedColumnsEmpty_Success()
        {
            const string searchWildCard = "Test";

            var result = await _collectionService.GetColumnsAsync(_collectionId, _userId, searchWildCard);

            Assert.IsNotNull(result);
            Assert.IsFalse(result.SelectedColumns.Any());
        }

        [TestMethod]
        public async Task GetColumnsAsync_PropertyTypeInfosAreCustomAndExistInProfileColumnsSettingsUnSelectedColumnsEmpty_Success()
        {

            InitializeProfileColumnsAndPropertyTypeInfos(_profileSettings, _propertyTypeInfos);

            var result = await _collectionService.GetColumnsAsync(_collectionId, _userId);

            Assert.IsNotNull(result);
            Assert.IsFalse(result.UnselectedColumns.Any());
        }

        [TestMethod]
        public async Task GetColumnsAsync_ProfileColumnsSettingsAreEmptyUnSelectedColumnsNotEmpty_Success()
        {
            _profileColumnsSettings = new ProfileColumns(new List<ProfileColumn>());

            InitializeProfileColumnsAndPropertyTypeInfos(_profileSettings, _propertyTypeInfos);

            var result = await _collectionService.GetColumnsAsync(_collectionId, _userId);

            Assert.IsNotNull(result);
            // Assert.IsTrue(result.UnselectedColumns.Any()); It will be fixed in STOR-9775
        }

        [TestMethod]
        public async Task GetColumnsAsync_PropertyTypeInfosAreEmptyUnSelectedColumnsEmpty_Success()
        {
            _propertyTypeInfos = new List<PropertyTypeInfo>();

            InitializeProfileColumnsAndPropertyTypeInfos(_profileSettings, _propertyTypeInfos);

            var result = await _collectionService.GetColumnsAsync(_collectionId, _userId);

            Assert.IsNotNull(result);
            Assert.IsFalse(result.UnselectedColumns.Any());
        }

        [TestMethod]
        public async Task GetColumnsAsync_PropertyTypeInfoPredefinedIsNotSystemUnSelectedColumnsEmpty_Success()
        {
            _propertyTypeInfos = new List<PropertyTypeInfo>
            {
                new PropertyTypeInfo
                {
                    Id = 2,
                    Predefined = PropertyTypePredefined.ColumnLabel,
                    Name = "NonSystem",
                    PrimitiveType = PropertyPrimitiveType.Number
                }
            };

            InitializeProfileColumnsAndPropertyTypeInfos(_profileSettings, _propertyTypeInfos);

            var result = await _collectionService.GetColumnsAsync(_collectionId, _userId);

            Assert.IsNotNull(result);
            Assert.IsFalse(result.UnselectedColumns.Any());
        }

        [TestMethod]
        public async Task GetColumnsAsync_ProfileColumnsSettingHasSamePredefeinedAsPropertyTypeInfoUnSelectedColumnsEmpty_Success()
        {
            _profileColumnsSettings = new ProfileColumns(
                new List<ProfileColumn>
                {
                    new ProfileColumn("System", PropertyTypePredefined.ID, PropertyPrimitiveType.Number, 2)
                });

            _propertyTypeInfos = new List<PropertyTypeInfo>
            {
                new PropertyTypeInfo
                {
                    Id = 2,
                    Predefined = PropertyTypePredefined.ID,
                    Name = "System",
                    PrimitiveType = PropertyPrimitiveType.Number
                }
            };

            InitializeProfileColumnsAndPropertyTypeInfos(_profileSettings, _propertyTypeInfos);

            var result = await _collectionService.GetColumnsAsync(_collectionId, _userId);

            Assert.IsNotNull(result);
            // Assert.IsFalse(result.UnselectedColumns.Any()); It will be fixed in STOR-9775
        }

        [TestMethod]
        public async Task GetColumnsAsync_EmptyCollection_NoSettings_ReturnsDefaultSelectedColumnsAndEmptyUnselectedColumns()
        {
            _artifacts.Clear();
            InitializeProfileColumnsAndPropertyTypeInfos(null, new List<PropertyTypeInfo>());

            var result = await _collectionService.GetColumnsAsync(_collectionId, _userId);

            CollectionAssert.AreEquivalent(ProfileColumns.Default.Items.ToList(), result.SelectedColumns.ToList());
            CollectionAssert.AreEquivalent(Enumerable.Empty<ProfileColumn>().ToList(), result.UnselectedColumns.ToList());
        }

        [TestMethod]
        public async Task GetColumnsAsync_EmptyCollection_WithSettings_ReturnsDefaultSelectedColumnsAndEmptyUnselectedColumns()
        {
            _artifacts.Clear();

            var result = await _collectionService.GetColumnsAsync(_collectionId, _userId);

            CollectionAssert.AreEquivalent(ProfileColumns.Default.Items.ToList(), result.SelectedColumns.ToList());
            CollectionAssert.AreEquivalent(Enumerable.Empty<ProfileColumn>().ToList(), result.UnselectedColumns.ToList());
        }

        #endregion GetColumnsAsync

        #region Private methods

        private void InitializeProfileColumnsAndPropertyTypeInfos(ProfileSettingsParams profileSettings, IReadOnlyList<PropertyTypeInfo> propertyTypeInfos)
        {
            _artifactListService
                .Setup(s => s.GetProfileSettingsAsync(It.IsAny<int>(), It.IsAny<int>()))
                .ReturnsAsync(profileSettings);

            _collectionsRepository
              .Setup(r => r.GetPropertyTypeInfosForItemTypesAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<string>()))
              .ReturnsAsync((propertyTypeInfos));
        }

        #endregion
    }
}
