using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using AdminStore.Models;
using ArtifactStore.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using ServiceLibrary.Exceptions;
using ServiceLibrary.Helpers;
using ServiceLibrary.Repositories;
using static ArtifactStore.Repositories.SqlProjectMetaRepository;

namespace ArtifactStore.Repositories
{
    [TestClass]
    public class SqlProjectMetaRepositoryTests
    {
        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public async Task GetCustomProjectTypesAsync_InvalidProjectId()
        {
            // Arrange
            var cxn = new SqlConnectionWrapperMock();
            var repository = new SqlProjectMetaRepository(cxn.Object);

            // Act
            await repository.GetCustomProjectTypesAsync(0, 2);

            // Assert
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public async Task GetCustomProjectTypesAsync_InvalidUserId()
        {
            // Arrange
            var cxn = new SqlConnectionWrapperMock();
            var repository = new SqlProjectMetaRepository(cxn.Object);

            // Act
            await repository.GetCustomProjectTypesAsync(2, 0);

            // Assert
        }

        [TestMethod]
        [ExpectedException(typeof(ResourceNotFoundException))]
        public async Task GetCustomProjectTypesAsync_ProjectNotFound()
        {
            // Arrange
            var projectId = 1;
            var userId = 2;
            ProjectVersion[] result = {};
            var cxn = new SqlConnectionWrapperMock();
            var repository = new SqlProjectMetaRepository(cxn.Object);
            cxn.SetupQueryAsync("GetInstanceProjectById", new Dictionary<string, object> { { "projectId", projectId }, { "userId", userId } }, result);

            // Act
            await repository.GetCustomProjectTypesAsync(projectId, userId);

            // Assert
        }

        [TestMethod]
        [ExpectedException(typeof(AuthorizationException))]
        public async Task GetCustomProjectTypesAsync_Unauthorized()
        {
            // Arrange
            var projectId = 1;
            var userId = 2;
            ProjectVersion[] result = { new ProjectVersion { IsAccesible = false } };
            var cxn = new SqlConnectionWrapperMock();
            var repository = new SqlProjectMetaRepository(cxn.Object);
            cxn.SetupQueryAsync("GetInstanceProjectById", new Dictionary<string, object> { { "projectId", projectId }, { "userId", userId } }, result);

            // Act
            await repository.GetCustomProjectTypesAsync(projectId, userId);

            // Assert
        }

        [TestMethod]
        public async Task GetCustomProjectTypesAsync_RemoveHiddenSubartifactTypes()
        {
            // Arrange
            var ptVersions = new List<PropertyTypeVersion>();
            var itVersions = new List<ItemTypeVersion>
            {
                new ItemTypeVersion { ItemTypeId = 10, Predefined = ItemTypePredefined.Content },
                new ItemTypeVersion { ItemTypeId = 11, Predefined = ItemTypePredefined.BaselinedArtifactSubscribe },
                new ItemTypeVersion { ItemTypeId = 12, Predefined = ItemTypePredefined.Extension },
                new ItemTypeVersion { ItemTypeId = 13, Predefined = ItemTypePredefined.Flow },
                // Not hidden
                new ItemTypeVersion { ItemTypeId = 13, Predefined = ItemTypePredefined.Step}
            };
            var itptMap = new List<ItemTypePropertyTypeMapRecord>();

            InitRepository(ptVersions, itVersions, itptMap);

            var expected = ItemTypePredefined.Step;

            // Act
            var result = await _repository.GetCustomProjectTypesAsync(_projectId, _userId);

            // Assert
            _cxn.Verify();
            Assert.AreEqual(0, result.ArtifactTypes.Count);
            Assert.AreEqual(0, result.PropertyTypes.Count);
            Assert.AreEqual(1, result.SubArtifactTypes.Count);
            Assert.AreEqual(expected, result.SubArtifactTypes[0].BaseType);
        }

        private readonly int _projectId = 1;
        private readonly int _userId = 2;
        private SqlConnectionWrapperMock _cxn;
        private SqlProjectMetaRepository _repository;

        private void InitRepository(IEnumerable<PropertyTypeVersion> ptVersions,
            IEnumerable<ItemTypeVersion> itVersions,
            IEnumerable<ItemTypePropertyTypeMapRecord> itptMap)
        {
            _cxn = new SqlConnectionWrapperMock();
            _repository = new SqlProjectMetaRepository(_cxn.Object);

            ProjectVersion[] project = { new ProjectVersion { IsAccesible = true } };
            _cxn.SetupQueryAsync("GetInstanceProjectById", new Dictionary<string, object> { { "projectId", _projectId }, { "userId", _userId } }, project);

            var mockResult = Tuple.Create(ptVersions, itVersions, itptMap);
            _cxn.SetupQueryMultipleAsync("GetProjectCustomTypes", new Dictionary<string, object> { { "projectId", _projectId }, { "revisionId", ServiceConstants.VersionHead } }, mockResult);
        }
    }
}
