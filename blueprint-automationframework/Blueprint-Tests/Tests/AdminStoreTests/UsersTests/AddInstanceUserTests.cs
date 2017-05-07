using System;
using CustomAttributes;
using Helper;
using Model;
using Model.Common.Enums;
using Model.Impl;
using NUnit.Framework;
using TestCommon;
using Utilities;
using Utilities.Factories;

namespace AdminStoreTests.UsersTests
{
    [TestFixture]
    [Category(Categories.AdminPortal)]
    [Category(Categories.AdminStore)]

    public class AddInstanceUserTests : TestBase
    {
        private const string USER_PATH = RestPaths.Svc.AdminStore.Users.USERS_id_;

        private IUser _adminUser = null;

        #region Setup and Cleanup

        [SetUp]
        public void SetUp()
        {
            Helper = new TestHelper();
            _adminUser = Helper.CreateUserAndAuthenticate(TestHelper.AuthenticationTokenTypes.AccessControlToken);
        }

        [TearDown]
        public void TearDown()
        {
            Helper?.Dispose();
        }

        #endregion Setup and Cleanup


        #region 201 Created Tests

        [TestCase]
        [Description("Create and add an instance user. Get the added user using the id of the user that was just created. " +
             "Verify the same user that was created is returned.")]
        [TestRail(303340)]
        public void AddInstanceUser_ValidUser_UserCreated()
        {
            // Setup:
            var createdUser = AdminStoreHelper.GenerateRandomInstanceUser();

            // Execute:
            int createdUserId = 0;

            Assert.DoesNotThrow(() =>
            {
                createdUserId = Helper.AdminStore.AddUser(_adminUser, createdUser);
            }, "'POST {0}' should return 201 OK for a valid session token!", USER_PATH);

            InstanceUser addedUser = null;
            
            Assert.DoesNotThrow(() =>
            {
                addedUser = Helper.AdminStore.GetUserById(_adminUser, createdUserId);
            }, "'GET {0}' should return 200 OK for a valid session token!", USER_PATH);

            // Verify:

            Assert.AreEqual(createdUserId, addedUser.Id, "The added InstanceUser id {0} does not match the expected id {1}!", 
                addedUser.Id, createdUserId);

            Assert.AreEqual(createdUser.CurrentVersion + 1, addedUser.CurrentVersion, "The added InstanceUser should have a " +
                           "current version of {0} but the current version is {1}!", createdUser.CurrentVersion + 1, addedUser.CurrentVersion);

            AdminStoreHelper.AssertAreEqual(createdUser, addedUser);
        }

        [Explicit(IgnoreReasons.UnderDevelopmentQaDev)]
        [TestCase(null)]
        [TestCase(UserSource.Invalid)]
        [TestCase(UserSource.Unknown)]
        [TestCase(UserSource.Windows)]
        [Description("Create an instance user with a missing user source. Add the user. " +
             "Verify that the user is created as a Database user.")]
        [TestRail(303385)]
        public void AddInstanceUser_MissingOrInvalidSource_DatabaseUserCreated(UserSource source)
        {
            // Setup:
            var createdUser = AdminStoreHelper.GenerateRandomInstanceUser();
            createdUser.Source = source;

            // Execute:
            int createdUserId = 0;

            Assert.DoesNotThrow(() =>
            {
                createdUserId = Helper.AdminStore.AddUser(_adminUser, createdUser);
            }, "'POST {0}' should return 201 OK for a valid session token!", USER_PATH);

            InstanceUser addedUser = null;

            Assert.DoesNotThrow(() =>
            {
                addedUser = Helper.AdminStore.GetUserById(_adminUser, createdUserId);
            }, "'GET {0}' should return 200 OK for a valid session token!", USER_PATH);

            // Verify:

            Assert.AreEqual(createdUserId, addedUser.Id, "The added InstanceUser id {0} does not match the expected id {1}!",
                addedUser.Id, createdUserId);

            Assert.AreEqual(createdUser.CurrentVersion + 1, addedUser.CurrentVersion, "The added InstanceUser should have a " +
                           "current version of {0} but the current version is {1}!", createdUser.CurrentVersion + 1, addedUser.CurrentVersion);

            // Add Source to createdUser to verify that the addedUser returned as a Database user
            createdUser.Source = UserSource.Database;
            AdminStoreHelper.AssertAreEqual(createdUser, addedUser);
        }

        [Explicit(IgnoreReasons.UnderDevelopmentQaDev)]
        [TestCase(null)]
        [TestCase(LicenseLevel.Unknown)]
        [TestCase(LicenseLevel.Author)]
        [TestCase(LicenseLevel.Collaborator)]
        [Description("Create an instance user with a missing or invalid license level. Add the user. " +
             "Verify that the suer is created with Viewer license.")]
        [TestRail(303386)]
        public void AddInstanceUser_MissingOrInvalidLicenseLevel_UserCreatedWithViewerLicense(LicenseLevel licenseLevel)
        {
            // Setup:
            var createdUser = AdminStoreHelper.GenerateRandomInstanceUser();
            createdUser.LicenseType = licenseLevel;

            // Execute:
            int createdUserId = 0;

            Assert.DoesNotThrow(() =>
            {
                createdUserId = Helper.AdminStore.AddUser(_adminUser, createdUser);
            }, "'POST {0}' should return 201 OK for a valid session token!", USER_PATH);

            InstanceUser addedUser = null;

            Assert.DoesNotThrow(() =>
            {
                addedUser = Helper.AdminStore.GetUserById(_adminUser, createdUserId);
            }, "'GET {0}' should return 200 OK for a valid session token!", USER_PATH);

            // Verify:

            Assert.AreEqual(createdUserId, addedUser.Id, "The added InstanceUser id {0} does not match the expected id {1}!",
                addedUser.Id, createdUserId);

            Assert.AreEqual(createdUser.CurrentVersion + 1, addedUser.CurrentVersion, "The added InstanceUser should have a " +
                           "current version of {0} but the current version is {1}!", createdUser.CurrentVersion + 1, addedUser.CurrentVersion);

            // Add LicenseType of Viewer to createdUser to verify that the addedUser returned with Viewer license
            createdUser.LicenseType = LicenseLevel.Viewer;
            AdminStoreHelper.AssertAreEqual(createdUser, addedUser);
        }

        [TestCase(InstanceAdminRole.AssignInstanceAdministrators)]
        [TestCase(InstanceAdminRole.DefaultInstanceAdministrator)]
        [TestCase(InstanceAdminRole.ProvisionUsers)]
        [Description("Create an instance user.  Add the user with another user that has" +
             "permission to manage users. Verify that the user was created.")]
        [TestRail(303595)]
        public void AddInstanceUser_PermissionsToManageUsers_UserCreated(InstanceAdminRole adminRole)
        {
            // Setup:
            var userPermissionsToManageUsers = Helper.CreateUserAndAddToDatabase(adminRole);
            Helper.AdminStore.AddSession(userPermissionsToManageUsers);

            var createdUser = AdminStoreHelper.GenerateRandomInstanceUser();

            // Execute:
            int createdUserId = 0;

            Assert.DoesNotThrow(() =>
            {
                createdUserId = Helper.AdminStore.AddUser(userPermissionsToManageUsers, createdUser);
            }, "'POST {0}' should return 201 OK for a valid session token!", USER_PATH);

            InstanceUser addedUser = null;

            Assert.DoesNotThrow(() =>
            {
                addedUser = Helper.AdminStore.GetUserById(userPermissionsToManageUsers, createdUserId);
            }, "'GET {0}' should return 200 OK for a valid session token!", USER_PATH);

            // Verify:

            Assert.AreEqual(createdUserId, addedUser.Id, "The added InstanceUser id {0} does not match the expected id {1}!",
                addedUser.Id, createdUserId);

            Assert.AreEqual(createdUser.CurrentVersion + 1, addedUser.CurrentVersion, "The added InstanceUser should have a " +
                           "current version of {0} but the current version is {1}!", createdUser.CurrentVersion + 1, addedUser.CurrentVersion);

            AdminStoreHelper.AssertAreEqual(createdUser, addedUser);
        }

        #endregion 201 Created Tests

        #region 400 Bad Request Tests

        [TestCase]
        [Description("Create and add an instance user. Try to add the user a second time with the same login. " +
            "Verify that 400 Bad Request is returned.")]
        [TestRail(303375)]
        public void AddInstanceUser_UserAlreadyExists_400BadRequest()
        {
            // Setup:
            var createdUser = AdminStoreHelper.GenerateRandomInstanceUser();

            Helper.AdminStore.AddUser(_adminUser, createdUser);

            // Execute:
            var ex = Assert.Throws<Http400BadRequestException>(() => { Helper.AdminStore.AddUser(_adminUser, createdUser); },
                "POST {0}' should return 400 Bad Request!", USER_PATH);

            // Verify:
            TestHelper.ValidateServiceErrorMessage(ex.RestResponse, InstanceAdminErrorMessages.LoginNameUnique);
        }

        [TestCase("", Description = "Password is empty")]
        [TestCase(null, Description = "Password is null")]
        [Description("Create an instance user with a missing password. Try to add the user. " +
                     "Verify that 400 Bad Request is returned.")]
        [TestRail(303401)]
        public void AddInstanceUser_MissingPassword_400BadRequest(string password)
        {
            CreateDefaultInstanceUserWithInvalidPropertyVerify400BadRequest(
                _adminUser,
                "Password",
                password,
                InstanceAdminErrorMessages.PasswordMissing);
        }

        [Explicit(IgnoreReasons.UnderDevelopmentQaDev)]
        [TestCase]
        [Description("Create an instance user with an invalid password. Try to add the user. " +
                     "Verify that 400 Bad Request is returned.")]
        [TestRail(303400)]
        public static void AddInstanceUser_InvalidPassword_400BadRequest()
        {
            throw new NotImplementedException();
        }

        [TestCase ("", Description = "Login is empty")]
        [TestCase (null, Description = "Login is null")]
        [Description("Create an instance user with a missing login name. Try to add the user. " +
                     "Verify that 400 Bad Request is returned.")]
        [TestRail(303379)]
        public void AddInstanceUser_MissingLogin_400BadRequest(string login)
        {
            CreateDefaultInstanceUserWithInvalidPropertyVerify400BadRequest(
                _adminUser,
                "Login",
                login,
                InstanceAdminErrorMessages.LoginRequired);
        }

        [TestCase((uint)3, Description = "Minimum 4 characters")]
        [TestCase((uint)256, Description = "Maximum 255 characters")]
        [Description("Create an instance user with an invalid login name. Try to add the user. " +
                     "Verify that 400 Bad Request is returned.")]
        [TestRail(303380)]
        public void AddInstanceUser_InvalidLogin_400BadRequest(uint numCharacters)
        {
            CreateDefaultInstanceUserWithInvalidPropertyVerify400BadRequest(
                _adminUser,
                "Login",
                RandomGenerator.RandomAlphaNumericUpperAndLowerCase(numCharacters),
                InstanceAdminErrorMessages.LoginFieldLimitation);
        }

        [TestCase("", Description = "DisplayName is empty")]
        [TestCase(null, Description = "DisplayName is null")]
        [Description("Create an instance user with a missing display name. Try to add the user. " +
                     "Verify that 400 Bad Request is returned.")]
        [TestRail(303381)]
        public void AddInstanceUser_MissingDisplayName_400BadRequest(string displayName)
        {
            CreateDefaultInstanceUserWithInvalidPropertyVerify400BadRequest(
                _adminUser,
                "DisplayName",
                displayName,
                InstanceAdminErrorMessages.DisplayNameRequired);
        }

        [TestCase((uint)1, Description = "Minimum 2 characters")]
        [TestCase((uint)256, Description = "Maximum 255 characters")]
        [Description("Create an instance user with an invalid display name. Try to add the user. " +
             "Verify that 400 Bad Request is returned.")]
        [TestRail(303564)]
        public void AddInstanceUser_InvalidDisplayName_400BadRequest(uint numCharacters)
        {
            CreateDefaultInstanceUserWithInvalidPropertyVerify400BadRequest(
                _adminUser,
                "DisplayName",
                RandomGenerator.RandomAlphaNumericUpperAndLowerCase(numCharacters),
                InstanceAdminErrorMessages.DisplayNameFieldLimitation);
        }

        [TestCase((uint)3, Description = "Minimum 4 characters")]
        [TestCase((uint)256, Description = "Maximum 255 characters")]
        [Description("Create an instance user with an invalid email. Try to add the user. " +
                     "Verify that 400 Bad Request is returned.")]
        [TestRail(303382)]
        public  void AddInstanceUser_InvalidEmail_400BadRequest(uint numCharacters)
        {
            CreateDefaultInstanceUserWithInvalidPropertyVerify400BadRequest(
                _adminUser,
                "Email",
                RandomGenerator.RandomAlphaNumericUpperAndLowerCase(numCharacters),
                InstanceAdminErrorMessages.EmailFieldLimitation);
        }

        [TestCase((uint)1, Description = "Minimum 2 characters")]
        [TestCase((uint)256, Description = "Maximum 255 characters")]
        [Description("Create an instance user with an invalid first name. Try to add the user. " +
                     "Verify that 400 Bad Request is returned.")]
        [TestRail(303396)]
        public  void AddInstanceUser_InvalidFirstName_400BadRequest(uint numCharacters)
        {
            CreateDefaultInstanceUserWithInvalidPropertyVerify400BadRequest(
                _adminUser,
                "FirstName",
                RandomGenerator.RandomAlphaNumericUpperAndLowerCase(numCharacters),
                InstanceAdminErrorMessages.FirstNameFieldLimitation);
        }

        [TestCase("", Description = "FirstName is empty")]
        [TestCase(null, Description = "FirstName is null")]
        [Description("Create an instance user with a missing first name. Try to add the user. " +
                     "Verify that 400 Bad Request is returned.")]
        [TestRail(303580)]
        public void AddInstanceUser_MissingFirstName_400BadRequest(string firstName)
        {
            CreateDefaultInstanceUserWithInvalidPropertyVerify400BadRequest(
                _adminUser,
                "FirstName",
                firstName,
                InstanceAdminErrorMessages.FirstNameRequired);
        }

        [TestCase((uint)1, Description = "Minimum 2 characters")]
        [TestCase((uint)256, Description = "Maximum 255 characters")]
        [Description("Create an instance user with an invalid last name. Try to add the user. " +
                     "Verify that 400 Bad Request is returned.")]
        [TestRail(303397)]
        public void AddInstanceUser_InvalidLastName_400BadRequest(uint numCharacters)
        {
            CreateDefaultInstanceUserWithInvalidPropertyVerify400BadRequest(
                _adminUser,
                "LastName",
                RandomGenerator.RandomAlphaNumericUpperAndLowerCase(numCharacters),
                InstanceAdminErrorMessages.LastNameFieldLimitation);
        }

        [TestCase("", Description = "LastName is empty")]
        [TestCase(null, Description = "LastName is null")]
        [Description("Create an instance user with a missing last name. Try to add the user. " +
                     "Verify that 400 Bad Request is returned.")]
        [TestRail(303581)]
        public void AddInstanceUser_MissingLastName_400BadRequest(string lastName)
        {
            CreateDefaultInstanceUserWithInvalidPropertyVerify400BadRequest(
                _adminUser,
                "LastName",
                lastName,
                InstanceAdminErrorMessages.LastNameRequired);
        }

        [TestCase((uint)256, Description = "Maximum 255 characters")]
        [Description("Create an instance user with an invalid department. Try to add the user. " +
                     "Verify that 400 Bad Request is returned.")]
        [TestRail(303398)]
        public void AddInstanceUser_InvalidDepartment_400BadRequest(uint numCharacters)
        {
            CreateDefaultInstanceUserWithInvalidPropertyVerify400BadRequest(
                _adminUser,
                "Department",
                RandomGenerator.RandomAlphaNumericUpperAndLowerCase(numCharacters),
                InstanceAdminErrorMessages.DepartmentFieldLimitation);
        }

        [TestCase((uint)1, Description = "Minimum 2 characters")]
        [TestCase((uint)256, Description = "Maximum 255 characters")]
        [Description("Create an instance user with an invalid title. Try to add the user. " +
                     "Verify that 400 Bad Request is returned.")]
        [TestRail(303399)]
        public  void AddInstanceUser_InvalidTitle_400BadRequest(uint numCharacters)
        {
            CreateDefaultInstanceUserWithInvalidPropertyVerify400BadRequest(
                _adminUser,
                "Title",
                RandomGenerator.RandomAlphaNumericUpperAndLowerCase(numCharacters),
                InstanceAdminErrorMessages.TitleFieldLimitation);
        }

        #endregion 400 Bad Request Tests

        #region 401 Unauthorized Tests

        [TestCase("")]
        [TestCase("invalidTokenString")]
        [Description("Create and add an instance user with an invalid token header. " +
                     "Verify that 401 Unauthorized is returned.")]
        [TestRail(30373)]
        public  void AddInstanceUser_InvalidTokenHeader_401Unauthorized(string tokenString)
        {
            // Setup:
            var createdUser = AdminStoreHelper.GenerateRandomInstanceUser();
            var userWithInvalidTokenHeader = Helper.CreateUserAndAuthenticate(TestHelper.AuthenticationTokenTypes.AccessControlToken, 
                InstanceAdminRole.DefaultInstanceAdministrator);
            userWithInvalidTokenHeader.SetToken(tokenString);

            // Execute & Verify:
            Assert.Throws<Http401UnauthorizedException>(() =>
            {
                Helper.AdminStore.AddUser(userWithInvalidTokenHeader, createdUser);
            }, "'POST {0}' should return 401 Unauthorized with invalid token header!", USER_PATH);
        }

        #endregion 401 Unauthorized Tests

        #region 403 Forbidden Tests

        [TestCase(null)]
        [TestCase(InstanceAdminRole.AdministerALLProjects)]
        [TestCase(InstanceAdminRole.BlueprintAnalytics)]
        [TestCase(InstanceAdminRole.Email_ActiveDirectory_SAMLSettings)]
        [TestCase(InstanceAdminRole.InstanceStandardsManager)]
        [TestCase(InstanceAdminRole.LogGatheringAndLicenseReporting)]
        [TestCase(InstanceAdminRole.ManageAdministratorRoles)]
        [TestCase(InstanceAdminRole.ProvisionProjects)]
        [Description("Create an instance user.  Try to add the user with another user that does not have" +
                     "permission to manage users. Verify that 401 Unauthorized is returned.")]
        [TestRail(303374)]
        public void AddInstanceUser_NoPermissionsToManageUsers_403Forbidden(InstanceAdminRole? adminRole)
        {
            // Setup:
            var userWithNoPermissionsToManageUsers = Helper.CreateUserAndAddToDatabase(adminRole);
            Helper.AdminStore.AddSession(userWithNoPermissionsToManageUsers);

            var createdUser = AdminStoreHelper.GenerateRandomInstanceUser();

            // Execute & Verify:
            Assert.Throws<Http403ForbiddenException>(() =>
            {
                Helper.AdminStore.AddUser(userWithNoPermissionsToManageUsers, createdUser);
            },
            "'POST {0}' should return 403 Forbidden when the user adding the created user has no permissions to manage users!", USER_PATH);
        }

        #endregion 403 Forbidden Tests

        #region Private Methods

        /// <summary>
        /// Creates a default Instance User with an invalid property and verifies 400 Bad Request when trying to add the user.
        /// </summary>
        /// <param name="adminUser">The user adding the created user.</param>
        /// <param name="property">The property of the created user to be made invalid.</param>
        /// <param name="propertyValue">The invalid property value.</param>
        /// <param name="expectedErrorMessage">The expected error message returned from the server.</param>
        private void CreateDefaultInstanceUserWithInvalidPropertyVerify400BadRequest(
            IUser adminUser,
            string property,
            object propertyValue,
            string expectedErrorMessage)
        {
            // Setup:
            var createdUser = AdminStoreHelper.GenerateRandomInstanceUser();

            if (property != null)
            {
                CSharpUtilities.SetProperty(property, propertyValue, createdUser);
            }

            // Execute:
            var ex = Assert.Throws<Http400BadRequestException>(() => { Helper.AdminStore.AddUser(adminUser, createdUser); },
                "POST {0}' should return 400 Bad Request!", USER_PATH);

            // Verify:
            TestHelper.ValidateServiceErrorMessage(ex.RestResponse, expectedErrorMessage);
        }

        #endregion Private Methods
    }
}
