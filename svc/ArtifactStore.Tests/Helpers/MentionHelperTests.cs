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
        private MentionHelper mentionHelper;
        private SqlConnectionWrapperMock cxn;

        [TestInitialize]
        public void init()
        {
            cxn = new SqlConnectionWrapperMock();
            userRepository = new SqlUserRepositoryMock();
            instanceSettingsRepository = new SqlInstanceSettingsRepositoryMock();
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

    }
}
