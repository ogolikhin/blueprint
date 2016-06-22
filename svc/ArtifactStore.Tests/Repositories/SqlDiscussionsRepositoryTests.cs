using ArtifactStore.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using ServiceLibrary.Repositories;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ArtifactStore.Repositories
{
    [TestClass]
    public class SqlDiscussionsRepositoryTests
    {
        private IUsersRepository userRepository;
        private IInstanceSettingsRepository instanceSettingsRepository;
        private IArtifactPermissionsRepository artifactPermissionsRepository;
        private IDiscussionsRepository discussionsRepository;
        private SqlConnectionWrapperMock cxn;

        [TestInitialize]
        public void Initialize()
        {
            cxn = new SqlConnectionWrapperMock();
            userRepository = new SqlUserRepositoryMock();
            instanceSettingsRepository = new SqlInstanceSettingsRepositoryMock();
            artifactPermissionsRepository = new SqlArtifactPermissionsRepository(cxn.Object);
            discussionsRepository = new SqlDiscussionsRepository(cxn.Object, userRepository, instanceSettingsRepository, artifactPermissionsRepository);
        }
        [TestMethod]
        public async Task GetDiscussions()
        {
            // Arrange
            int itemId = 1;
            int userId = 1;
            bool includeDrafts = true;
            cxn.SetupQueryAsync("GetItemDiscussions", new Dictionary<string, object> { { "ItemId", itemId}, { "UserId", userId }, { "AddDrafts", includeDrafts } }, new List<Discussion> { new Discussion { ItemId = itemId, DiscussionId = 1, UserId = userId, Comment = "<html></html>"} });
            cxn.SetupQueryAsync("GetItemDiscussionStates", new Dictionary<string, object> { { "ItemId", itemId }, { "UserId", userId }, { "IncludeDrafts", includeDrafts } }, new List<DiscussionState> { new DiscussionState { DiscussionId = 1 } });
            // Act
            var result = await discussionsRepository.GetDiscussions(itemId, userId, includeDrafts);
        }
    }
}
