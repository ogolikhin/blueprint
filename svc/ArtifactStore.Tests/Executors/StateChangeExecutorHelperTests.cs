using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using ArtifactStore.Repositories;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using ServiceLibrary.Models.Reuse;
using ServiceLibrary.Models.VersionControl;
using ServiceLibrary.Models.Workflow;
using ServiceLibrary.Repositories;
using ServiceLibrary.Repositories.ConfigControl;
using ServiceLibrary.Repositories.Reuse;
using ServiceLibrary.Repositories.Workflow;

namespace ArtifactStore.Executors
{
    [TestClass]
    public class StateChangeExecutorHelperTests
    {
        private IStateChangeExecutorRepositories _stateChangeExecutorRepositories;
        private IStateChangeExecutorHelper _stateChangeExecutorHelper;

        private Mock<IWorkflowRepository> _workflowRepository;
        private Mock<IArtifactVersionsRepository> _artifactVersionsRepository;
        private Mock<IVersionControlService> _versionControlService;
        private Mock<IReuseRepository> _reuseRepository;
        private Mock<ISaveArtifactRepository> _saveArtifactRepositoryMock;
        private Mock<IApplicationSettingsRepository> _applicationSettingsRepositoryMock;
        private Mock<IServiceLogRepository> _serviceLogRepositoryMock;
        private Mock<IUsersRepository> _usersRepositoryMock;
        [TestInitialize]
        public void Setup()
        {
            _workflowRepository = new Mock<IWorkflowRepository>(MockBehavior.Strict);
            _artifactVersionsRepository = new Mock<IArtifactVersionsRepository>(MockBehavior.Strict);
            _versionControlService = new Mock<IVersionControlService>(MockBehavior.Loose);
            _reuseRepository = new Mock<IReuseRepository>(MockBehavior.Loose);
            _saveArtifactRepositoryMock = new Mock<ISaveArtifactRepository>(MockBehavior.Loose);
            _applicationSettingsRepositoryMock = new Mock<IApplicationSettingsRepository>(MockBehavior.Loose);
            _serviceLogRepositoryMock = new Mock<IServiceLogRepository>(MockBehavior.Loose);
            _usersRepositoryMock = new Mock<IUsersRepository>(MockBehavior.Loose);

            _stateChangeExecutorRepositories = new StateChangeExecutorRepositories(_artifactVersionsRepository.Object,
                _workflowRepository.Object,
                _versionControlService.Object,
                _reuseRepository.Object,
                _saveArtifactRepositoryMock.Object,
                _applicationSettingsRepositoryMock.Object,
                _serviceLogRepositoryMock.Object,
                _usersRepositoryMock.Object);

            _stateChangeExecutorHelper = new StateChangeExecutorHelper(_stateChangeExecutorRepositories);
        }
        [TestMethod]
        public async Task BuildTriggerExecutionParameters_WhenTriggersAreEmpty_ReturnsNull()
        {
            // Arrange
            var result =
                await
                    _stateChangeExecutorHelper.BuildTriggerExecutionParameters(1, null, new WorkflowEventTriggers(),
                        null);

            // Assert
            Assert.IsNull(result);
        }

        [TestMethod]
        public async Task BuildTriggerExecutionParameters_WhenValid_ReturnsExecutionParameters()
        {
            // Arrange
            var artifactId = 1;
            var artifactStandardItemTypeId = 2;
            var artifactItemTypeId = 3;
            var artifactInfo = new VersionControlArtifactInfo()
            {
                Id = artifactId,
                ItemTypeId = artifactItemTypeId
            };
            var triggers = new WorkflowEventTriggers();
            triggers.Add(new WorkflowEventTrigger());
            var sqlItemTypeInfo = new SqlItemTypeInfo() { InstanceTypeId = artifactStandardItemTypeId};
            var isArtifactReadonlyReuseDictionary = new Dictionary<int, bool>() { { artifactId, false}};
            var artifactStandardItemTypeDictionary = new Dictionary<int, SqlItemTypeInfo>() { { artifactId, sqlItemTypeInfo } };
            _reuseRepository.Setup(
                r => r.DoItemsContainReadonlyReuse(It.IsAny<IEnumerable<int>>(), It.IsAny<IDbTransaction>()))
                .ReturnsAsync(isArtifactReadonlyReuseDictionary);
            _reuseRepository.Setup(
               r => r.GetStandardTypeIdsForArtifactsIdsAsync(It.IsAny<ISet<int>>()))
               .ReturnsAsync(artifactStandardItemTypeDictionary);

            var result =
                await
                    _stateChangeExecutorHelper.BuildTriggerExecutionParameters(1, artifactInfo, triggers,
                        null);

            // Assert
            Assert.IsNotNull(result);
        }
    }
}
