using ArtifactStore.Repositories;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ServiceLibrary.Models;
using ServiceLibrary.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArtifactStore.Helpers
{
    [TestClass]
    public class MentionHelperTests
    {
        private IUsersRepository userRepository;
        private IInstanceSettingsRepository instanceSettingsRepository;
        private IArtifactPermissionsRepository artifactPermissionsRepository;
        private EmailSettings fakeEmailSettings;
        private MentionHelper mentionHelper;
        private SqlConnectionWrapperMock cxn;

        [TestInitialize]
        public void init()
        {

            fakeEmailSettings =  new EmailSettings
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
            cxn = new SqlConnectionWrapperMock();
            userRepository = new SqlUserRepositoryMock();
            instanceSettingsRepository = new SqlInstanceSettingsRepositoryMock(fakeEmailSettings);
            artifactPermissionsRepository = new SqlArtifactPermissionsRepository(cxn.Object);
            mentionHelper = new MentionHelper(userRepository, instanceSettingsRepository, artifactPermissionsRepository);
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
            var result = mentionHelper.CheckUsersEmailDomain(email, isUserEnabled, isGuest, emailSettings);
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
            var result = mentionHelper.CheckUsersEmailDomain(email, isUserEnabled, isGuest, emailSettings);
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
            var result = mentionHelper.CheckUsersEmailDomain(email, isUserEnabled, isGuest, null);
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
            var result = mentionHelper.CheckUsersEmailDomain(email, isUserEnabled, isGuest, emailSettings);
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
            var result = mentionHelper.CheckUsersEmailDomain(email, isUserEnabled, isGuest, emailSettings);
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
            var result = mentionHelper.CheckUsersEmailDomain(email, isUserEnabled, isGuest, emailSettings);
            Assert.IsTrue(result);
        }

        [TestMethod]
        public async Task IsEmailBlocked_GuestUserDisabled_ReturnsTrue()
        {
            // Arrange
            var email = "DisabledUser@MyDomain";
            // Act
            var result = await mentionHelper.IsEmailBlocked(email);
            // Assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        public async Task IsEmailBlocked_NotGuestUserEnabled_ReturnsFalse()
        {
            // Arrange
            var email = "User@MyDomain";
            // Act
            var result = await mentionHelper.IsEmailBlocked(email);
            // Assert
            Assert.IsFalse(result);
        }
        [TestMethod]
        public async Task AreEmailDiscussionsEnabled_DisabledEmailReplies_ReturnsFalse()
        {
            // Arrange
            var projectId = 1;
            fakeEmailSettings.EnableEmailReplies = false;
            instanceSettingsRepository = new SqlInstanceSettingsRepositoryMock(fakeEmailSettings);
            // Act
            var result = await mentionHelper.AreEmailDiscussionsEnabled(projectId);
            // Assert
            Assert.IsFalse(result);
        }

        [TestMethod]
        public async Task AreEmailDiscussionsEnabled_EnableEmailRepliesProjectHasPermission_ReturnsTrue()
        {
            // Arrange
            var projectId = 1;
            fakeEmailSettings.EnableEmailReplies = true;
            instanceSettingsRepository = new SqlInstanceSettingsRepositoryMock(fakeEmailSettings);
            cxn.SetupExecuteScalarAsync("GetProjectPermissions", new Dictionary<string, object> { { "ProjectId", projectId} },  ProjectPermissions.AreEmailRepliesEnabled);
            // Act
            var result = await mentionHelper.AreEmailDiscussionsEnabled(projectId);
            // Assert
            Assert.IsTrue(result);
        }
    }
}
