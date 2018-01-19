using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ArtifactStore.Services.Collections;
using ArtifactStore.Services.Workflow;
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

        private int SessionUserId = 1;
        private ISet<int> artifactIds;
        private int CollectionId;

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

            _collectionService = new CollectionsService(_collectionsRepository.Object,
                                                        _artifactRepository.Object,
                                                        _lockArtifactsRepository.Object,
                                                        _itemInfoRepository.Object,
                                                        _artifactPermissionsRepository.Object,
                                                        _sqlHelperMock,
                                                        _searchEngineService.Object);


            artifactIds = new HashSet<int>() { 1, 2, 3 };
            CollectionId = 1;
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public async Task AddArtifactsToCollectionAsync_InvalidUserId_ThrowArgumentOutOfRangeException()
        {
            SessionUserId = 0;
            await _collectionService.AddArtifactsToCollectionAsync(SessionUserId, CollectionId, artifactIds);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public async Task AddArtifactsToCollectionAsync_InvalidCollectionId_ThrowArgumentOutOfRangeException()
        {
            CollectionId = 0;
            await _collectionService.AddArtifactsToCollectionAsync(SessionUserId, CollectionId, artifactIds);
        }

    }
}