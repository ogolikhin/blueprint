using Microsoft.VisualStudio.TestTools.UnitTesting;
using ServiceLibrary.Models;
using ServiceLibrary.Repositories;
using System.Collections.Generic;
using System.Threading.Tasks;
using ServiceLibrary.Repositories.InstanceSettings;

namespace ArtifactStore.Helpers
{
    [TestClass]
    public class MentionHelperTests
    {
        private IUsersRepository _userRepository;
        private IInstanceSettingsRepository _instanceSettingsRepository;
        private IArtifactPermissionsRepository _artifactPermissionsRepository;
        private EmailSettings _fakeEmailSettings;
        private InstanceSettings _instanceSettings;
        private MentionHelper _mentionHelper;
        private SqlConnectionWrapperMock _cxn;

        [TestInitialize]
        public void init()
        {

            _fakeEmailSettings =  new EmailSettings
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
            _mentionHelper = new MentionHelper(_userRepository, _instanceSettingsRepository, _artifactPermissionsRepository);
        }

        [TestMethod]
        public void CheckUsersEmailDomain_NotGuest_ReturnsTrue()
        {
            // Arrange
            var email = "test@email.com";
            var isUserEnabled = false;
            var isGuest = false;
            var emailSettings = new EmailSettings();

            // Act
            var result = _mentionHelper.CheckUsersEmailDomain(email, isUserEnabled, isGuest, emailSettings);
            Assert.IsTrue(result);
        }
        [TestMethod]
        public void CheckUsersEmailDomain_UserNotEnabled_ReturnsFalse()
        {
            // Arrange
            var email = "test@email.com";
            var isUserEnabled = false;
            var isGuest = true;
            var emailSettings = new EmailSettings();

            // Act
            var result = _mentionHelper.CheckUsersEmailDomain(email, isUserEnabled, isGuest, emailSettings);
            Assert.IsFalse(result);
        }
        [TestMethod]
        public void CheckUsersEmailDomain_NullEmailSettings_ReturnsFalse()
        {
            // Arrange
            var email = "test@email.com";
            var isUserEnabled = true;
            var isGuest = true;

            // Act
            var result = _mentionHelper.CheckUsersEmailDomain(email, isUserEnabled, isGuest, null);
            Assert.IsFalse(result);
        }
        [TestMethod]
        public void CheckUsersEmailDomain_NoUsers_ReturnsFalse()
        {
            // Arrange
            var email = "testemail@MyDomain";
            var isUserEnabled = true;
            var isGuest = true;
            var emailSettings = new EmailSettings();
            emailSettings.Domains = "MyDomain;MyOtherDomain;";
            emailSettings.EnableAllUsers = false;
            // Act
            var result = _mentionHelper.CheckUsersEmailDomain(email, isUserEnabled, isGuest, emailSettings);
            Assert.IsFalse(result);
        }
        [TestMethod]
        public void CheckUsersEmailDomain_UsersNoDomain_ReturnsTrue()
        {
            // Arrange
            var email = "testemail@MyDomain";
            var isUserEnabled = true;
            var isGuest = true;
            var emailSettings = new EmailSettings();
            emailSettings.Domains = "MyDomain;MyOtherDomain;";
            emailSettings.EnableAllUsers = true;
            emailSettings.EnableDomains = false;
            // Act
            var result = _mentionHelper.CheckUsersEmailDomain(email, isUserEnabled, isGuest, emailSettings);
            Assert.IsTrue(result);
        }
        [TestMethod]
        public void CheckUsersEmailDomain_UsersDomainsMatchingDomain_ReturnsTrue()
        {
            // Arrange
            var email = "testemail@MyDomain";
            var isUserEnabled = true;
            var isGuest = true;
            var emailSettings = new EmailSettings();
            emailSettings.Domains = "MyDomain;MyOtherDomain;";
            emailSettings.EnableAllUsers = true;
            emailSettings.EnableDomains = true;

            // Act
            var result = _mentionHelper.CheckUsersEmailDomain(email, isUserEnabled, isGuest, emailSettings);
            Assert.IsTrue(result);
        }

        [TestMethod]
        public async Task IsEmailBlocked_GuestUserDisabled_ReturnsTrue()
        {
            // Arrange
            var email = "DisabledUser@MyDomain";
            // Act
            var result = await _mentionHelper.IsEmailBlocked(email);
            // Assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        public async Task IsEmailBlocked_NotGuestUserEnabled_ReturnsFalse()
        {
            // Arrange
            var email = "User@MyDomain";
            // Act
            var result = await _mentionHelper.IsEmailBlocked(email);
            // Assert
            Assert.IsFalse(result);
        }
        [TestMethod]
        public async Task AreEmailDiscussionsEnabled_DisabledEmailReplies_ReturnsFalse()
        {
            // Arrange
            var projectId = 1;
            _fakeEmailSettings.EnableEmailReplies = false;
            _instanceSettingsRepository = new SqlInstanceSettingsRepositoryMock(_fakeEmailSettings, _instanceSettings);
            // Act
            var result = await _mentionHelper.AreEmailDiscussionsEnabled(projectId);
            // Assert
            Assert.IsFalse(result);
        }

        [TestMethod]
        public async Task AreEmailDiscussionsEnabled_EnableEmailRepliesProjectHasPermission_ReturnsTrue()
        {
            // Arrange
            var projectId = 1;
            _fakeEmailSettings.EnableEmailReplies = true;
            _instanceSettingsRepository = new SqlInstanceSettingsRepositoryMock(_fakeEmailSettings, _instanceSettings);
            _cxn.SetupExecuteScalarAsync("GetProjectPermissions", new Dictionary<string, object> { { "ProjectId", projectId} },  ProjectPermissions.AreEmailRepliesEnabled);
            // Act
            var result = await _mentionHelper.AreEmailDiscussionsEnabled(projectId);
            // Assert
            Assert.IsTrue(result);
        }
    }
}
