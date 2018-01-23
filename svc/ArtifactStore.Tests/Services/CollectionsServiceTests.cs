using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ArtifactStore.ArtifactList;
using ArtifactStore.Collections;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using SearchEngineLibrary.Service;
using ServiceLibrary.Helpers;
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

    }
}