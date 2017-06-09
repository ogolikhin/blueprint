using NUnit.Framework;
using CustomAttributes;
using Model;
using Helper;
using TestCommon;
using Model.NovaModel.AdminStoreModel;
using Model.Factories;
using Utilities;

namespace AdminStoreTests
{
    [TestFixture]
    [Category(Categories.AdminPortal)]
    [Category(Categories.AdminStore)]
    [Category(Categories.CannotRunInParallel)]
    public class FederatedAuthenticationTests : TestBase // TODO : delete this file and distribute test cases between other files to keep one endpoint under the test per file logic
    {
        private IUser _defaultAdmin;
        private IUser _nonAdmin;
        private IUser _usersAdmin;
        private bool _isFederatedAuthenticationEnabled;

        [SetUp]
        public void SetUp()
        {
            Helper = new TestHelper();
            _isFederatedAuthenticationEnabled = Helper.BlueprintServer.GetIsFedAuthenticationEnabledDB();
            _usersAdmin = Helper.CreateUserAndAuthenticate(TestHelper.AuthenticationTokenTypes.AccessControlToken,
                Model.Common.Enums.InstanceAdminRole.ProvisionUsers);
        }

        [TearDown]
        public void TearDown()
        {
            Helper.BlueprintServer.SetIsFedAuthenticationEnabledDB(_isFederatedAuthenticationEnabled);
            Helper?.Dispose();
        }

        [TestCase(true)]
        [TestCase(false)]
        [TestRail(308939)]
        [Description("Set FedAuthenticationEnabled value in DB, get it from svc/adminstore/config/users and compare with expectations.")]
        public void GetUserManagementSettings_EnableOrDisableFederatedAuthentication_CheckFederatedAuthenticationSetting(bool isFederatedAuthenticationEnabled)
        {
            UserManagementSettings userManagementSettings = null;
            Helper.BlueprintServer.SetIsFedAuthenticationEnabledDB(isFederatedAuthenticationEnabled);

            // Execute:
            Assert.DoesNotThrow(() =>
            {
                userManagementSettings = Helper.AdminStore.GetUserManagementSettings(_usersAdmin);
            }, "GetUserManagementSettings() should return 200 OK for a valid user!");

            // Verify:
            Assert.NotNull(userManagementSettings, "GetUserManagementSettings() returned null!");
            Assert.AreEqual(isFederatedAuthenticationEnabled, userManagementSettings.IsFederatedAuthenticationEnabled,
                "FederatedAuthenticationEnabled should have expected value.");
        }

        [TestCase(true)]
        [TestCase(false)]
        [Description("Set FedAuthenticationEnabled to true in DB. Create and add an instance user without password. " +
            "Get the added user using an admin user. Verify the same user that was created is returned.")]
        [TestRail(308940)] // TODO : add test 
        public void AddInstanceUser_WithWithoutPassword_FederatedAuthenticationEnabled_UserCreated(bool isPasswordNull)
        {
            // Setup:
            var createdUser = AdminStoreHelper.GenerateRandomInstanceUser();
            if (isPasswordNull)
            {
                createdUser.Password = null;
            }

            Helper.BlueprintServer.SetIsFedAuthenticationEnabledDB(isFedAuthenticationEnabledDB: true);

            // Execute:
            Assert.DoesNotThrow(() =>
            {
                createdUser.Id = Helper.AdminStore.AddUser(_usersAdmin, createdUser);
            }, "AddUser should return 201 Created!");

            // Verify:
            // Update Id and CurrentVersion in CreatedUser for comparison
            AdminStoreHelper.UpdateUserIdAndIncrementCurrentVersion(createdUser, createdUser.Id);
            var addedUser = Helper.AdminStore.GetUserById(_usersAdmin, createdUser.Id);

            AdminStoreHelper.AssertAreEqual(createdUser, addedUser);
        }

        [TestCase]
        [TestRail(308941)]
        [Description("Try to get UserManagementSettings from svc/adminstore/config/users for user without Admin role. " +
            "Call should return 403.")]
        public void GetUserManagementSettings_NonAdminUser_Returns403()
        {
            _defaultAdmin = Helper.CreateUserAndAuthenticate(TestHelper.AuthenticationTokenTypes.BothAccessControlAndOpenApiTokens,
                Model.Common.Enums.InstanceAdminRole.DefaultInstanceAdministrator);
            var project = ProjectFactory.GetProject(_defaultAdmin);
            _nonAdmin = Helper.CreateUserWithProjectRolePermissions(TestHelper.ProjectRole.AuthorFullAccess, project);

            // Execute:
            var ex = Assert.Throws<Http403ForbiddenException>(() =>
            {
                Helper.AdminStore.GetUserManagementSettings(_nonAdmin);
            }, "GetUserManagementSettings() should return 403 for non-admin user!");

            // Verify:
            string errorMessage = "Exception of type 'ServiceLibrary.Exceptions.AuthorizationException' was thrown.";
            TestHelper.ValidateServiceErrorMessage(ex.RestResponse, errorMessage);
        }
    }
}
