using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ServiceLibrary.Repositories;

namespace ArtifactStore.Collections
{
    [TestClass]
    public class SqlCollectionsRepositoryTests
    {
        private int _userId;
        private int _collectionId;

        private SqlConnectionWrapperMock _cxn;
        private SqlCollectionsRepository _repository;

        [TestInitialize]
        public void Initialize()
        {
            _userId = 1;
            _collectionId = 1;

            _cxn = new SqlConnectionWrapperMock();
            _repository = new SqlCollectionsRepository(_cxn.Object);
        }

        [TestMethod]
        public async Task GetContentArtifactIdsAsync_AddDraftsIsTrue_ReturnsCorrectResult()
        {
            // Arrange
            var queryParameters = new Dictionary<string, object>
            {
                { "@userId", _userId },
                { "@collectionId", _collectionId },
                { "@addDrafts", true }
            };
            var expectedResult = new List<int> { 2, 3, 4, 5 };

            _cxn.SetupQueryAsync(
                SqlCollectionsRepository.GetArtifactIdsInCollectionQuery,
                queryParameters,
                expectedResult,
                commandType: CommandType.Text);

            // Act
            var actualResult = await _repository.GetContentArtifactIdsAsync(_collectionId, _userId);

            // Assert
            CollectionAssert.AreEquivalent(expectedResult, actualResult.ToList());
        }

        [TestMethod]
        public async Task GetContentArtifactIdsAsync_AddDraftsIsFalse_ReturnsCorrectResult()
        {
            // Arrange
            var queryParameters = new Dictionary<string, object>
            {
                { "@userId", _userId },
                { "@collectionId", _collectionId },
                { "@addDrafts", false }
            };
            var expectedResult = new List<int> { 2, 4 };

            _cxn.SetupQueryAsync(
                SqlCollectionsRepository.GetArtifactIdsInCollectionQuery,
                queryParameters,
                expectedResult,
                commandType: CommandType.Text);

            // Act
            var actualResult = await _repository.GetContentArtifactIdsAsync(_collectionId, _userId, false);

            // Assert
            CollectionAssert.AreEquivalent(expectedResult, actualResult.ToList());
        }
    }
}
