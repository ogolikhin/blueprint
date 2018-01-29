using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ArtifactStore.ArtifactList;
using ArtifactStore.ArtifactList.Models;
using ArtifactStore.Collections;
using ArtifactStore.Collections.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using SearchEngineLibrary.Service;
using ServiceLibrary.Helpers;
using ServiceLibrary.Models;
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
        private List<PropertyTypeInfo> _artifacTypeInfos;
        private ProfileColumns _profileColumns;

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

            _profileColumnsSettings = new ProfileColumns(new List<ProfileColumn>()
                {
                    new ProfileColumn("Custom", PropertyTypePredefined.CustomGroup, PropertyPrimitiveType.Number, 2)
                    {
                    }
                })
            {

            };

            _collectionDetails = new ArtifactBasicDetails()
            {
                ArtifactId = 1,
                DraftDeleted = false,
                PrimitiveItemTypePredefined = (int)ItemTypePredefined.ArtifactCollection
            };

            _artifacts = new List<ItemDetails>()
            {
                new ItemDetails()
                {
                    Name = "Artifact1",
                    ItemTypeId = 2
                }
            };

            _profileColumns = new ProfileColumns(
            new List<ProfileColumn>
            {
                new ProfileColumn("Custom", PropertyTypePredefined.CustomGroup, PropertyPrimitiveType.Text, 2)
            });

            _artifacTypeInfos = new List<PropertyTypeInfo>()
            {
                new PropertyTypeInfo()
                {
                    Id = 2,
                    Predefined = PropertyTypePredefined.CustomGroup,
                    Name = "Custom",
                    PrimitiveType = PropertyPrimitiveType.Number
                }
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
        public async Task GetColumnsAsync_SelectedColumnsEmpty_Success()
        {
            _artifactRepository.Setup(repo => repo.GetArtifactBasicDetails(It.IsAny<int>(), It.IsAny<int>(), null))
                .ReturnsAsync(_collectionDetails);

            _artifactPermissionsRepository.Setup(repo => repo.HasReadPermissions(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<int>(), It.IsAny<bool>()))
                .ReturnsAsync(true);

            _profileColumnsSettings = new ProfileColumns(new List<ProfileColumn>());

            _artifactListService.Setup(repo => repo.GetProfileColumnsAsync(It.IsAny<int>(), It.IsAny<int>(), ProfileColumns.Default))
                .ReturnsAsync(_profileColumnsSettings);

            _collectionsRepository.Setup(repo => repo.GetContentArtifactIdsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
                .ReturnsAsync((_artifactIds.ToList()));

            _itemInfoRepository.Setup(repo => repo.GetItemsDetails(It.IsAny<int>(), It.IsAny<List<int>>(), It.IsAny<bool>(), It.IsAny<int>(), null))
                .ReturnsAsync((_artifacts));

            _collectionsRepository.Setup(repo => repo.GetPropertyTypeInfosForItemTypesAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<string>()))
                .ReturnsAsync((_artifacTypeInfos));

            var result = await _collectionService.GetColumnsAsync(_collectionId, _userId);

            Assert.IsNotNull(result);
            Assert.AreEqual(0, result.SelectedColumns.Count());
        }

        [TestMethod]
        public async Task GetColumnsAsync_SelectedColumnsNotEmpty_Success()
        {
            _artifactRepository.Setup(repo => repo.GetArtifactBasicDetails(It.IsAny<int>(), It.IsAny<int>(), null))
                .ReturnsAsync(_collectionDetails);

            _artifactPermissionsRepository.Setup(repo => repo.HasReadPermissions(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<int>(), It.IsAny<bool>()))
                .ReturnsAsync(true);

            _artifactListService.Setup(repo => repo.GetProfileColumnsAsync(It.IsAny<int>(), It.IsAny<int>(), ProfileColumns.Default))
                .ReturnsAsync(_profileColumnsSettings);

            _collectionsRepository.Setup(repo => repo.GetContentArtifactIdsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
                .ReturnsAsync((_artifactIds.ToList()));

            _itemInfoRepository.Setup(repo => repo.GetItemsDetails(It.IsAny<int>(), It.IsAny<List<int>>(), It.IsAny<bool>(), It.IsAny<int>(), null))
                .ReturnsAsync((_artifacts));

            _collectionsRepository.Setup(repo => repo.GetPropertyTypeInfosForItemTypesAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<string>()))
                .ReturnsAsync((_artifacTypeInfos));

            var result = await _collectionService.GetColumnsAsync(_collectionId, _userId);

            Assert.IsNotNull(result);
            Assert.IsTrue(result.SelectedColumns.Any());
        }

        [TestMethod]
        public async Task GetColumnsAsync_SearchByNameMatchesSelectedColumnsNotEmpty_Success()
        {
            string searchWildCard = "Cust";

            _artifactRepository.Setup(repo => repo.GetArtifactBasicDetails(It.IsAny<int>(), It.IsAny<int>(), null))
                .ReturnsAsync(_collectionDetails);

            _artifactPermissionsRepository.Setup(repo => repo.HasReadPermissions(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<int>(), It.IsAny<bool>()))
                .ReturnsAsync(true);

            _artifactListService.Setup(repo => repo.GetProfileColumnsAsync(It.IsAny<int>(), It.IsAny<int>(), ProfileColumns.Default))
                .ReturnsAsync(_profileColumnsSettings);

            _collectionsRepository.Setup(repo => repo.GetContentArtifactIdsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
                .ReturnsAsync((_artifactIds.ToList()));

            _itemInfoRepository.Setup(repo => repo.GetItemsDetails(It.IsAny<int>(), It.IsAny<List<int>>(), It.IsAny<bool>(), It.IsAny<int>(), null))
                .ReturnsAsync((_artifacts));

            _collectionsRepository.Setup(repo => repo.GetPropertyTypeInfosForItemTypesAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<string>()))
                .ReturnsAsync((_artifacTypeInfos));

            var result = await _collectionService.GetColumnsAsync(_collectionId, _userId, searchWildCard);

            Assert.IsNotNull(result);
            Assert.IsTrue(result.SelectedColumns.Any());
        }

        [TestMethod]
        public async Task GetColumnsAsync_SearchByNameDoeNotMatchSelectedColumnsNotEmpty_Success()
        {
            string searchWildCard = "Test";

            _artifactRepository.Setup(repo => repo.GetArtifactBasicDetails(It.IsAny<int>(), It.IsAny<int>(), null))
                .ReturnsAsync(_collectionDetails);

            _artifactPermissionsRepository.Setup(repo => repo.HasReadPermissions(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<int>(), It.IsAny<bool>()))
                .ReturnsAsync(true);

            _artifactListService.Setup(repo => repo.GetProfileColumnsAsync(It.IsAny<int>(), It.IsAny<int>(), ProfileColumns.Default))
                .ReturnsAsync(_profileColumnsSettings);

            _collectionsRepository.Setup(repo => repo.GetContentArtifactIdsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
                .ReturnsAsync((_artifactIds.ToList()));

            _itemInfoRepository.Setup(repo => repo.GetItemsDetails(It.IsAny<int>(), It.IsAny<List<int>>(), It.IsAny<bool>(), It.IsAny<int>(), null))
                .ReturnsAsync((_artifacts));

            _collectionsRepository.Setup(repo => repo.GetPropertyTypeInfosForItemTypesAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<string>()))
                .ReturnsAsync((_artifacTypeInfos));

            var result = await _collectionService.GetColumnsAsync(_collectionId, _userId, searchWildCard);

            Assert.IsNotNull(result);
            Assert.AreEqual(0, result.SelectedColumns.Count());
        }

        [TestMethod]
        public async Task GetColumnsAsync_UnSelectedColumnsEmpty_Success()
        {
            _artifactRepository.Setup(repo => repo.GetArtifactBasicDetails(It.IsAny<int>(), It.IsAny<int>(), null))
                .ReturnsAsync(_collectionDetails);

            _artifactPermissionsRepository.Setup(repo => repo.HasReadPermissions(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<int>(), It.IsAny<bool>()))
                .ReturnsAsync(true);

            _artifactListService.Setup(repo => repo.GetProfileColumnsAsync(It.IsAny<int>(), It.IsAny<int>(), ProfileColumns.Default))
                .ReturnsAsync(_profileColumnsSettings);

            _collectionsRepository.Setup(repo => repo.GetContentArtifactIdsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
                .ReturnsAsync((_artifactIds.ToList()));

            _itemInfoRepository.Setup(repo => repo.GetItemsDetails(It.IsAny<int>(), It.IsAny<List<int>>(), It.IsAny<bool>(), It.IsAny<int>(), null))
                .ReturnsAsync((_artifacts));

            _collectionsRepository.Setup(repo => repo.GetPropertyTypeInfosForItemTypesAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<string>()))
                .ReturnsAsync((_artifacTypeInfos));

            var result = await _collectionService.GetColumnsAsync(_collectionId, _userId);

            Assert.IsNotNull(result);
            Assert.AreEqual(0, result.UnselectedColumns.Count());
        }

        [TestMethod]
        public async Task GetColumnsAsync_ProfileColumnsAreEmptySettingsUnSelectedColumnsEmpty_Success()
        {
            _profileColumnsSettings = new ProfileColumns(new List<ProfileColumn>());

            _artifactRepository.Setup(repo => repo.GetArtifactBasicDetails(It.IsAny<int>(), It.IsAny<int>(), null))
                .ReturnsAsync(_collectionDetails);

            _artifactPermissionsRepository.Setup(repo => repo.HasReadPermissions(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<int>(), It.IsAny<bool>()))
                .ReturnsAsync(true);

            _artifactListService.Setup(repo => repo.GetProfileColumnsAsync(It.IsAny<int>(), It.IsAny<int>(), ProfileColumns.Default))
                .ReturnsAsync(_profileColumnsSettings);

            _collectionsRepository.Setup(repo => repo.GetContentArtifactIdsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
                .ReturnsAsync((_artifactIds.ToList()));

            _itemInfoRepository.Setup(repo => repo.GetItemsDetails(It.IsAny<int>(), It.IsAny<List<int>>(), It.IsAny<bool>(), It.IsAny<int>(), null))
                .ReturnsAsync((_artifacts));

            _collectionsRepository.Setup(repo => repo.GetPropertyTypeInfosForItemTypesAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<string>()))
                .ReturnsAsync((_artifacTypeInfos));

            var result = await _collectionService.GetColumnsAsync(_collectionId, _userId);

            Assert.IsNotNull(result);
            Assert.IsTrue(result.UnselectedColumns.Any());
        }

        [TestMethod]
        public async Task GetColumnsAsync_ProfileColumnsInvalidIdSettingsUnSelectedColumnsNotEmpty_Success()
        {
            _artifacTypeInfos[0].Id = -1;

            _artifactRepository.Setup(repo => repo.GetArtifactBasicDetails(It.IsAny<int>(), It.IsAny<int>(), null))
                .ReturnsAsync(_collectionDetails);

            _artifactPermissionsRepository.Setup(repo => repo.HasReadPermissions(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<int>(), It.IsAny<bool>()))
                .ReturnsAsync(true);

            _artifactListService.Setup(repo => repo.GetProfileColumnsAsync(It.IsAny<int>(), It.IsAny<int>(), ProfileColumns.Default))
                .ReturnsAsync(_profileColumnsSettings);

            _collectionsRepository.Setup(repo => repo.GetContentArtifactIdsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
                .ReturnsAsync((_artifactIds.ToList()));

            _itemInfoRepository.Setup(repo => repo.GetItemsDetails(It.IsAny<int>(), It.IsAny<List<int>>(), It.IsAny<bool>(), It.IsAny<int>(), null))
                .ReturnsAsync((_artifacts));

            _collectionsRepository.Setup(repo => repo.GetPropertyTypeInfosForItemTypesAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<string>()))
                .ReturnsAsync((_artifacTypeInfos));

            var result = await _collectionService.GetColumnsAsync(_collectionId, _userId);

            Assert.IsNotNull(result);
            Assert.IsTrue(result.UnselectedColumns.Any());
        }

        [TestMethod]
        public async Task GetColumnsAsync_ProfileColumnsEmptySettingsUnSelectedColumnsEmpty_Success()
        {
            _artifacTypeInfos = new List<PropertyTypeInfo>();

            _artifactRepository.Setup(repo => repo.GetArtifactBasicDetails(It.IsAny<int>(), It.IsAny<int>(), null))
                .ReturnsAsync(_collectionDetails);

            _artifactPermissionsRepository.Setup(repo => repo.HasReadPermissions(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<int>(), It.IsAny<bool>()))
                .ReturnsAsync(true);

            _artifactListService.Setup(repo => repo.GetProfileColumnsAsync(It.IsAny<int>(), It.IsAny<int>(), ProfileColumns.Default))
                .ReturnsAsync(_profileColumnsSettings);

            _collectionsRepository.Setup(repo => repo.GetContentArtifactIdsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
                .ReturnsAsync((_artifactIds.ToList()));

            _itemInfoRepository.Setup(repo => repo.GetItemsDetails(It.IsAny<int>(), It.IsAny<List<int>>(), It.IsAny<bool>(), It.IsAny<int>(), null))
                .ReturnsAsync((_artifacts));

            _collectionsRepository.Setup(repo => repo.GetPropertyTypeInfosForItemTypesAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<string>()))
                .ReturnsAsync((_artifacTypeInfos));

            var result = await _collectionService.GetColumnsAsync(_collectionId, _userId);

            Assert.IsNotNull(result);
            Assert.AreEqual(0, result.UnselectedColumns.Count());
        }

        [TestMethod]
        public async Task GetColumnsAsync_PropertyTypeInfoPredefinedIsNotStandardUnSelectedColumnsNotEmpty_Success()
        {
            _artifacTypeInfos = new List<PropertyTypeInfo>()
            {
                new PropertyTypeInfo()
                {
                    Id = 2,
                    Predefined = PropertyTypePredefined.ColumnLabel,
                    Name = "NonCustom",
                    PrimitiveType = PropertyPrimitiveType.Number
                }
            };

            _artifactRepository.Setup(repo => repo.GetArtifactBasicDetails(It.IsAny<int>(), It.IsAny<int>(), null))
                .ReturnsAsync(_collectionDetails);

            _artifactPermissionsRepository.Setup(repo => repo.HasReadPermissions(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<int>(), It.IsAny<bool>()))
                .ReturnsAsync(true);

            _artifactListService.Setup(repo => repo.GetProfileColumnsAsync(It.IsAny<int>(), It.IsAny<int>(), ProfileColumns.Default))
                .ReturnsAsync(_profileColumnsSettings);

            _collectionsRepository.Setup(repo => repo.GetContentArtifactIdsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
                .ReturnsAsync((_artifactIds.ToList()));

            _itemInfoRepository.Setup(repo => repo.GetItemsDetails(It.IsAny<int>(), It.IsAny<List<int>>(), It.IsAny<bool>(), It.IsAny<int>(), null))
                .ReturnsAsync((_artifacts));

            _collectionsRepository.Setup(repo => repo.GetPropertyTypeInfosForItemTypesAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<string>()))
                .ReturnsAsync((_artifacTypeInfos));

            var result = await _collectionService.GetColumnsAsync(_collectionId, _userId);

            Assert.IsNotNull(result);
            Assert.AreEqual(0, result.UnselectedColumns.Count());
        }

        [TestMethod]
        public async Task GetColumnsAsync_ProfileColumnsSettingsHasSamePredefeinedAsArtifactTypeIsNotStandardUnSelectedColumnsNotEmpty_Success()
        {
            _artifacTypeInfos = new List<PropertyTypeInfo>()
            {
                new PropertyTypeInfo()
                {
                    Id = 2,
                    Predefined = PropertyTypePredefined.ID,
                    Name = "NonCustom",
                    PrimitiveType = PropertyPrimitiveType.Number
                }
            };

            _profileColumnsSettings = new ProfileColumns(new List<ProfileColumn>()
            {
                new ProfileColumn("Custom", PropertyTypePredefined.ID, PropertyPrimitiveType.Number, 2)
            });

            _artifactRepository.Setup(repo => repo.GetArtifactBasicDetails(It.IsAny<int>(), It.IsAny<int>(), null))
                .ReturnsAsync(_collectionDetails);

            _artifactPermissionsRepository.Setup(repo => repo.HasReadPermissions(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<int>(), It.IsAny<bool>()))
                .ReturnsAsync(true);

            _artifactListService.Setup(repo => repo.GetProfileColumnsAsync(It.IsAny<int>(), It.IsAny<int>(), ProfileColumns.Default))
                .ReturnsAsync(_profileColumnsSettings);

            _collectionsRepository.Setup(repo => repo.GetContentArtifactIdsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
                .ReturnsAsync((_artifactIds.ToList()));

            _itemInfoRepository.Setup(repo => repo.GetItemsDetails(It.IsAny<int>(), It.IsAny<List<int>>(), It.IsAny<bool>(), It.IsAny<int>(), null))
                .ReturnsAsync((_artifacts));

            _collectionsRepository.Setup(repo => repo.GetPropertyTypeInfosForItemTypesAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<string>()))
                .ReturnsAsync((_artifacTypeInfos));

            var result = await _collectionService.GetColumnsAsync(_collectionId, _userId);

            Assert.IsNotNull(result);
            Assert.AreEqual(0, result.UnselectedColumns.Count());
        }

        #endregion GetColumnsAsync
    }
}
