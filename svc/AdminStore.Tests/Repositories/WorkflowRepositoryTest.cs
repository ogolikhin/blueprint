using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AdminStore.Models;
using AdminStore.Models.DTO;
using AdminStore.Models.Workflow;
using AdminStore.Repositories.Workflow;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using ServiceLibrary.Exceptions;
using ServiceLibrary.Helpers;
using ServiceLibrary.Models;
using ServiceLibrary.Repositories;

namespace AdminStore.Repositories
{
    [TestClass]
    public class WorkflowRepositoryTest
    {
        [TestInitialize]
        public void Initialize()
        {
            _sqlConnectionWrapperMock = new SqlConnectionWrapperMock();
            _sqlHelperMock = new Mock<ISqlHelper>();
            _workflowRepository = new WorkflowRepository(_sqlConnectionWrapperMock.Object, _sqlHelperMock.Object);
            _workflowAssignScope = new WorkflowAssignScope() { AllArtifacts = true, AllProjects = true, ArtifactIds = new List<int>() { 145, 148 }, ProjectIds = new List<int>() { 1, 4 } };
            _projectsUnassignedScope = new OperationScope() { Ids = new List<int>() { 1, 2 }, SelectAll = false };

            _listArtifactTypesIds = new List<int> { 1, 2, 3 };
            _workflowId = 1;
            _pagination = new Pagination() { Limit = int.MaxValue, Offset = 0 };

            _scope = new OperationScope() { Ids = _listArtifactTypesIds, SelectAll = false };
            _copyWorkfloDto = new CopyWorkfloDto() { Name = "TestWorkflow" };

            _workflowStateIds = new List<int> { 1, 2, 3 };
            _workflowEventIds = new List<int> { 1, 2, 3 };
        }

        private SqlConnectionWrapperMock _sqlConnectionWrapperMock;
        private WorkflowRepository _workflowRepository;
        private Mock<ISqlHelper> _sqlHelperMock;
        private WorkflowAssignScope _workflowAssignScope;
        private OperationScope _projectsUnassignedScope;
        private int _workflowId;
        private const int _projectId = 1;
        private Pagination _pagination;
        private List<int> _listArtifactTypesIds;
        private IEnumerable<SyncResult> _outputSyncResult = new List<SyncResult>() { new SyncResult { TotalAdded = 2, TotalDeleted = 1 } };
        private string _projectSearch = "test";
        private OperationScope _scope;
        private int _userId = 1;
        private CopyWorkfloDto _copyWorkfloDto;
        private IEnumerable<int> _workflowStateIds;
        private IEnumerable<int> _workflowEventIds;
        private const int PublishRevision = 12;
        private const int IncorrectPublishRevision = 0;

        #region AssignProjectsAndArtifactTypesToWorkflow
        [TestMethod]
        public async Task AssignProjectsAndArtifactTypesToWorkflow_ExistsAssignedProjects_QueryReturnCountAssignedProjects()
        {
            // arrange
            _sqlConnectionWrapperMock.SetupExecuteScalarAsync("AssignProjectsAndArtifactTypesToWorkflow", It.IsAny<Dictionary<string, object>>(), 2, new Dictionary<string, object> { { "AllProjectsAssignedToWorkflow", true } });

            // act
            var result = await _workflowRepository.AssignProjectsAndArtifactTypesToWorkflow(_workflowId, _workflowAssignScope);

            // assert
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public async Task AssignProjectsAndArtifactTypesToWorkflow_WorkflowByWorkflowIdIsActive_ReturnConflictExceptionE()
        {
            // arrange
            Exception exception = null;

            _sqlConnectionWrapperMock.SetupExecuteScalarAsync("AssignProjectsAndArtifactTypesToWorkflow", It.IsAny<Dictionary<string, object>>(), 2, new Dictionary<string, object> { { "ErrorCode", (int)SqlErrorCodes.WorkflowWithCurrentIdIsActive } });
            // act
            try
            {
                await _workflowRepository.AssignProjectsAndArtifactTypesToWorkflow(_workflowId, _workflowAssignScope);
            }
            catch (Exception ex)
            {
                exception = ex;
            }
            // assert
            Assert.IsNotNull(exception);
            Assert.IsInstanceOfType(exception, typeof(ConflictException));
        }

        [TestMethod]
        public async Task AssignProjectsAndArtifactTypesToWorkflow_WorkflowByWorkflowIdIsNotFound_ReturnResourceNotFoundExceptionE()
        {
            // arrange
            Exception exception = null;

            _sqlConnectionWrapperMock.SetupExecuteScalarAsync("AssignProjectsAndArtifactTypesToWorkflow", It.IsAny<Dictionary<string, object>>(), 2, new Dictionary<string, object> { { "ErrorCode", (int)SqlErrorCodes.WorkflowWithCurrentIdNotExist } });
            // act
            try
            {
                await _workflowRepository.AssignProjectsAndArtifactTypesToWorkflow(_workflowId, _workflowAssignScope);
            }
            catch (Exception ex)
            {
                exception = ex;
            }
            // assert
            Assert.IsNotNull(exception);
            Assert.IsInstanceOfType(exception, typeof(ResourceNotFoundException));
        }

        [TestMethod]
        public async Task AssignProjectsAndArtifactTypesToWorkflow_GeneralSQLException_ReturnException()
        {
            // arrange
            Exception exception = null;

            _sqlConnectionWrapperMock.SetupExecuteScalarAsync("AssignProjectsAndArtifactTypesToWorkflow", It.IsAny<Dictionary<string, object>>(), 2, new Dictionary<string, object> { { "ErrorCode", (int)SqlErrorCodes.GeneralSqlError } });
            // act
            try
            {
                await _workflowRepository.AssignProjectsAndArtifactTypesToWorkflow(_workflowId, _workflowAssignScope);
            }
            catch (Exception ex)
            {
                exception = ex;
            }
            // assert
            Assert.IsNotNull(exception);
            Assert.IsInstanceOfType(exception, typeof(Exception));
        }
        #endregion

        #region AssignArtifactTypesToProjectInWorkflow
        [TestMethod]
        public async Task AssignArtifactTypesToProjectInWorkflow_ExistsAssignedProjects_QueryReturnCountAddedAndDeletedProjects()
        {
            // arrange
            _sqlConnectionWrapperMock.SetupQueryAsync("AssignArtifactTypesToProjectInWorkflow", It.IsAny<Dictionary<string, object>>(), _outputSyncResult);

            // act
            var result = await _workflowRepository.AssignArtifactTypesToProjectInWorkflow(_workflowId, _projectId, _scope);

            // assert
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public async Task AssignArtifactTypesToProjectInWorkflow_WorkflowWithCurrentIdIsActive_ReturnConflictException()
        {
            // arrange
            Exception exception = null;

            _sqlConnectionWrapperMock.SetupQueryAsync("AssignArtifactTypesToProjectInWorkflow", It.IsAny<Dictionary<string, object>>(), _outputSyncResult, new Dictionary<string, object> { { "ErrorCode", (int)SqlErrorCodes.WorkflowWithCurrentIdIsActive } });
            // act
            try
            {
                await _workflowRepository.AssignArtifactTypesToProjectInWorkflow(_workflowId, _projectId, _scope);
            }
            catch (ConflictException ex)
            {
                exception = ex;
            }
            // assert
            Assert.IsNotNull(exception);
            Assert.IsInstanceOfType(exception, typeof(ConflictException));
        }

        [TestMethod]
        public async Task AssignArtifactTypesToProjectInWorkflow_WorkflowWithCurrentIdNotExist_ReturnResourceNotFoundException()
        {
            // arrange
            Exception exception = null;

            _sqlConnectionWrapperMock.SetupQueryAsync("AssignArtifactTypesToProjectInWorkflow", It.IsAny<Dictionary<string, object>>(), _outputSyncResult, new Dictionary<string, object> { { "ErrorCode", (int)SqlErrorCodes.WorkflowWithCurrentIdNotExist } });
            // act
            try
            {
                await _workflowRepository.AssignArtifactTypesToProjectInWorkflow(_workflowId, _projectId, _scope);
            }
            catch (ResourceNotFoundException ex)
            {
                exception = ex;
            }
            // assert
            Assert.IsNotNull(exception);
            Assert.IsInstanceOfType(exception, typeof(ResourceNotFoundException));
        }

        [TestMethod]
        public async Task AssignArtifactTypesToProjectInWorkflow_GeneralSQLException_ReturnException()
        {
            // arrange
            Exception exception = null;

            _sqlConnectionWrapperMock.SetupQueryAsync("AssignArtifactTypesToProjectInWorkflow", It.IsAny<Dictionary<string, object>>(), _outputSyncResult, new Dictionary<string, object> { { "ErrorCode", (int)SqlErrorCodes.GeneralSqlError } });
            // act
            try
            {
                await _workflowRepository.AssignArtifactTypesToProjectInWorkflow(_workflowId, _projectId, _scope);
            }
            catch (Exception ex)
            {
                exception = ex;
            }
            // assert
            Assert.IsNotNull(exception);
            Assert.IsInstanceOfType(exception, typeof(Exception));
        }

        [TestMethod]
        public async Task AssignArtifactTypesToProjectInWorkflow_ProjectOfWorkflowDoesNotHaveArtifactTypes_ReturnConflictException()
        {
            // arrange
            Exception exception = null;

            _sqlConnectionWrapperMock.SetupQueryAsync("AssignArtifactTypesToProjectInWorkflow", It.IsAny<Dictionary<string, object>>(), _outputSyncResult, new Dictionary<string, object> { { "ErrorCode", (int)SqlErrorCodes.WorkflowProjectHasNoArtifactTypes } });
            // act
            try
            {
                await _workflowRepository.AssignArtifactTypesToProjectInWorkflow(_workflowId, _projectId, _scope);
            }
            catch (Exception ex)
            {
                exception = ex;
            }
            // assert
            Assert.IsNotNull(exception);
            Assert.IsInstanceOfType(exception, typeof(ConflictException));
        }

        [TestMethod]
        public async Task AssignArtifactTypesToProjectInWorkflow_ProjectOfWorkflowDoesNotHaveLiveArtifactTypes_ReturnConflictException()
        {
            // arrange
            Exception exception = null;

            _sqlConnectionWrapperMock.SetupQueryAsync("AssignArtifactTypesToProjectInWorkflow", It.IsAny<Dictionary<string, object>>(), _outputSyncResult, new Dictionary<string, object> { { "ErrorCode", (int)SqlErrorCodes.WorkflowProjectHasNoLiveArtifactTypes } });
            // act
            try
            {
                await _workflowRepository.AssignArtifactTypesToProjectInWorkflow(_workflowId, _projectId, _scope);
            }
            catch (Exception ex)
            {
                exception = ex;
            }
            // assert
            Assert.IsNotNull(exception);
            Assert.IsInstanceOfType(exception, typeof(ConflictException));
        }

        [TestMethod]
        public async Task AssignArtifactTypesToProjectInWorkflow_ProjectWithCurrentIdNotExist_ReturnResourceNotFoundException()
        {
            // arrange
            Exception exception = null;

            _sqlConnectionWrapperMock.SetupQueryAsync("AssignArtifactTypesToProjectInWorkflow", It.IsAny<Dictionary<string, object>>(), _outputSyncResult, new Dictionary<string, object> { { "ErrorCode", (int)SqlErrorCodes.ProjectWithCurrentIdNotExist } });
            // act
            try
            {
                await _workflowRepository.AssignArtifactTypesToProjectInWorkflow(_workflowId, _projectId, _scope);
            }
            catch (ResourceNotFoundException ex)
            {
                exception = ex;
            }
            // assert
            Assert.IsNotNull(exception);
            Assert.IsInstanceOfType(exception, typeof(ResourceNotFoundException));
        }
        #endregion

        #region GetWorkflowDetailsAsync

        [TestMethod]
        public async Task GetWorkflowDetailsAsync_WeHaveThisWorkflowInDb_QueryReturnWorkflow()
        {
            // arrange
            var cxn = new SqlConnectionWrapperMock();
            var sqlHelperMock = new Mock<ISqlHelper>();

            var repository = new WorkflowRepository(cxn.Object, sqlHelperMock.Object);

            var workflowId = 10;
            var workflow = new SqlWorkflow { Name = "Workflow1", Description = "Workflow1Description" };
            var workflowsList = new List<SqlWorkflow> { workflow };
            cxn.SetupQueryAsync("GetWorkflowDetails", new Dictionary<string, object> { { "WorkflowId", workflowId } }, workflowsList);

            // act
            var workflowDetails = await repository.GetWorkflowDetailsAsync(workflowId);

            // assert
            Assert.IsNotNull(workflowDetails);
        }

        #endregion

        #region GetWorkflowArtifactTypesAndProjectsAsync

        [TestMethod]
        public async Task GetWorkflowArtifactTypesAndProjectsAsync_ThereExistWorkflowArtifactTypesAndProjects_QueryReturnWorkflowArtifactTypesAndProjects()
        {
            // arrange
            var cxn = new SqlConnectionWrapperMock();
            var sqlHelperMock = new Mock<ISqlHelper>();

            var repository = new WorkflowRepository(cxn.Object, sqlHelperMock.Object);
            var workflowId = 10;
            var workflowArtifactTypesAndProjects = new List<SqlWorkflowArtifactTypes>
            {
                new SqlWorkflowArtifactTypes
                {
                    ProjectId = 1,
                    ProjectPath = "Project1",
                    ArtifactTypeName = "Artifact1",
                    ArtifactTypeId = 205
                },
                new SqlWorkflowArtifactTypes
                {
                    ProjectId = 1,
                    ProjectPath = "Project1",
                    ArtifactTypeName = "Artifact2",
                    ArtifactTypeId = 206
                },
                new SqlWorkflowArtifactTypes
                {
                    ProjectId = 2,
                    ProjectPath = "Project1",
                    ArtifactTypeName = "Artifact2",
                    ArtifactTypeId = 206
                }
            };

            cxn.SetupQueryAsync("GetWorkflowArtifactTypesAsync", It.IsAny<Dictionary<string, object>>(), workflowArtifactTypesAndProjects);

            // act
            var workflowDetails = await repository.GetWorkflowArtifactTypesAsync(workflowId);

            // assert
            Assert.IsNotNull(workflowDetails);
        }

        #endregion

        #region UpdateWorkflowsAsync

        [TestMethod]
        public async Task UpdateWorkflowsAsync_UpdateThisWorkflowInDb_QueryReturnWorkflows()
        {
            // arrange
            var cxn = new SqlConnectionWrapperMock();
            var sqlHelperMock = new Mock<ISqlHelper>();

            var repository = new WorkflowRepository(cxn.Object, sqlHelperMock.Object);
            var publishRevision = 12;
            var workflow = new SqlWorkflow { Name = "Workflow1", Description = "Workflow1Description", Active = true };
            var workflowsList = new List<SqlWorkflow> { workflow };
            cxn.SetupQueryAsync("UpdateWorkflows", It.IsAny<Dictionary<string, object>>(), workflowsList);

            // act
            var updatedWorkflows = await repository.UpdateWorkflowsAsync(workflowsList, publishRevision);

            // assert
            Assert.IsNotNull(updatedWorkflows);
        }

        #endregion

        #region GetWorkflowTransitionsAndPropertyChangesByWorkflowId

        [TestMethod]
        public async Task GetWorkflowTransitionsAndPropertyChangesByWorkflowId_ThereExistWorkflowTransitionsAndPropertyChanges_QueryReturnWorkflowTransitionsAndPropertyChanges()
        {
            // arrange
            var cxn = new SqlConnectionWrapperMock();
            var sqlHelperMock = new Mock<ISqlHelper>();

            var repository = new WorkflowRepository(cxn.Object, sqlHelperMock.Object);
            var workflowId = 10;
            var workflowTransitionsAndPropertyChanges = new List<SqlWorkflowEventData>
            {
                new SqlWorkflowEventData
                {
                    WorkflowId = 10,
                    Name = "FirsTrigger",
                    FromState = "new",
                    ToState = "Active",
                    Permissions = "<P S=\"0\"><G>1</G></P>",
                    Type = 1
                },
                new SqlWorkflowEventData
                {
                    WorkflowId = 10,
                    Name = "second Trigger",
                    FromState = "Active",
                    Permissions = "<P S=\"0\"/>",
                    Type = 2
                }
            };

            cxn.SetupQueryAsync("GetWorkflowStatesById", It.IsAny<Dictionary<string, object>>(), workflowTransitionsAndPropertyChanges);

            // act
            var workflowDetails = await repository.GetWorkflowEventsAsync(workflowId);

            // assert
            Assert.IsNotNull(workflowDetails);
        }

        #endregion

        #region GetWorkflowStatesByWorkflowId

        [TestMethod]
        public async Task GetWorkflowStatesByWorkflowId_WeHaveThisWorkflowInDb_QueryReturnWorkflowStates()
        {
            // arrange
            var cxn = new SqlConnectionWrapperMock();
            var sqlHelperMock = new Mock<ISqlHelper>();

            var repository = new WorkflowRepository(cxn.Object, sqlHelperMock.Object);

            var workflowId = 10;
            var workflow = new SqlState { Name = "Workflow1", Default = true };
            var workflowsList = new List<SqlState> { workflow };
            cxn.SetupQueryAsync("GetWorkflowStatesById", new Dictionary<string, object> { { "WorkflowId", workflowId } }, workflowsList);

            // act
            var workflowStates = await repository.GetWorkflowStatesAsync(workflowId);

            // assert
            Assert.IsNotNull(workflowStates);
        }

        #endregion

        #region GetWorkflowAvailableProjectsAsync
        [TestMethod]
         public async Task GetWorkflowAvailableProjectsAsync_ExistsAvailableProjects_QueryReturnAvailableProjects()
         {
            // arrange
            int workflowId = 1;
            int folderId = 2;

            var listAvailableProjects = new List<InstanceItem>
             {
                 new InstanceItem
                 {
                     Id = 4,
                     ParentFolderId = 2,
                     Name = "Project11",
                     Type = (InstanceItemTypeEnum)1
                 },
                 new InstanceItem
                 {
                     Id = 7,
                     ParentFolderId = 2,
                     Name = "Project12",
                     Type = (InstanceItemTypeEnum)1
                 }
             };

            _sqlConnectionWrapperMock.SetupQueryAsync("GetWorkflowAvailableProjects", It.IsAny<Dictionary<string, object>>(), listAvailableProjects);

             // act
             var result = await _workflowRepository.GetWorkflowAvailableProjectsAsync(workflowId, folderId);

             // assert
             Assert.IsNotNull(result);
         }

         [TestMethod]
         public async Task GetWorkflowAvailableProjects_InvalidWorkflowId_ReturnArgumentOutOfRangeException()
         {
            // arrange
            int workflowId = 0;
            int folderId = 2;
            Exception exception = null;

            // act
            try
             {
                 await _workflowRepository.GetWorkflowAvailableProjectsAsync(workflowId, folderId);
             }
             catch (Exception ex)
             {
                exception = ex;
             }

             // assert
             Assert.IsNotNull(exception);
             Assert.IsInstanceOfType(exception, typeof(ArgumentOutOfRangeException));
         }

         [TestMethod]
         public async Task GetWorkflowAvailableProjects_InvalidFolderId_ReturnArgumentOutOfRangeException()
         {
            // arrange
            int workflowId = 1;
            int folderId = 0;
            Exception exception = null;

            // act
            try
             {
                 await _workflowRepository.GetWorkflowAvailableProjectsAsync(workflowId, folderId);
             }
             catch (Exception ex)
             {
                 exception = ex;
             }

             // assert
             Assert.IsNotNull(exception);
             Assert.IsInstanceOfType(exception, typeof(ArgumentOutOfRangeException));
         }

         [TestMethod]
         public async Task GetWorkflowAvailableProjects_NotExistFolderByFolderId_ReturnResourceNotFoundException()
         {
            // arrange
            int workflowId = 1;
            int folderId = 99999;
            Exception exception = null;

            _sqlConnectionWrapperMock.SetupQueryAsync("GetWorkflowAvailableProjects", It.IsAny<Dictionary<string, object>>(), new List<InstanceItem>(), new Dictionary<string, object> { { "ErrorCode", (int)SqlErrorCodes.FolderWithCurrentIdNotExist } });

             // act
             try
             {
                 await _workflowRepository.GetWorkflowAvailableProjectsAsync(workflowId, folderId);
             }
             catch (Exception ex)
             {
                exception = ex;
             }

             // assert
             Assert.IsNotNull(exception);
             Assert.IsInstanceOfType(exception, typeof(ResourceNotFoundException));
         }

         [TestMethod]
         public async Task GetWorkflowAvailableProjects_NotExistWorkflowByWorkflowId_ReturnResourceNotFoundException()
         {
            // arrange
            int workflowId = 99999;
            int folderId = 1;
            Exception exception = null;

            _sqlConnectionWrapperMock.SetupQueryAsync("GetWorkflowAvailableProjects", It.IsAny<Dictionary<string, object>>(), new List<InstanceItem>(), new Dictionary<string, object> { { "ErrorCode", (int)SqlErrorCodes.WorkflowWithCurrentIdNotExist } });

             // act
             try
             {
                 await _workflowRepository.GetWorkflowAvailableProjectsAsync(workflowId, folderId);
             }
             catch (Exception ex)
             {
                 exception = ex;
             }
             // assert
             Assert.IsNotNull(exception);
             Assert.IsInstanceOfType(exception, typeof(ResourceNotFoundException));
         }
        #endregion

        #region UnassignProjectsAndArtifactTypesFromWorkflowAsync

        [TestMethod]
        public async Task UnassignProjectsAndArtifactTypesFromWorkflowAsync_ProjectsUnassigned_SuccessfulResult()
        {
            // Arrange
            var unassignedCount = 2;

            _sqlConnectionWrapperMock.SetupExecuteScalarAsync("UnassignProjectsAndArtifactTypesFromWorkflow", It.IsAny<Dictionary<string, object>>(), unassignedCount);

            // act
            var result = await _workflowRepository.UnassignProjectsAndArtifactTypesFromWorkflowAsync(_workflowId, _projectsUnassignedScope);

            // assert
            Assert.IsNotNull(result);
            Assert.AreEqual(unassignedCount, result);
        }

        [TestMethod]
        [ExpectedException(typeof(ResourceNotFoundException))]
        public async Task UnassignProjectsAndArtifactTypesFromWorkflowAsync_WorkflowIdNotExist_ReturnResourceNotFoundException()
        {
            // Arrange
            int errorCode = 50024;
            int total = 0;

            _sqlConnectionWrapperMock.SetupExecuteScalarAsync("UnassignProjectsAndArtifactTypesFromWorkflow",
                                                        It.IsAny<Dictionary<string, object>>(),
                                                        total,
                                                        new Dictionary<string, object>
                                                        {
                                                            { "ErrorCode", errorCode }
                                                        });
            // Act
            await
                _workflowRepository.UnassignProjectsAndArtifactTypesFromWorkflowAsync(_workflowId, _projectsUnassignedScope);

            // Assert
        }

        [TestMethod]
        [ExpectedException(typeof(ConflictException))]
        public async Task UnassignProjectsAndArtifactTypesFromWorkflowAsync_WorkflowIsActive_ReturnConflictException()
        {
            // Arrange
            int errorCode = 50025;
            int total = 0;

            _sqlConnectionWrapperMock.SetupExecuteScalarAsync("UnassignProjectsAndArtifactTypesFromWorkflow",
                                                        It.IsAny<Dictionary<string, object>>(),
                                                        total,
                                                        new Dictionary<string, object>
                                                        {
                                                            { "ErrorCode", errorCode }
                                                        });
            // Act
            await
                _workflowRepository.UnassignProjectsAndArtifactTypesFromWorkflowAsync(_workflowId, _projectsUnassignedScope);

            // Assert
        }

        [TestMethod]
        [ExpectedException(typeof(Exception))]
        public async Task UnassignProjectsAndArtifactTypesFromWorkflowAsync_WorkflowIsActive_ReturnException()
        {
            // Arrange
            int errorCode = 50000;
            int total = 0;

            _sqlConnectionWrapperMock.SetupExecuteScalarAsync("UnassignProjectsAndArtifactTypesFromWorkflow",
                                                        It.IsAny<Dictionary<string, object>>(),
                                                        total,
                                                        new Dictionary<string, object>
                                                        {
                                                            { "ErrorCode", errorCode }
                                                        });
            // Act
            await
                _workflowRepository.UnassignProjectsAndArtifactTypesFromWorkflowAsync(_workflowId, _projectsUnassignedScope);

            // Assert

        }

        #endregion

        #region GetProjectArtifactTypesAssignedToWorkflowAsync

        [TestMethod]
        public async Task GetProjectArtifactTypesAssignedtoWorkflowAsync_ArtifactsAreFound_SuccessfulResult()
        {
            // Arrange
            int errorCode = 0;
            int total = 1;

            var spResult = new List<WorkflowProjectArtifactType>
            {
                new WorkflowProjectArtifactType()
                {
                    ProjectId = 1,
                    ProjectName = "TestProject",
                    ArtifactId = 1,
                    ArtifactName = "TestArtifact"
                }
            };

            _sqlConnectionWrapperMock.SetupQueryAsync("GetWorkflowProjectsArtifactTypes",
                                                        It.IsAny<Dictionary<string, object>>(),
                                                        spResult,
                                                        new Dictionary<string, object>
                                                        {
                                                            { "ErrorCode", errorCode },
                                                            { "Total", total }
                                                        });
            // Act
            var artifacts = await
                _workflowRepository.GetProjectArtifactTypesAssignedtoWorkflowAsync(_workflowId, _pagination);

            // Assert
            Assert.IsNotNull(artifacts);
            Assert.AreEqual(total, artifacts.Total);
        }

        [TestMethod]
        [ExpectedException(typeof(ResourceNotFoundException))]
        public async Task GetProjectArtifactTypesAssignedtoWorkflowAsync_InvalidWorkflowId_ReturnResourceNotFoundException()
        {
            // Arrange
            int errorCode = 50024;
            _workflowId = 1000;
            int total = 0;

            _sqlConnectionWrapperMock.SetupQueryAsync("GetWorkflowProjectsArtifactTypes",
                                                        It.IsAny<Dictionary<string, object>>(),
                                                        new List<WorkflowProjectArtifactType>(),
                                                        new Dictionary<string, object>
                                                        {
                                                            { "ErrorCode", errorCode },
                                                            { "Total", total }
                                                        });
            // Act
            await
                _workflowRepository.GetProjectArtifactTypesAssignedtoWorkflowAsync(_workflowId, _pagination);

            // Assert
            _sqlConnectionWrapperMock.Verify();

        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public async Task GetProjectArtifactTypesAssignedtoWorkflowAsync_WorkflowIdNotValid_ThrowsArgumentOutOfRangeException()
        {
            // Arrange
            _workflowId = 0;

            // Act
            await
                _workflowRepository.GetProjectArtifactTypesAssignedtoWorkflowAsync(_workflowId, _pagination);

            // Assert
            _sqlConnectionWrapperMock.Verify();
        }

        #endregion

        #region SearchProjectsByName

        [TestMethod]
        public async Task SearchProjectsByName_AllParamsAreCorrect_ReturnProjects()
        {
            // Arrange
            int errorCode = 0;

            var expectedProjects = new List<WorkflowProjectSearch>()
            {
                new WorkflowProjectSearch()
                {
                    ItemId = 1,
                    Name = "Project1",
                    Path = "Path1"
                },
                new WorkflowProjectSearch()
                {
                    ItemId = 2,
                    Name = "Project2",
                    Path = "Path2"
                }
            };

            _sqlConnectionWrapperMock.SetupQueryAsync("SearchProjectsByName",
                                                        It.IsAny<Dictionary<string, object>>(),
                                                        expectedProjects,
                                                        new Dictionary<string, object>
                                                        {
                                                            { "ErrorCode", errorCode }
                                                        });
            // Act
            var actualProjects = await
                _workflowRepository.SearchProjectsByName(_workflowId, _projectSearch);

            // Assert
            Assert.IsNotNull(actualProjects);
            Assert.AreEqual(expectedProjects.Count, actualProjects.Count());
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public async Task SearchProjectsByName_InvalidWorkflowId_ThrowsArgumentOutOfRangeException()
        {
            // Arrange
            _workflowId = 0;

            // Act
            await
                _workflowRepository.SearchProjectsByName(_workflowId, _projectSearch);

            // Assert
        }

        [TestMethod]
        [ExpectedException(typeof(ResourceNotFoundException))]
        public async Task SearchProjectsByName_InvalidWorkflowId_ReturnResourceNotFoundException()
        {
            // Arrange
            int errorCode = 50024;
            _workflowId = 1000;

            _sqlConnectionWrapperMock.SetupQueryAsync("SearchProjectsByName",
                                                        It.IsAny<Dictionary<string, object>>(),
                                                        new List<WorkflowProjectSearch>(),
                                                        new Dictionary<string, object>
                                                        {
                                                            { "ErrorCode", errorCode }
                                                        });
            // Act
            await _workflowRepository.SearchProjectsByName(_workflowId, _projectSearch);

            // Assert
        }

        #endregion

        #region CopyWorkflowAsync

        [TestMethod]
        public async Task CopyWorkflowAsync_AllParamsAreCorrect_UpdateWorkflowSuccessfuly()
        {
            // Arrange
            int errorCode = 0;
            var updatedWorkflowId = 2;
            _sqlConnectionWrapperMock.SetupExecuteScalarAsync("CopyWorkflow",
                                                        It.IsAny<Dictionary<string, object>>(),
                                                        updatedWorkflowId,
                                                        new Dictionary<string, object>
                                                        {
                                                            { "ErrorCode", errorCode }
                                                        });
            // Act
            var copyWorkflowResult = await
                _workflowRepository.CopyWorkflowAsync(_workflowId, _userId, _copyWorkfloDto);

            // Assert
            Assert.IsNotNull(copyWorkflowResult);
            Assert.AreEqual(updatedWorkflowId, copyWorkflowResult);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public async Task CopyWorkflowAsync_WorkflowIdNotValid_ArgumentOutOfRangeException()
        {
            // Arrange
            _workflowId = 0;

            // Act
            await
                _workflowRepository.CopyWorkflowAsync(_workflowId, _userId, _copyWorkfloDto);

            // Assert

        }
        [TestMethod]
        [ExpectedException(typeof(ResourceNotFoundException))]
        public async Task CopyWorkflowAsync_WorkflowIdNotExists_ResourceNotFoundException()
        {
            // Arrange
            int errorCode = 50024;
            var updatedWorkflowId = 0;
            _workflowId = 10000;
            _sqlConnectionWrapperMock.SetupExecuteScalarAsync("CopyWorkflow",
                                                        It.IsAny<Dictionary<string, object>>(),
                                                        updatedWorkflowId,
                                                        new Dictionary<string, object>
                                                        {
                                                            { "ErrorCode", errorCode }
                                                        });
            // Act
            await
                _workflowRepository.CopyWorkflowAsync(_workflowId, _userId, _copyWorkfloDto);

            // Assert
        }

        [TestMethod]
        [ExpectedException(typeof(ConflictException))]
        public async Task CopyWorkflowAsync_WorkflowWithSpecifiedNameAlreadyExists_ConflictException()
        {
            // Arrange
            int errorCode = 50023;
            var updatedWorkflowId = 0;
            _workflowId = 10000;
            _sqlConnectionWrapperMock.SetupExecuteScalarAsync("CopyWorkflow",
                                                        It.IsAny<Dictionary<string, object>>(),
                                                        updatedWorkflowId,
                                                        new Dictionary<string, object>
                                                        {
                                                            { "ErrorCode", errorCode }
                                                        });
            // Act
            await
                _workflowRepository.CopyWorkflowAsync(_workflowId, _userId, _copyWorkfloDto);

            // Assert
        }

        [TestMethod]
        [ExpectedException(typeof(Exception))]
        public async Task CopyWorkflowAsync_GeneralError_Exception()
        {
            // Arrange
            int errorCode = 50000;
            var updatedWorkflowId = 0;
            _sqlConnectionWrapperMock.SetupExecuteScalarAsync("CopyWorkflow",
                                                        It.IsAny<Dictionary<string, object>>(),
                                                        updatedWorkflowId,
                                                        new Dictionary<string, object>
                                                        {
                                                            { "ErrorCode", errorCode }
                                                        });
            // Act
            await
                _workflowRepository.CopyWorkflowAsync(_workflowId, _userId, _copyWorkfloDto);

            // Assert
        }

        #endregion

        #region DeleteWorkflowStatesAsync

        [TestMethod]
        public async Task DeleteWorkflowStatesAsync_DeleteWorkflowStatesInDb_QueryReturnWorkflowStateIds()
        {
            // arrange
            var resultSqlStates = new List<SqlState>(3);
            resultSqlStates.AddRange(_workflowStateIds.Select(
                workflowStateId => new SqlState
                {
                    WorkflowStateId = workflowStateId
                }));
            _sqlConnectionWrapperMock.SetupQueryAsync("DeleteWorkflowStates", It.IsAny<Dictionary<string, object>>(), resultSqlStates);

            // act
            var deletedWorkflowStates = await _workflowRepository.DeleteWorkflowStatesAsync(_workflowStateIds, PublishRevision);

            // assert
            Assert.IsNotNull(deletedWorkflowStates);
            var deletedWorkflowStatesList = deletedWorkflowStates.ToList();
            foreach (var workflowStateId in _workflowStateIds)
            {
                Assert.IsTrue(deletedWorkflowStatesList.Exists(x => x == workflowStateId));
            }
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task DeleteWorkflowStatesAsync_EmptyWorkflowStateIdsList_ArgumentException()
        {
            // arrange

            // act
            await _workflowRepository.DeleteWorkflowStatesAsync(new List<int>(), PublishRevision);

            // assert
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task DeleteWorkflowStatesAsync_InvalidPublishRevision_ArgumentException()
        {
            // arrange

            // act
            await _workflowRepository.DeleteWorkflowStatesAsync(_workflowStateIds, IncorrectPublishRevision);

            // assert
        }

        #endregion

        #region CreateWorkflowStatesAsync

        [TestMethod]
        public async Task CreateWorkflowStatesAsync_CreateWorkflowStatesInDb_QueryReturnWorkflowStates()
        {
            // arrange
            var workflowStates = new List<SqlState>(3);
            workflowStates.AddRange(_workflowStateIds.Select(
                workflowStateId => new SqlState
                {
                    WorkflowStateId = workflowStateId
                }));
            _sqlConnectionWrapperMock.SetupQueryAsync("CreateWorkflowStates", It.IsAny<Dictionary<string, object>>(), workflowStates);

            // act
            var createdWorkflowStates = await _workflowRepository.CreateWorkflowStatesAsync(workflowStates, PublishRevision);

            // assert
            Assert.IsNotNull(createdWorkflowStates);
            var createdWorkflowStatesList = createdWorkflowStates.ToList();
            foreach (var workflowState in workflowStates)
            {
                Assert.IsTrue(createdWorkflowStatesList.Exists(x => x.WorkflowStateId == workflowState.WorkflowStateId));
            }
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task CreateWorkflowStatesAsync_NullWorkflowStatesList_ArgumentNullException()
        {
            // arrange

            // act
            await _workflowRepository.CreateWorkflowStatesAsync(null, PublishRevision);

            // assert
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task CreateWorkflowStatesAsync_EmptyWorkflowStatesList_ArgumentException()
        {
            // arrange

            // act
            await _workflowRepository.CreateWorkflowStatesAsync(new List<SqlState>(), PublishRevision);

            // assert
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task CreateWorkflowStatesAsync_InvalidPublishRevision_ArgumentException()
        {
            // arrange
            var workflowStates = new List<SqlState>(3);
            workflowStates.AddRange(_workflowStateIds.Select(
                workflowStateId => new SqlState
                {
                    WorkflowStateId = workflowStateId
                }));

            // act
            await _workflowRepository.CreateWorkflowStatesAsync(workflowStates, IncorrectPublishRevision);

            // assert
        }

        #endregion

        #region UpdateWorkflowStatesAsync

        [TestMethod]
        public async Task UpdateWorkflowStatesAsync_UpdateWorkflowStatesInDb_QueryReturnWorkflowStates()
        {
            // arrange
            var workflowStates = new List<SqlState>(3);
            workflowStates.AddRange(_workflowStateIds.Select(
                workflowStateId => new SqlState
                {
                    WorkflowStateId = workflowStateId
                }));
            _sqlConnectionWrapperMock.SetupQueryAsync("UpdateWorkflowStates", It.IsAny<Dictionary<string, object>>(), workflowStates);

            // act
            var updatedWorkflowStates = await _workflowRepository.UpdateWorkflowStatesAsync(workflowStates, PublishRevision);

            // assert
            Assert.IsNotNull(updatedWorkflowStates);
            var updatedWorkflowStatesList = updatedWorkflowStates.ToList();
            foreach (var workflowState in workflowStates)
            {
                Assert.IsTrue(updatedWorkflowStatesList.Exists(x => x.WorkflowStateId == workflowState.WorkflowStateId));
            }
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task UpdateWorkflowStatesAsync_NullWorkflowStatesList_ArgumentNullException()
        {
            // arrange

            // act
            await _workflowRepository.UpdateWorkflowStatesAsync(null, PublishRevision);

            // assert
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task UpdateWorkflowStatesAsync_EmptyWorkflowStatesList_ArgumentException()
        {
            // arrange

            // act
            await _workflowRepository.UpdateWorkflowStatesAsync(new List<SqlState>(), PublishRevision);

            // assert
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task UpdateWorkflowStatesAsync_InvalidPublishRevision_ArgumentException()
        {
            // arrange
            var workflowStates = new List<SqlState>(3);
            workflowStates.AddRange(_workflowStateIds.Select(
                workflowStateId => new SqlState
                {
                    WorkflowStateId = workflowStateId
                }));

            // act
            await _workflowRepository.UpdateWorkflowStatesAsync(workflowStates, IncorrectPublishRevision);

            // assert
        }

        #endregion

        #region DeleteWorkflowEventsAsync

        [TestMethod]
        public async Task DeleteWorkflowEventsAsync_DeleteWorkflowEventsInDb_QueryReturnWorkflowEventIds()
        {
            // arrange
            var resultSqlEvents = new List<SqlWorkflowEvent>(3);
            resultSqlEvents.AddRange(_workflowEventIds.Select(
                workflowEventId => new SqlWorkflowEvent
                {
                    WorkflowEventId = workflowEventId
                }));
            _sqlConnectionWrapperMock.SetupQueryAsync("DeleteWorkflowEvents", It.IsAny<Dictionary<string, object>>(), resultSqlEvents);

            // act
            var deletedWorkflowEvents = await _workflowRepository.DeleteWorkflowEventsAsync(_workflowEventIds, PublishRevision);

            // assert
            Assert.IsNotNull(deletedWorkflowEvents);
            var deletedWorkflowEventsList = deletedWorkflowEvents.ToList();
            foreach (var workflowEventId in _workflowEventIds)
            {
                Assert.IsTrue(deletedWorkflowEventsList.Exists(x => x == workflowEventId));
            }
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task DeleteWorkflowEventsAsync_EmptyWorkflowEventIdsList_ArgumentException()
        {
            // arrange

            // act
            await _workflowRepository.DeleteWorkflowEventsAsync(new List<int>(), PublishRevision);

            // assert
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task DeleteWorkflowEventsAsync_InvalidPublishRevision_ArgumentException()
        {
            // arrange

            // act
            await _workflowRepository.DeleteWorkflowEventsAsync(_workflowEventIds, IncorrectPublishRevision);

            // assert
        }

        #endregion

        #region CreateWorkflowEventsAsync

        [TestMethod]
        public async Task CreateWorkflowEventsAsync_CreateWorkflowEventsInDb_QueryReturnWorkflowEvents()
        {
            // arrange
            var workflowEvents = new List<SqlWorkflowEvent>(3);
            workflowEvents.AddRange(_workflowEventIds.Select(
                workflowEventId => new SqlWorkflowEvent
                {
                    WorkflowEventId = workflowEventId
                }));
            _sqlConnectionWrapperMock.SetupQueryAsync("CreateWorkflowEvents", It.IsAny<Dictionary<string, object>>(), workflowEvents);

            // act
            var createdWorkflowEvents = await _workflowRepository.CreateWorkflowEventsAsync(workflowEvents, PublishRevision);

            // assert
            Assert.IsNotNull(createdWorkflowEvents);
            var createdWorkflowEventsList = createdWorkflowEvents.ToList();
            foreach (var workflowEvent in workflowEvents)
            {
                Assert.IsTrue(createdWorkflowEventsList.Exists(x => x.WorkflowEventId == workflowEvent.WorkflowEventId));
            }
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task CreateWorkflowEventsAsync_NullWorkflowEventsList_ArgumentNullException()
        {
            // arrange

            // act
            await _workflowRepository.CreateWorkflowEventsAsync(null, PublishRevision);

            // assert
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task CreateWorkflowEventsAsync_EmptyWorkflowEventsList_ArgumentException()
        {
            // arrange

            // act
            await _workflowRepository.CreateWorkflowEventsAsync(new List<SqlWorkflowEvent>(), PublishRevision);

            // assert
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task CreateWorkflowEventsAsync_InvalidPublishRevision_ArgumentException()
        {
            // arrange
            var workflowEvents = new List<SqlWorkflowEvent>(3);
            workflowEvents.AddRange(_workflowEventIds.Select(
                workflowEventId => new SqlWorkflowEvent
                {
                    WorkflowEventId = workflowEventId
                }));

            // act
            await _workflowRepository.CreateWorkflowEventsAsync(workflowEvents, IncorrectPublishRevision);

            // assert
        }

        #endregion

        #region UpdateWorkflowEventsAsync

        [TestMethod]
        public async Task UpdateWorkflowEventsAsync_UpdateWorkflowEventsInDb_QueryReturnWorkflowEvents()
        {
            // arrange
            var workflowEvents = new List<SqlWorkflowEvent>(3);
            workflowEvents.AddRange(_workflowEventIds.Select(
                workflowEventId => new SqlWorkflowEvent
                {
                    WorkflowEventId = workflowEventId
                }));
            _sqlConnectionWrapperMock.SetupQueryAsync("UpdateWorkflowEvents", It.IsAny<Dictionary<string, object>>(), workflowEvents);

            // act
            var updatedWorkflowEvents = await _workflowRepository.UpdateWorkflowEventsAsync(workflowEvents, PublishRevision);

            // assert
            Assert.IsNotNull(updatedWorkflowEvents);
            var updatedWorkflowEventsList = updatedWorkflowEvents.ToList();
            foreach (var workflowEvent in workflowEvents)
            {
                Assert.IsTrue(updatedWorkflowEventsList.Exists(x => x.WorkflowEventId == workflowEvent.WorkflowEventId));
            }
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task UpdateWorkflowEventsAsync_NullWorkflowEventsList_ArgumentNullException()
        {
            // arrange

            // act
            await _workflowRepository.UpdateWorkflowEventsAsync(null, PublishRevision);

            // assert
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task UpdateWorkflowEventsAsync_EmptyWorkflowEventsList_ArgumentException()
        {
            // arrange

            // act
            await _workflowRepository.UpdateWorkflowEventsAsync(new List<SqlWorkflowEvent>(), PublishRevision);

            // assert
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task UpdateWorkflowEventsAsync_InvalidPublishRevision_ArgumentException()
        {
            // arrange
            var workflowEvents = new List<SqlWorkflowEvent>(3);
            workflowEvents.AddRange(_workflowEventIds.Select(
                workflowEventId => new SqlWorkflowEvent
                {
                    WorkflowEventId = workflowEventId
                }));

            // act
            await _workflowRepository.UpdateWorkflowEventsAsync(workflowEvents, IncorrectPublishRevision);

            // assert
        }

        #endregion
    }
}
