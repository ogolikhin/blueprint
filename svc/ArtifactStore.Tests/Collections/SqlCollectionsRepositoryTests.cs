using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using ArtifactStore.Collections.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using ServiceLibrary.Repositories;

namespace ArtifactStore.Collections
{
    [TestClass]
    public class SqlCollectionsRepositoryTests
    {
        private int _userId;
        private int _collectionId;
        private List<CollectionArtifact> _expectedCollectionArtifacts;

        private SqlConnectionWrapperMock _cxn;
        private SqlCollectionsRepository _repository;

        [TestInitialize]
        public void Initialize()
        {
            _userId = 1;
            _collectionId = 1;
            _expectedCollectionArtifacts = new List<CollectionArtifact>
            {
                new CollectionArtifact
                {
                    PropertyName = "Name",
                    PropertyTypeId = 80,
                    PropertyTypePredefined = 4098,
                    Prefix = "Prefix",
                    ArtifactId = 7545,
                    PrimitiveType = 0,
                    ItemTypeId = 134,
                    PredefinedType = 4107,
                    PropertyValue = "Value_Name",
                    ItemTypeIconId = 0
                },
                new CollectionArtifact
                {
                    PropertyName = "Description",
                    PropertyTypeId = 81,
                    PropertyTypePredefined = 4099,
                    Prefix = "Prefix",
                    ArtifactId = 7551,
                    PrimitiveType = 0,
                    ItemTypeId = 132,
                    PredefinedType = 4105,
                    PropertyValue = "Value_Description",
                    ItemTypeIconId = 0
                }
            };

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

        #region RemoveDeletedArtifactsFromCollection

        [TestMethod]
        public async Task RemoveDeletedArtifactsFromCollection_AllParametersAreValid_Success()
        {
            // Arrange

            Exception exception = null;

            // Act
            try
            {
                await _repository.RemoveDeletedArtifactsFromCollectionAsync(_collectionId, _userId);
            }
            catch (Exception ex)
            {
                exception = ex;
            }

            Assert.IsNull(exception);
        }

        #endregion

        #region AddArtifactsToCollectionAsync

        [TestMethod]
        public async Task AddArtifactsToCollectionAsync_AllParametersAreValid_Success()
        {
            // Arrange
            var expectedResult = 3;

            var artifactIds = new List<int> { 1, 2, 3 };

            _cxn.SetupExecuteScalarAsync("AddArtifactsToCollection", It.IsAny<Dictionary<string, object>>(),
                expectedResult);

            // Act
            var actualResult = await _repository.AddArtifactsToCollectionAsync(_collectionId, artifactIds, _userId);
            Assert.IsNotNull(actualResult);
            Assert.AreEqual(expectedResult, actualResult);
        }

        #endregion

        #region RemoveArtifactsFromCollectionAsync

        [TestMethod]
        public async Task RemoveArtifactsFromCollectionAsync_AllParametersAreValid_Success()
        {
            // Arrange
            var expectedResult = 3;

            var artifactIds = new List<int> { 1, 2, 3 };

            _cxn.SetupExecuteScalarAsync("RemoveArtifactsFromCollection", It.IsAny<Dictionary<string, object>>(),
                expectedResult);

            // Act
            var actualResult = await _repository.RemoveArtifactsFromCollectionAsync(_collectionId, artifactIds, _userId);
            Assert.IsNotNull(actualResult);
            Assert.AreEqual(expectedResult, actualResult);
        }

        #endregion

        #region GetArtifactsWithPropertyValuesAsync

        [TestMethod]
        public async Task GetArtifactsWithPropertyValuesAsync_AllParametersAreValid_Success()
        {
            // Arrange
            var artifactIds = new List<int> { 1, 2, 3 };
            _cxn.SetupQueryAsync("GetPropertyValuesForArtifacts", It.IsAny<Dictionary<string, object>>(),
                _expectedCollectionArtifacts);

            // Act
            var actualResult =
                await _repository.GetArtifactsWithPropertyValuesAsync(_userId, artifactIds);

            // assert
            Assert.IsNotNull(actualResult);
            Assert.AreEqual(_expectedCollectionArtifacts.Count, actualResult.Count);
        }

        #endregion
    }
}
