using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using ServiceLibrary.Repositories;

namespace ArtifactStore.Repositories
{
    [TestClass]
    public class SqlDiscussionsRepositoryTests
    {
        private IUsersRepository userRepository;
        private IInstanceSettingsRepository instanceSettingsRepository;
        private IArtifactPermissionsRepository artifactPermissionsRepository;
        private IDiscussionsRepository discussionsRepository;
        private Mock<ISqlConnectionWrapper> cxn;

        [TestInitialize]
        public void Initialize()
        {
            cxn = new SqlConnectionWrapperMock();
            userRepository = new SqlUserRepositoryMock();
            instanceSettingsRepository = new SqlInstanceSettingsRepositoryMock();
            artifactPermissionsRepository = new SqlArtifactPermissionsRepository(cxn.Object);
            discussionsRepository = new SqlDiscussionsRepository(cxn.Object, userRepository, instanceSettingsRepository, artifactPermissionsRepository);
        }
    }
}
