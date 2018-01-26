using System;
using System.Collections.Generic;
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

namespace ArtifactStore.Services
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

        private int _sessionUserId = 1;
        private ISet<int> _artifactIds;
        private int _collectionId;
        private ProfileColumnsSettings _profileColumnsSettings;
        private ArtifactBasicDetails _collectionDetails;

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
            _collectionService = new CollectionsService(_collectionsRepository.Object,
                                                        _artifactRepository.Object,
                                                        _lockArtifactsRepository.Object,
                                                        _itemInfoRepository.Object,
                                                        _artifactPermissionsRepository.Object,
                                                        _sqlHelperMock,
                                                        _searchEngineService.Object,
                                                        _artifactListService.Object);

            _artifactIds = new HashSet<int>() { 1, 2, 3 };
            _collectionId = 1;

            _profileColumnsSettings = new ProfileColumnsSettings()
            {
                Items = new List<ProfileColumn>()
                {
                    new ProfileColumn()
                    {
                        Predefined = 1,
                        PropertyName = "Custom",
                        PropertyTypeId = 2
                    }
                }
            };

            _collectionDetails = new ArtifactBasicDetails()
            {
                ArtifactId = 1,
                DraftDeleted = false,
                PrimitiveItemTypePredefined = (int)ItemTypePredefined.ArtifactCollection
            };
        }

        #region AddArtifactsToCollectionAsync

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public async Task AddArtifactsToCollectionAsync_InvalidUserId_ThrowArgumentOutOfRangeException()
        {
            _sessionUserId = 0;
            await _collectionService.AddArtifactsToCollectionAsync(_collectionId, _artifactIds, _sessionUserId);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public async Task AddArtifactsToCollectionAsync_InvalidCollectionId_ThrowArgumentOutOfRangeException()
        {
            _collectionId = 0;
            await _collectionService.AddArtifactsToCollectionAsync(_collectionId, _artifactIds, _sessionUserId);
        }

        #endregion AddArtifactsToCollectionAsync

        #region SaveColumnSettingsAsync

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public async Task SaveColumnSettingsAsync_InvalidUserId_ThrowArgumentOutOfRangeException()
        {
            _sessionUserId = 0;
            await _collectionService.SaveColumnSettingsAsync(_collectionId, _profileColumnsSettings, _sessionUserId);
        }

        #endregion SaveColumnSettingsAsync

        #region GetColumnsAsync

        [TestMethod]
        public async Task GetColumnsAsync_SelectedColumnsEmpty_Success()
        {
            List<int> artifactIds = new List<int>() { 1, 2, 3 };

            List<ItemDetails> artifacts = new List<ItemDetails>()
            {
                new ItemDetails()
                {
                    Name = "Artifact1",
                    ItemTypeId = 2
                }
            };

            List<PropertyTypeInfo> artifacTypeInfos = new List<PropertyTypeInfo>()
            {
                new PropertyTypeInfo()
                {
                    Id = 1,
                    Predefined = PropertyTypePredefined.ArtifactType,
                    Name = "Custom",
                    PrimitiveType = PropertyPrimitiveType.Number
                }
            };

            _artifactRepository.Setup(repo => repo.GetArtifactBasicDetails(It.IsAny<int>(), It.IsAny<int>(), null))
                .ReturnsAsync(_collectionDetails);

            _artifactPermissionsRepository.Setup(repo => repo.HasReadPermissions(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<int>(), It.IsAny<bool>()))
                .ReturnsAsync(true);

            _artifactListService.Setup(repo => repo.GetColumnSettingsAsync(It.IsAny<int>(), It.IsAny<int>()))
                .ReturnsAsync(/*_profileColumnsSettings*/(ProfileColumnsSettings)null);

            _collectionsRepository.Setup(repo => repo.GetContentArtifactIdsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
                .ReturnsAsync((artifactIds));

            _itemInfoRepository.Setup(repo => repo.GetItemsDetails(It.IsAny<int>(), It.IsAny<List<int>>(), It.IsAny<bool>(), It.IsAny<int>(), null))
                .ReturnsAsync((artifacts));

            _collectionsRepository.Setup(repo => repo.GetPropertyTypeInfosForItemTypesAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<string>()))
                .ReturnsAsync((artifacTypeInfos));

            var result = await _collectionService.GetColumnsAsync(_collectionId, _sessionUserId);
            Assert.IsNotNull(result);
        }

        #endregion GetColumnsAsync
    }
}