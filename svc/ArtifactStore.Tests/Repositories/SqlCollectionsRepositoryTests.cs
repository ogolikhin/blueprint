using System.Collections.Generic;
using System.Threading.Tasks;
using ArtifactStore.Collections;
using ArtifactStore.Collections.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using ServiceLibrary.Repositories;
using System.Linq;

namespace ArtifactStore.Repositories
{
    [TestClass]
    public class SqlCollectionsRepositoryTests
    {
        private SqlConnectionWrapperMock _cxn;
        private ICollectionsRepository _collectionRepository;

        private const int UserId = 1;
        private int CollectionId = 1;
        private List<CollectionArtifact> _expectedCollectionArtifacts;


        [TestInitialize]
        public void Initialize()
        {
            _cxn = new SqlConnectionWrapperMock();
            _collectionRepository = new SqlCollectionsRepository(_cxn.Object);

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
            await _collectionRepository.AddArtifactsToCollectionAsync(CollectionId, new List<int> { 1, 2, 3 }, UserId);
        }

        #endregion

        #region GetArtifactsWithPropertyValuesAsync

        [TestMethod]
        public async Task GetArtifactsWithPropertyValuesAsync_AllParametersAreValid_Success()
        {
            // Arrange
            _cxn.SetupQueryAsync("GetPropertyValuesForArtifacts", It.IsAny<Dictionary<string, object>>(), _expectedCollectionArtifacts);

            // Act
            var actualResult = await _collectionRepository.GetArtifactsWithPropertyValuesAsync(UserId, new List<int> { 1, 2, 3 });

            // assert
            Assert.IsNotNull(actualResult);
            Assert.AreEqual(_expectedCollectionArtifacts.Count, actualResult.Count);
        }

        #endregion
    }
}