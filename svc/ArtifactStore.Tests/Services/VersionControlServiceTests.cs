using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using ArtifactStore.Repositories.Revisions;
using ArtifactStore.Repositories.VersionControl;
using ArtifactStore.Services.VersionControl;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using ServiceLibrary.Exceptions;
using ServiceLibrary.Helpers;
using ServiceLibrary.Models.VersionControl;

namespace ArtifactStore.Services
{
    [TestClass]
    public class VersionControlServiceTests
    {
        private Mock<IVersionControlRepository> _versionControlRepository;
        private Mock<IPublishRepository> _publishRepository;
        private Mock<IRevisionRepository> _revisionRepository;
        private Mock<ISqlHelper> _sqlHelper;
        [TestInitialize]
        public void TestInitialize()
        {
            _versionControlRepository = new Mock<IVersionControlRepository>(MockBehavior.Strict);
            _publishRepository = new Mock<IPublishRepository>(MockBehavior.Strict);
            _revisionRepository = new Mock<IRevisionRepository>(MockBehavior.Strict);
            _sqlHelper = new Mock<ISqlHelper>(MockBehavior.Strict);
        }
        [TestMethod]
        [ExpectedException(typeof(BadRequestException))]
        public async Task PublishArtifacts_EmptyListIds_ThrowsBadRequestException()
        {
            var versionControlService = new VersionControlService(
                _versionControlRepository.Object, 
                _publishRepository.Object, 
                _revisionRepository.Object, 
                _sqlHelper.Object);

            var publishParameters = new PublishParameters {ArtifactIds = new List<int>() };

            await versionControlService.PublishArtifacts(publishParameters);
        }
        [TestMethod]
        [ExpectedException(typeof(ResourceNotFoundException))]
        public async Task PublishArtifacts_ItemCountMismatch_ThrowsBadRequestException()
        {
            _versionControlRepository.Setup(
                a => a.GetDiscardPublishStates(It.IsAny<int>(), It.IsAny<IEnumerable<int>>(), It.IsAny<IDbTransaction>()))
                .ReturnsAsync(new List<SqlDiscardPublishState>
                {
                    new SqlDiscardPublishState()
                });
            var versionControlService = new VersionControlService(
                _versionControlRepository.Object,
                _publishRepository.Object,
                _revisionRepository.Object,
                _sqlHelper.Object);

            var publishParameters = new PublishParameters {ArtifactIds = new List<int>() {1, 2, 3}};

            await versionControlService.PublishArtifacts(publishParameters);
        }
        [TestMethod]
        [ExpectedException(typeof(ResourceNotFoundException))]
        public async Task PublishArtifacts_ItemNotExists_ThrowsResourceNotFoundException()
        {
            _versionControlRepository.Setup(
                a => a.GetDiscardPublishStates(It.IsAny<int>(), It.IsAny<IEnumerable<int>>(), It.IsAny<IDbTransaction>()))
                .ReturnsAsync(new List<SqlDiscardPublishState>
                {
                    new SqlDiscardPublishState
                    {
                        NotExist = true
                    }
                });
            var versionControlService = new VersionControlService(
                _versionControlRepository.Object,
                _publishRepository.Object,
                _revisionRepository.Object,
                _sqlHelper.Object);

            var publishParameters = new PublishParameters {ArtifactIds = new List<int>() {1}};

            await versionControlService.PublishArtifacts(publishParameters);
        }
        [TestMethod]
        [ExpectedException(typeof(ResourceNotFoundException))]
        public async Task PublishArtifacts_ItemNotArtifact_ThrowsResourceNotFoundException()
        {
            _versionControlRepository.Setup(
                a => a.GetDiscardPublishStates(It.IsAny<int>(), It.IsAny<IEnumerable<int>>(), It.IsAny<IDbTransaction>()))
                .ReturnsAsync(new List<SqlDiscardPublishState>
                {
                    new SqlDiscardPublishState
                    {
                        NotArtifact = true
                    }
                });
            var versionControlService = new VersionControlService(
                _versionControlRepository.Object,
                _publishRepository.Object,
                _revisionRepository.Object,
                _sqlHelper.Object);

            var publishParameters = new PublishParameters {ArtifactIds = new List<int>() {1}};

            await versionControlService.PublishArtifacts(publishParameters);
        }

        [TestMethod]
        [ExpectedException(typeof(ResourceNotFoundException))]
        public async Task PublishArtifacts_ItemDeleted_ThrowsResourceNotFoundException()
        {
            _versionControlRepository.Setup(
                a => a.GetDiscardPublishStates(It.IsAny<int>(), It.IsAny<IEnumerable<int>>(), It.IsAny<IDbTransaction>()))
                .ReturnsAsync(new List<SqlDiscardPublishState>
                {
                    new SqlDiscardPublishState
                    {
                        Deleted = true
                    }
                });
            var versionControlService = new VersionControlService(
                _versionControlRepository.Object,
                _publishRepository.Object,
                _revisionRepository.Object,
                _sqlHelper.Object);

            var publishParameters = new PublishParameters {ArtifactIds = new List<int>() {1}};

            await versionControlService.PublishArtifacts(publishParameters);
        }
        [TestMethod]
        [ExpectedException(typeof(ConflictException))]
        public async Task PublishArtifacts_ItemNoChanges_ThrowsConflictException()
        {
            _versionControlRepository.Setup(
                a => a.GetDiscardPublishStates(It.IsAny<int>(), It.IsAny<IEnumerable<int>>(), It.IsAny<IDbTransaction>()))
                .ReturnsAsync(new List<SqlDiscardPublishState>
                {
                    new SqlDiscardPublishState
                    {
                        NoChanges = true
                    }
                });
            var versionControlService = new VersionControlService(
                _versionControlRepository.Object,
                _publishRepository.Object,
                _revisionRepository.Object,
                _sqlHelper.Object);

            var publishParameters = new PublishParameters {ArtifactIds = new List<int>() {1}};

            await versionControlService.PublishArtifacts(publishParameters);
        }
        [TestMethod]
        [ExpectedException(typeof(ConflictException))]
        public async Task PublishArtifacts_ItemInvalid_ThrowsConflictException()
        {
            _versionControlRepository.Setup(
                a => a.GetDiscardPublishStates(It.IsAny<int>(), It.IsAny<IEnumerable<int>>(), It.IsAny<IDbTransaction>()))
                .ReturnsAsync(new List<SqlDiscardPublishState>
                {
                    new SqlDiscardPublishState
                    {
                        Invalid = true
                    }
                });
            var versionControlService = new VersionControlService(
                _versionControlRepository.Object,
                _publishRepository.Object,
                _revisionRepository.Object,
                _sqlHelper.Object);

            var publishParameters = new PublishParameters {ArtifactIds = new List<int>() {1}};

            await versionControlService.PublishArtifacts(publishParameters);
        }
    }
}
