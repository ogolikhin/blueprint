using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ActionHandlerService.Models;
using ActionHandlerService.Repositories;
using BluePrintSys.Messaging.Models.Actions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ServiceLibrary.Models.Workflow;
using ServiceLibrary.Repositories;

namespace ActionHandlerServiceTests
{
    /// <summary>
    /// Tests for the Action Handler Service Repositories
    /// </summary>
    [TestClass]
    public class RepositoryTests
    {
        [TestMethod]
        public void ActionHandlerServiceRepository_InstantiatesSuccessfully()
        {
            //arrange
            var connectionString = string.Empty;

            //act
            var repository = new ActionHandlerServiceRepository(connectionString);

            //assert
            Assert.IsNotNull(repository);
        }

        [TestMethod]
        public async Task GetPropertyModificationsForRevisionIdAsync_CompletesSuccessfuly()
        {
            //arrange
            var connectionMock = new SqlConnectionWrapperMock();
            const string storedProcedure = "GetPropertyModificationsForRevisionId";
            const int revisionId = 1;
            var parameters = new Dictionary<string, object> {{nameof(revisionId), revisionId}};
            var result = new List<SqlModifiedProperty>();
            for (int i = 0; i < 5; i++)
            {
                result.Add(
                    new SqlModifiedProperty
                    {
                        PropertyName = $"Property{i}",
                        ArtifactId = 0,
                        ItemId = 0,
                        ProjectId = 0,
                        Type = 0,
                        TypeId = 0,
                        VersionId = 0,
                        StartRevision = 0,
                        EndRevision = 0,
                        NewPropertyValue = ""
                    });
            }
            connectionMock.SetupQueryAsync(storedProcedure, parameters, result);

            //act
            var repository = new ArtifactsPublishedRepository(connectionMock.Object);
            var repositoryResult = await repository.GetPropertyModificationsForRevisionIdAsync(revisionId);

            //assert
            Assert.AreEqual(result.Count, repositoryResult.Count);
            var firstResult = result.First();
            var matchFound = repositoryResult.Any(
                r => firstResult.ArtifactId == r.ArtifactId && firstResult.EndRevision == r.EndRevision && firstResult.ItemId == r.ItemId && firstResult.NewPropertyValue == r.NewPropertyValue &&
                     firstResult.ProjectId == r.ProjectId && firstResult.PropertyName == r.PropertyName && firstResult.StartRevision == r.StartRevision && firstResult.Type == r.Type &&
                     firstResult.TypeId == r.TypeId && firstResult.VersionId == r.VersionId);
            Assert.IsTrue(matchFound);
        }

        [TestMethod]
        public async Task GetWorkflowPropertyTransitionsForArtifactsAsync_CompletesSuccessfuly()
        {
            //arrange
            var connectionMock = new SqlConnectionWrapperMock();
            const string storedProcedure = "GetWorkflowTriggersForArtifacts";
            const int userId = 1;
            const int revisionId = 2;
            const int eventType = 1;
            var itemIds = new[] {1, 2, 3};
            var parameters = new Dictionary<string, object>
            {
                {nameof(userId), userId},
                {nameof(revisionId), revisionId},
                {nameof(eventType), eventType},
                {nameof(itemIds), SqlConnectionWrapper.ToDataTable(itemIds)}
            };
            var result = new List<SqlWorkflowEvent>();
            for (int i = 0; i < 5; i++)
            {
                result.Add(new SqlWorkflowEvent {Triggers = $"Triggers{i}"});
            }
            connectionMock.SetupQueryAsync(storedProcedure, parameters, result);

            //act
            var repository = new ArtifactsPublishedRepository(connectionMock.Object);
            var repositoryResult = await repository.GetWorkflowPropertyTransitionsForArtifactsAsync(userId, revisionId, eventType, itemIds);

            //assert
            Assert.AreEqual(result.Count, repositoryResult.Count);
        }

        [TestMethod]
        public async Task GetWorkflowStatesForArtifactsAsync_CompletesSuccessfuly()
        {
            //arrange
            var connectionMock = new SqlConnectionWrapperMock();
            const string storedProcedure = "GetWorkflowStatesForArtifacts";
            const int userId = 1;
            var artifactIds = new[] {1, 2, 3};
            const int revisionId = 2;
            const bool addDrafts = true;
            var parameters = new Dictionary<string, object>
            {
                {nameof(userId), userId},
                {nameof(artifactIds), SqlConnectionWrapper.ToDataTable(artifactIds)},
                {nameof(revisionId), revisionId},
                {nameof(addDrafts), addDrafts}
            };
            var result = new List<SqlWorkFlowStateInformation>();
            for (int i = 0; i < 5; i++)
            {
                result.Add(new SqlWorkFlowStateInformation {Name = $"Name{i}"});
            }
            connectionMock.SetupQueryAsync(storedProcedure, parameters, result);

            //act
            var repository = new ArtifactsPublishedRepository(connectionMock.Object);
            var repositoryResult = await repository.GetWorkflowStatesForArtifactsAsync(userId, artifactIds, revisionId, addDrafts);

            //assert
            Assert.AreEqual(result.Count, repositoryResult.Count);
        }

        [TestMethod]
        public async Task GetInstancePropertyTypeIdsMap_CompletesSuccessfuly()
        {
            //arrange
            var connectionMock = new SqlConnectionWrapperMock();
            const string storedProcedure = "[dbo].[GetInstancePropertyTypeIdsFromCustomIds]";
            var customPropertyTypeIds = new[] {1, 2, 3};
            var parameters = new Dictionary<string, object> {{nameof(customPropertyTypeIds), SqlConnectionWrapper.ToDataTable(customPropertyTypeIds)}};
            var result = new List<SqlCustomToInstancePropertyTypeIds>();
            for (int i = 0; i < 5; i++)
            {
                result.Add(new SqlCustomToInstancePropertyTypeIds {InstancePropertyTypeId = i, PropertyTypeId = i});
            }
            connectionMock.SetupQueryAsync(storedProcedure, parameters, result);

            //act
            var repository = new ArtifactsPublishedRepository(connectionMock.Object);
            var repositoryResult = await repository.GetInstancePropertyTypeIdsMap(customPropertyTypeIds);

            //assert
            Assert.AreEqual(result.Count, repositoryResult.Count);
        }

        [TestMethod]
        public async Task GetProjectNameByIdsAsync_CompletesSuccessfuly()
        {
            //arrange
            var connectionMock = new SqlConnectionWrapperMock();
            const string storedProcedure = "GetProjectNameByIds";
            var projectIds = new[] {1, 2, 3};
            var parameters = new Dictionary<string, object> {{nameof(projectIds), SqlConnectionWrapper.ToDataTable(projectIds)}};
            var result = new List<SqlProject>();
            for (int i = 0; i < 5; i++)
            {
                result.Add(new SqlProject {ItemId = i, Name = $"Name{i}"});
            }
            connectionMock.SetupQueryAsync(storedProcedure, parameters, result);

            //act
            var repository = new ArtifactsPublishedRepository(connectionMock.Object);
            var repositoryResult = await repository.GetProjectNameByIdsAsync(projectIds);

            //assert
            Assert.AreEqual(result.Count, repositoryResult.Count);
        }
    }
}
