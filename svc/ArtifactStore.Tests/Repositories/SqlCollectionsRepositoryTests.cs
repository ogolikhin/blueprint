using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;
using ArtifactStore.Collections;

namespace ServiceLibrary.Repositories
{
    [TestClass]
    public class SqlCollectionsRepositoryTests
    {
        private SqlConnectionWrapperMock _cxn;
        private ICollectionsRepository _collectionRepository;

        private const int UserId = 1;
        private int CollectionId = 1;


        [TestInitialize]
        public void Initialize()
        {
            _cxn = new SqlConnectionWrapperMock();
            _collectionRepository = new SqlCollectionsRepository(_cxn.Object);
        }

        #region RemoveDeletedArtifactsFromCollection

        [TestMethod]
        public async Task RemoveDeletedArtifactsFromCollection_AllParametersAreValid_Success()
        {
            // Arrange

            // Act
            await _collectionRepository.RemoveDeletedArtifactsFromCollectionAsync(CollectionId, UserId);
        }

        #endregion

        #region AddArtifactsToCollectionAsync

        [TestMethod]
        public async Task AddArtifactsToCollectionAsync_AllParametersAreValid_Success()
        {
            // Arrange

            // Act
            await _collectionRepository.AddArtifactsToCollectionAsync(CollectionId, new List<int>() { 1, 2, 3 }, UserId);
        }

        #endregion
    }
}