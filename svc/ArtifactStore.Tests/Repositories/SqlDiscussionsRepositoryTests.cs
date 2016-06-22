using ArtifactStore.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using ServiceLibrary.Repositories;
using System.Collections.Generic;
using System.Linq;
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
        public async Task GetDiscussions_CommentReturned_NoCommentReturned()
        {
            // Arrange
            int itemId = 1;
            int projectId = 1;
            cxn.SetupQueryAsync("GetItemDiscussions", new Dictionary<string, object> { { "ItemId", itemId }}, new List<Discussion>());
            cxn.SetupQueryAsync("GetItemDiscussionStates", new Dictionary<string, object> { { "ItemId", itemId }}, new List<DiscussionState>());
            // Act
            var result = (await discussionsRepository.GetDiscussions(itemId, projectId)).ToList();
            Assert.AreEqual(0, result.Count);
        }
        [TestMethod]
        public async Task GetDiscussions_CommentReturned_CorrectCommentReturned()
        {
            // Arrange
            int itemId = 1;
            int projectId = 1;
            cxn.SetupQueryAsync("GetItemDiscussions", new Dictionary<string, object> { { "ItemId", itemId} }, new List<Discussion> { new Discussion { ItemId = itemId, DiscussionId = 1, UserId = 1, Comment = "<html></html>"} });
            cxn.SetupQueryAsync("GetItemDiscussionStates", new Dictionary<string, object> { { "ItemId", itemId }}, new List<DiscussionState> { new DiscussionState { DiscussionId = 1, IsClosed = false, Status = "Test Status" } });
            // Act
            var result = (await discussionsRepository.GetDiscussions(itemId, projectId)).ToList();

            Assert.AreEqual(1, result.Count);
            Assert.AreEqual(1, result[0].UserId);
            Assert.AreEqual("Test Status", result[0].Status);
            Assert.AreEqual(false, result[0].IsClosed);
        }

        [TestMethod]
        public async Task GetReplies_CommentReturned_NoCommentReturned()
        {
            // Arrange
            int discussionId = 1;
            int projectId = 1;
            cxn.SetupQueryAsync("GetItemReplies", new Dictionary<string, object> { { "DiscussionId", discussionId } }, new List<Reply>());
            // Act
            var result = (await discussionsRepository.GetReplies(discussionId, projectId)).ToList();
            Assert.AreEqual(0, result.Count);
        }
        [TestMethod]
        public async Task GetReplies_CommentReturned_CorrectCommentReturned()
        {
            // Arrange
            int discussionId = 1;
            int projectId = 1;
            cxn.SetupQueryAsync("GetItemReplies", new Dictionary<string, object> { { "DiscussionId", discussionId }}, new List<Reply> { new Reply { ItemId = 1, DiscussionId = 1, ReplyId = 2, UserId = 1, Comment = "<html></html>" } });
            // Act
            var result = (await discussionsRepository.GetReplies(discussionId, projectId)).ToList();

            Assert.AreEqual(1, result.Count);
            Assert.AreEqual(1, result[0].UserId);
            Assert.AreEqual(2, result[0].ReplyId);
        }
    }
}
