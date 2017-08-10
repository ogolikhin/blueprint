using ArtifactStore.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ServiceLibrary.Models;
using ServiceLibrary.Repositories;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ServiceLibrary.Repositories.InstanceSettings;

namespace ArtifactStore.Repositories
{
    [TestClass]
    public class SqlDiscussionsRepositoryTests
    {
        private IUsersRepository _userRepository;
        private IInstanceSettingsRepository _instanceSettingsRepository;
        private IArtifactPermissionsRepository _artifactPermissionsRepository;
        private IDiscussionsRepository _discussionsRepository;
        private SqlConnectionWrapperMock _cxn;
        private EmailSettings _fakeEmailSettings;
        private InstanceSettings _instanceSettings;


        [TestInitialize]
        public void Initialize()
        {
            _fakeEmailSettings = new EmailSettings
            {
                Id = "Fake",
                Authenticated = false,
                Domains = "Domain;MyDomain",
                EnableAllUsers = true,
                EnableDomains = true,
                EnableEmailDiscussion = false,
                EnableEmailReplies = false,
                EnableNotifications = false,
                EnableSSL = false,
                HostName = "FakeHostName",
                IncomingEnableSSL = false,
                IncomingHostName = "FakeIncomingHostName",
                IncomingPassword = "FakeIncomingPassword",
                IncomingPort = 1234,
                IncomingServerType = 1,
                IncomingUserName = "FakeIncomingUserName",
                Password = "FakePassword",
                Port = 1234,
                SenderEmailAddress = "FakeSenderAddress",
                UserName = "FakeUserName"
            };
            _instanceSettings = new InstanceSettings();
            _cxn = new SqlConnectionWrapperMock();
            _userRepository = new SqlUserRepositoryMock();
            _instanceSettingsRepository = new SqlInstanceSettingsRepositoryMock(_fakeEmailSettings, _instanceSettings);
            _artifactPermissionsRepository = new SqlArtifactPermissionsRepository(_cxn.Object);
            _discussionsRepository = new SqlDiscussionsRepository(_cxn.Object, _userRepository, _instanceSettingsRepository, _artifactPermissionsRepository);
        }
        [TestMethod]
        public async Task GetDiscussions_CommentReturned_NoCommentReturned()
        {
            // Arrange
            int itemId = 1;
            int projectId = 1;
            _cxn.SetupQueryAsync("GetItemDiscussions", new Dictionary<string, object> { { "ItemId", itemId }}, new List<Discussion>());
            _cxn.SetupQueryAsync("GetItemDiscussionStates", new Dictionary<string, object> { { "ItemId", itemId }}, new List<DiscussionState>());
            // Act
            var result = (await _discussionsRepository.GetDiscussions(itemId, projectId)).ToList();
            Assert.AreEqual(0, result.Count);
        }
        [TestMethod]
        public async Task GetDiscussions_CommentReturned_CorrectCommentReturned()
        {
            // Arrange
            int itemId = 1;
            int projectId = 1;
            _cxn.SetupQueryAsync("GetItemDiscussions", new Dictionary<string, object> { { "ItemId", itemId} }, new List<Discussion> { new Discussion { ItemId = itemId, DiscussionId = 1, UserId = 1, Comment = "<html></html>"} });
            _cxn.SetupQueryAsync("GetItemDiscussionStates", new Dictionary<string, object> { { "ItemId", itemId }}, new List<DiscussionState> { new DiscussionState { DiscussionId = 1, IsClosed = false, Status = "Test Status" } });
            // Act
            var result = (await _discussionsRepository.GetDiscussions(itemId, projectId)).ToList();

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
            _cxn.SetupQueryAsync("GetItemReplies", new Dictionary<string, object> { { "DiscussionId", discussionId } }, new List<Reply>());
            // Act
            var result = (await _discussionsRepository.GetReplies(discussionId, projectId)).ToList();
            Assert.AreEqual(0, result.Count);
        }
        [TestMethod]
        public async Task GetReplies_CommentReturned_CorrectCommentReturned()
        {
            // Arrange
            int discussionId = 1;
            int projectId = 1;
            _cxn.SetupQueryAsync("GetItemReplies", new Dictionary<string, object> { { "DiscussionId", discussionId }}, new List<Reply> { new Reply { ItemId = 1, DiscussionId = 1, ReplyId = 2, UserId = 1, Comment = "<html></html>" } });
            // Act
            var result = (await _discussionsRepository.GetReplies(discussionId, projectId)).ToList();

            Assert.AreEqual(1, result.Count);
            Assert.AreEqual(1, result[0].UserId);
            Assert.AreEqual(2, result[0].ReplyId);
        }
    }
}
