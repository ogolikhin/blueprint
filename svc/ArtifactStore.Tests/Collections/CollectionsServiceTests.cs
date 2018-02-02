using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ArtifactStore.ArtifactList;
using ArtifactStore.ArtifactList.Models;
using ArtifactStore.Collections;
using ArtifactStore.Collections.Models;
using ArtifactStore.Models.Review;
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
        private ArtifactBasicDetails _collectionDetails;
        private List<ItemDetails> _artifacts;
        private List<PropertyTypeInfo> _propertyTypeInfos;
        private ProfileColumns _profileColumns;
        private ReviewItemsRemovalParams _reviewItemsRemovalParams;
        private SearchArtifactsResult _searchArtifactsResult;
        private Dictionary<int, RolePermissions> _artifactPermissions;

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
            _searchArtifactsResult = new SearchArtifactsResult { ArtifactIds = _artifactIds, Total = _artifactIds.Count };

            _profileColumnsSettings = new ProfileColumns(new List<ProfileColumn>
                {
                    new ProfileColumn("Custom", PropertyTypePredefined.CustomGroup, PropertyPrimitiveType.Number, 2)

                });

            _collectionDetails = new ArtifactBasicDetails
            {
                ArtifactId = 1,
                DraftDeleted = false,
                PrimitiveItemTypePredefined = (int)ItemTypePredefined.ArtifactCollection,
                LockedByUserId = _userId
            };

            _artifacts = new List<ItemDetails>
            {
                new ItemDetails()
                {
                    Name = "Artifact1",
                    ItemTypeId = 2,
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

            _propertyTypeInfos = new List<PropertyTypeInfo>()
            {
                new PropertyTypeInfo
                {
                    Id = 2,
                    Predefined = PropertyTypePredefined.CustomGroup,
                    Name = "Custom",
                    PrimitiveType = PropertyPrimitiveType.Number
                }
            };

            _artifactRepository.Setup(repo => repo.GetArtifactBasicDetails(It.IsAny<int>(), It.IsAny<int>(), null))
                .ReturnsAsync(_collectionDetails);

            _artifactPermissionsRepository.Setup(repo => repo.HasReadPermissions(It.IsAny<int>(), _userId, It.IsAny<bool>(), It.IsAny<int>(), It.IsAny<bool>()))
                .ReturnsAsync(true);

            _artifactPermissionsRepository.Setup(repo => repo.HasEditPermissions(It.IsAny<int>(), _userId, It.IsAny<bool>(), It.IsAny<int>(), It.IsAny<bool>(), null))
                .ReturnsAsync(true);

            _artifactPermissionsRepository.Setup(repo => repo.GetArtifactPermissions(It.IsAny<IEnumerable<int>>(), _userId, It.IsAny<bool>(), It.IsAny<int>(), It.IsAny<bool>(), null))
                .ReturnsAsync(_artifactPermissions);

            _collectionsRepository.Setup(repo => repo.GetContentArtifactIdsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
                .ReturnsAsync(_artifactIds.ToList());

            _itemInfoRepository.Setup(repo => repo.GetItemsDetails(It.IsAny<int>(), It.IsAny<List<int>>(), It.IsAny<bool>(), It.IsAny<int>(), null))
                .ReturnsAsync(_artifacts);

            _searchEngineService.Setup(repo => repo.Search(It.IsAny<int>(), It.IsAny<Pagination>(), It.IsAny<ScopeType>(), It.IsAny<bool>(), It.IsAny<int>()))
                .ReturnsAsync(_searchArtifactsResult);

            InitializeProfileColumnsAndPropertyTypeInfos(_profileColumnsSettings, _propertyTypeInfos);

            _reviewItemsRemovalParams =
                new ReviewItemsRemovalParams()
                {
                    ItemIds = new List<int> { 1, 2, 3 },
                    SelectionType = SelectionType.Selected
                };
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
        [ExpectedException(typeof(BadRequestException))]
        public async Task RemoveArtifactsFromCollectionAsync_InvalidItemIds_ItemIdsIsNull_BadRequestException()
        {
            _reviewItemsRemovalParams.ItemIds = null;
            await _collectionService.RemoveArtifactsFromCollectionAsync(_collectionId, _reviewItemsRemovalParams, _userId);
        }

        [TestMethod]
        [ExpectedException(typeof(BadRequestException))]
        public async Task RemoveArtifactsFromCollectionAsync_InvalidItemIds_ItemIdsIsEmpty_BadRequestException()
        {
            _reviewItemsRemovalParams.ItemIds = new List<int>();
            await _collectionService.RemoveArtifactsFromCollectionAsync(_collectionId, _reviewItemsRemovalParams, _userId);
        }

        [TestMethod]
        public async Task RemoveArtifactsFromCollectionAsync_AllParametersAreValid_Success()
        {
            _collectionsRepository.Setup(q => q.RemoveArtifactsFromCollectionAsync(_collectionId, It.IsAny<IEnumerable<int>>(), _userId, null))
                .ReturnsAsync(_artifacts.Count);

            var result = await _collectionService.RemoveArtifactsFromCollectionAsync(_collectionId, _reviewItemsRemovalParams, _userId);

            Assert.IsNotNull(result);
            Assert.AreEqual(_artifacts.Count, result.RemovedCount);
        }

        [TestMethod]
        [ExpectedException(typeof(AuthorizationException))]
        public async Task RemoveArtifactsFromCollectionAsync_ValidateCollection_NoEditPermissionArtifact_NoEditPermissionException()
        {
            _artifactPermissionsRepository
                .Setup(repo => repo.HasEditPermissions(_collectionId, _userId, false, int.MaxValue, true, null))
                .ReturnsAsync(false);
            await _collectionService.RemoveArtifactsFromCollectionAsync(_collectionId, _reviewItemsRemovalParams, _userId);
        }

        [TestMethod]
        [ExpectedException(typeof(ConflictException))]
        public async Task RemoveArtifactsFromCollectionAsync_LockAsync_ArtifactLockedByAnotherUser_LockedByAnotherUserException()
        {
            _userId = 2;
            _artifactPermissionsRepository.Setup(repo => repo.HasReadPermissions(_collectionId, _userId, false, int.MaxValue, true)).ReturnsAsync(true);
            _artifactPermissionsRepository.Setup(repo => repo.HasEditPermissions(It.IsAny<int>(), _userId, It.IsAny<bool>(), It.IsAny<int>(), It.IsAny<bool>(), null)).ReturnsAsync(true);

            await _collectionService.RemoveArtifactsFromCollectionAsync(_collectionId, _reviewItemsRemovalParams, _userId);
        }

        [TestMethod]
        [ExpectedException(typeof(ConflictException))]
        public async Task RemoveArtifactsFromCollectionAsync_LockAsync_ArtifactNotLocked_ArtifactNotLockedException()
        {
            _artifactRepository.Setup(q => q.GetArtifactBasicDetails(_collectionId, _userId, null))
                .ReturnsAsync(new ArtifactBasicDetails { ArtifactId = _collectionId, LockedByUserId = null, PrimitiveItemTypePredefined = (int)ItemTypePredefined.ArtifactCollection });

            _lockArtifactsRepository.Setup(repo => repo.LockArtifactAsync(_collectionId, _userId, null))
                .ReturnsAsync(false);

            await _collectionService.RemoveArtifactsFromCollectionAsync(_collectionId, _reviewItemsRemovalParams, _userId);
        }

        [TestMethod]
        [ExpectedException(typeof(ResourceNotFoundException))]
        public async Task RemoveArtifactsFromCollectionAsync_GetCollectionBasicDetailsAsync_ArtifactIsNotFound_ResourceNotFoundException()
        {
            _artifactRepository.Setup(q => q.GetArtifactBasicDetails(_collectionId, _userId, null))
                .ReturnsAsync(new ArtifactBasicDetails
                {
                    ArtifactId = _collectionId,
                    LockedByUserId = null,
                    PrimitiveItemTypePredefined = (int)ItemTypePredefined.ArtifactCollection,
                    DraftDeleted = true
                });
            await _collectionService.RemoveArtifactsFromCollectionAsync(_collectionId, _reviewItemsRemovalParams, _userId);
        }

        [TestMethod]
        [ExpectedException(typeof(BadRequestException))]
        public async Task RemoveArtifactsFromCollectionAsync_GetCollectionBasicDetailsAsync_ArtifactIsNotArtifactCollection_InvalidTypeException()
        {
            _artifactRepository.Setup(q => q.GetArtifactBasicDetails(_collectionId, _userId, null))
                .ReturnsAsync(new ArtifactBasicDetails
                {
                    ArtifactId = _collectionId,
                    LockedByUserId = null,
                    PrimitiveItemTypePredefined = (int)ItemTypePredefined.Actor
                });

            await _collectionService.RemoveArtifactsFromCollectionAsync(_collectionId, _reviewItemsRemovalParams, _userId);
        }

        [TestMethod]
        [ExpectedException(typeof(AuthorizationException))]
        public async Task RemoveArtifactsFromCollectionAsync_GetCollectionBasicDetailsAsync_UsersNoHasReadPermissions_AuthorizationException()
        {
            _userId = 2;
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

        #endregion SaveColumnSettingsAsync

        #region GetColumnsAsync

        [TestMethod]
        public async Task GetColumnsAsync_ProfileColumnsSettingsAreEmptySelectedColumnsEmpty_Success()
        {
            _profileColumnsSettings = new ProfileColumns(new List<ProfileColumn>());

            InitializeProfileColumnsAndPropertyTypeInfos(_profileColumnsSettings, _propertyTypeInfos);

            var result = await _collectionService.GetColumnsAsync(_collectionId, _userId);

            Assert.IsNotNull(result);
            Assert.IsFalse(result.SelectedColumns.Any());
        }

        [TestMethod]
        public async Task GetColumnsAsync_PropertyTypeInfosAreEmptySelectedColumnsNotEmpty_Success()
        {
            _propertyTypeInfos = new List<PropertyTypeInfo>();

            InitializeProfileColumnsAndPropertyTypeInfos(_profileColumnsSettings, _propertyTypeInfos);

            var result = await _collectionService.GetColumnsAsync(_collectionId, _userId);

            Assert.IsNotNull(result);
            Assert.IsTrue(result.SelectedColumns.Any());
        }

        [TestMethod]
        public async Task GetColumnsAsync_PropertyTypeInfosExistInProfileColumnsSettingsSelectedColumnsNotEmpty_Success()
        {
            InitializeProfileColumnsAndPropertyTypeInfos(_profileColumnsSettings, _propertyTypeInfos);

            var result = await _collectionService.GetColumnsAsync(_collectionId, _userId);

            Assert.IsNotNull(result);
            Assert.IsTrue(result.SelectedColumns.Any());
        }

        [TestMethod]
        public async Task GetColumnsAsync_SearchByNameMatchesSelectedColumnsNotEmpty_Success()
        {
            string searchWildCard = "Cust";

            var result = await _collectionService.GetColumnsAsync(_collectionId, _userId, searchWildCard);

            Assert.IsNotNull(result);
            Assert.IsTrue(result.SelectedColumns.Any());
        }

        [TestMethod]
        public async Task GetColumnsAsync_SearchByNameDoesNotMatchSelectedColumnsEmpty_Success()
        {
            string searchWildCard = "Test";

            var result = await _collectionService.GetColumnsAsync(_collectionId, _userId, searchWildCard);

            Assert.IsNotNull(result);
            Assert.IsFalse(result.SelectedColumns.Any());
        }

        [TestMethod]
        public async Task GetColumnsAsync_PropertyTypeInfosAreCustomAndExistInProfileColumnsSettingsUnSelectedColumnsEmpty_Success()
        {
            var result = await _collectionService.GetColumnsAsync(_collectionId, _userId);

            Assert.IsNotNull(result);
            Assert.IsFalse(result.UnselectedColumns.Any());
        }

        [TestMethod]
        public async Task GetColumnsAsync_ProfileColumnsSettingsAreEmptyUnSelectedColumnsNotEmpty_Success()
        {
            _profileColumnsSettings = new ProfileColumns(new List<ProfileColumn>());

            InitializeProfileColumnsAndPropertyTypeInfos(_profileColumnsSettings, _propertyTypeInfos);

            var result = await _collectionService.GetColumnsAsync(_collectionId, _userId);

            Assert.IsNotNull(result);
            Assert.IsTrue(result.UnselectedColumns.Any());
        }

        [TestMethod]
        public async Task GetColumnsAsync_PropertyTypeInfosAreEmptyUnSelectedColumnsEmpty_Success()
        {
            _propertyTypeInfos = new List<PropertyTypeInfo>();

            InitializeProfileColumnsAndPropertyTypeInfos(_profileColumnsSettings, _propertyTypeInfos);

            var result = await _collectionService.GetColumnsAsync(_collectionId, _userId);

            Assert.IsNotNull(result);
            Assert.IsFalse(result.UnselectedColumns.Any());
        }

        [TestMethod]
        public async Task GetColumnsAsync_PropertyTypeInfoPredefinedIsNotSystemUnSelectedColumnsEmpty_Success()
        {
            _propertyTypeInfos = new List<PropertyTypeInfo>()
            {
                new PropertyTypeInfo()
                {
                    Id = 2,
                    Predefined = PropertyTypePredefined.ColumnLabel,
                    Name = "NonSystem",
                    PrimitiveType = PropertyPrimitiveType.Number
                }
            };

            InitializeProfileColumnsAndPropertyTypeInfos(_profileColumnsSettings, _propertyTypeInfos);

            var result = await _collectionService.GetColumnsAsync(_collectionId, _userId);

            Assert.IsNotNull(result);
            Assert.IsFalse(result.UnselectedColumns.Any());
        }

        [TestMethod]
        public async Task GetColumnsAsync_ProfileColumnsSettingHasSamePredefeinedAsPropertyTypeInfoUnSelectedColumnsEmpty_Success()
        {
            _profileColumnsSettings = new ProfileColumns(new List<ProfileColumn>()
            {
                new ProfileColumn("System", PropertyTypePredefined.ID, PropertyPrimitiveType.Number, 2)
            });

            _propertyTypeInfos = new List<PropertyTypeInfo>()
            {
                new PropertyTypeInfo
                {
                    Id = 2,
                    Predefined = PropertyTypePredefined.ID,
                    Name = "System",
                    PrimitiveType = PropertyPrimitiveType.Number
                }
            };

            InitializeProfileColumnsAndPropertyTypeInfos(_profileColumnsSettings, _propertyTypeInfos);

            var result = await _collectionService.GetColumnsAsync(_collectionId, _userId);

            Assert.IsNotNull(result);
            Assert.IsFalse(result.UnselectedColumns.Any());
        }

        #endregion GetColumnsAsync

        #region Private methods

        private void InitializeProfileColumnsAndPropertyTypeInfos(ProfileColumns profileColumnsSettings, List<PropertyTypeInfo> propertyTypeInfos)
        {
            _artifactListService.Setup(repo => repo.GetProfileColumnsAsync(It.IsAny<int>(), It.IsAny<int>(), ProfileColumns.Default))
                .ReturnsAsync(profileColumnsSettings);

            _collectionsRepository.Setup(repo => repo.GetPropertyTypeInfosForItemTypesAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<string>()))
                .ReturnsAsync((propertyTypeInfos));
        }

        #endregion
    }
}
