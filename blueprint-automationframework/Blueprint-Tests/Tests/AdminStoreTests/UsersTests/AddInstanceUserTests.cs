using Common;
using CustomAttributes;
using Helper;
using Model;
using Model.Common.Enums;
using Model.NovaModel.AdminStoreModel;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Net;
using TestCommon;
using Utilities;
using Utilities.Facades;
using Utilities.Factories;

namespace AdminStoreTests.UsersTests
{
    [TestFixture]
    [Category(Categories.AdminPortal)]
    [Category(Categories.AdminStore)]

    public class AddInstanceUserTests : TestBase
    {
        private const string USER_PATH = RestPaths.Svc.AdminStore.Users.USERS;
        private const string USER_PATH_ID = RestPaths.Svc.AdminStore.Users.USERS_id_;

        private const string PASSWORD_EXPIRATION_IN_DAYS = "PasswordExpirationInDays";

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
            Helper.DeleteInstanceUsers(_adminUser);

            Helper?.Dispose();
        }

        #endregion Setup and Cleanup

        // TODO: Add test that adds a user with the login name of a deleted user
        // TODO: Add edge case tests for field and password validation?
        // TODO: Verify license type calculation is performed correctly
        // TODO: Add test for creation of Guest user
        // TODO: Add test for SSO

        #region 201 Created Tests

        [TestCase]
        [Description("Create and add an instance user. Get the added user using using an admin user. " +
                     "Verify the same user that was created is returned.")]
        [TestRail(303340)]
        public void AddInstanceUser_ValidUser_UserCreated()
        {
            // Setup:
            var createdUser = AdminStoreHelper.GenerateRandomInstanceUser();

            // Execute:
            Assert.DoesNotThrow(() =>
            {
                createdUser.Id = Helper.AdminStore.AddUser(_adminUser, createdUser);
            }, "'POST {0}' should return 201 Created for a valid session token!", USER_PATH);

            InstanceUser addedUser = null;

            Assert.DoesNotThrow(() =>
            {
                addedUser = Helper.AdminStore.GetUserById(_adminUser, createdUser.Id);
            }, "'GET {0}' should return 200 OK for a valid session token!", USER_PATH_ID);

            // Verify:
            // Update Id and CurrentVersion in CreatedUser for comparison
            AdminStoreHelper.UpdateUserIdAndIncrementCurrentVersion(createdUser, createdUser.Id);

            AdminStoreHelper.AssertAreEqual(createdUser, addedUser);
        }

        [TestCase]
        [Description("Create an instance user with a missing user source. Add the user. " +
                     "Verify that the user is created as a Database user.")]
        [TestRail(303385)]
        public void AddInstanceUser_MissingSource_DatabaseUserCreated()
        {
            // Setup:
            var createdUser = AdminStoreHelper.GenerateRandomInstanceUser();

            // Source defaults to Database, so we need to remove it here
            createdUser.Source = null;

            // Execute:
            int createdUserId = 0;

            Assert.DoesNotThrow(() =>
            {
                createdUserId = Helper.AdminStore.AddUser(_adminUser, createdUser);
            }, "'POST {0}' should return 201 Created for a valid session token!", USER_PATH);

            InstanceUser addedUser = null;

            Assert.DoesNotThrow(() =>
            {
                addedUser = Helper.AdminStore.GetUserById(_adminUser, createdUserId);
            }, "'GET {0}' should return 200 OK for a valid session token!", USER_PATH_ID);

            // Verify:
            // Update Id and CurrentVersion in CreatedUser for comparison
            AdminStoreHelper.UpdateUserIdAndIncrementCurrentVersion(createdUser, createdUserId);

            // Add Source to createdUser to verify that the addedUser returned as a Database user
            createdUser.Source = UserGroupSource.Database;
            AdminStoreHelper.AssertAreEqual(createdUser, addedUser);
        }

        [TestCase(null)]
        [TestCase(LicenseLevel.Unknown)]
        [TestCase(LicenseLevel.Author)]
        [TestCase(LicenseLevel.Collaborator)]
        [Description("Create an instance user with a missing or invalid license level. Add the user. " +
                     "Verify that the user is created with Viewer license.")]
        [TestRail(303386)]
        public void AddInstanceUser_MissingOrInvalidLicenseLevel_UserCreatedWithViewerLicense(LicenseLevel? licenseLevel)
        {
            // Setup:
            var createdUser = AdminStoreHelper.GenerateRandomInstanceUser();

            // Source defaults to Database, so we need to remove it here
            createdUser.LicenseType = licenseLevel;

            // Execute:
            int createdUserId = 0;

            Assert.DoesNotThrow(() =>
            {
                createdUserId = Helper.AdminStore.AddUser(_adminUser, createdUser);
            }, "'POST {0}' should return 201 Created for a valid session token!", USER_PATH);

            InstanceUser addedUser = null;

            Assert.DoesNotThrow(() =>
            {
                addedUser = Helper.AdminStore.GetUserById(_adminUser, createdUserId);
            }, "'GET {0}' should return 200 OK for a valid session token!", USER_PATH_ID);

            // Verify:
            // Update Id and CurrentVersion in CreatedUser for comparison
            AdminStoreHelper.UpdateUserIdAndIncrementCurrentVersion(createdUser, createdUserId);

            // Add LicenseType of Viewer to createdUser to verify that the addedUser returned with Viewer license
            createdUser.LicenseType = LicenseLevel.Viewer;
            AdminStoreHelper.AssertAreEqual(createdUser, addedUser);
        }

        [TestCase("FirstName", "", Description = "FirstName is empty")]
        [TestCase("FirstName", null, Description = "FirstName is null")]
        [TestCase("LastName", "", Description = "LastName is empty")]
        [TestCase("LastName", null, Description = "LastName is null")]
        [TestCase("Title", "", Description = "Title is empty")]
        [TestCase("Title", null, Description = "Title is null")]
        [TestCase("Department", "", Description = "Department is empty")]
        [TestCase("Department", null, Description = "Department is null")]
        [TestCase("Email", "", Description = "Email is empty")]
        [TestCase("Email", null, Description = "Email is null")]
        [Description("Create an instance user with a property that can be missing or null. Add the user. " +
                     "Verify that the user is created.")]
        [TestRail(303989)]
        public void AddInstanceUser_TextPropertyThatCanBeEmptyOrNull_UserCreated(string property, string propertyValue)
        {
            // Setup:
            var createdUser = AdminStoreHelper.GenerateRandomInstanceUser();

            CSharpUtilities.SetProperty(property, propertyValue, createdUser);

            // Execute:
            int createdUserId = 0;

            Assert.DoesNotThrow(() =>
            {
                createdUserId = Helper.AdminStore.AddUser(_adminUser, createdUser);
            }, "'POST {0}' should return 201 Created for a valid session token!", USER_PATH);

            InstanceUser addedUser = null;

            Assert.DoesNotThrow(() =>
            {
                addedUser = Helper.AdminStore.GetUserById(_adminUser, createdUserId);
            }, "'GET {0}' should return 200 OK for a valid session token!", USER_PATH_ID);

            // Verify:
            // Update Id and CurrentVersion in CreatedUser for comparison
            AdminStoreHelper.UpdateUserIdAndIncrementCurrentVersion(createdUser, createdUserId);

            // Missing or null user will always return an empty string for the property, so update here for comparison
            CSharpUtilities.SetProperty(property, "", createdUser);

            // Add LicenseType of Viewer to createdUser to verify that the addedUser returned with Viewer license
            createdUser.LicenseType = LicenseLevel.Viewer;

            AdminStoreHelper.AssertAreEqual(createdUser, addedUser);
        }

        [TestCase]
        [Description("Create a default instance user.  Add the user with another user that only has " +
                     "permission to manage users.  Verify that the user was created.")]
        [TestRail(303595)]
        public void AddInstanceUser_PermissionsToManageUsers_UserCreated()
        {
            using (var adminStoreHelper = new AdminStoreHelper())
            {
                // Setup:
                var adminRole = adminStoreHelper.AddInstanceAdminRoleToDatabase(InstanceAdminPrivileges.ManageUsers);

                var userPermissionsToManageUsers = Helper.CreateUserAndAuthenticate(
                    TestHelper.AuthenticationTokenTypes.AccessControlToken, adminRole);

                var createdUser = AdminStoreHelper.GenerateRandomInstanceUser();

                // Execute:
                Assert.DoesNotThrow(() =>
                {
                    createdUser.Id = Helper.AdminStore.AddUser(userPermissionsToManageUsers, createdUser);
                }, "'POST {0}' should return 201 Created for a valid session token!", USER_PATH);

                InstanceUser addedUser = null;

                Assert.DoesNotThrow(() =>
                {
                    addedUser = Helper.AdminStore.GetUserById(_adminUser, createdUser.Id);
                }, "'GET {0}' should return 200 OK for a valid session token!", USER_PATH_ID);

                // Verify:
                // Update Id and CurrentVersion in CreatedUser for comparison
                AdminStoreHelper.UpdateUserIdAndIncrementCurrentVersion(createdUser, createdUser.Id);

                AdminStoreHelper.AssertAreEqual(createdUser, addedUser);
            }
        }

        [TestCase(InstanceAdminRole.AssignInstanceAdministrators, LicenseLevel.Viewer)]
        [TestCase(InstanceAdminRole.DefaultInstanceAdministrator, LicenseLevel.Author)]
        [TestCase(InstanceAdminRole.ProvisionUsers, LicenseLevel.Viewer)]
        [TestCase(InstanceAdminRole.AdministerALLProjects, LicenseLevel.Collaborator)]
        [TestCase(InstanceAdminRole.BlueprintAnalytics, LicenseLevel.Viewer)]
        [TestCase(InstanceAdminRole.Email_ActiveDirectory_SAMLSettings, LicenseLevel.Viewer)]
        [TestCase(InstanceAdminRole.InstanceStandardsManager, LicenseLevel.Collaborator)]
        [TestCase(InstanceAdminRole.LogGatheringAndLicenseReporting, LicenseLevel.Viewer)]
        [TestCase(InstanceAdminRole.ManageAdministratorRoles, LicenseLevel.Viewer)]
        [TestCase(InstanceAdminRole.ProvisionProjects, LicenseLevel.Collaborator)]
        [Description("Create an instance user with an admin role.  Add the user using another user that has " +
                     "permission to assign admin roles. Verify that the user was created.")]
        [TestRail(303705)]
        public void AddInstanceUser_AssignInstanceAdminRole_UserCreated(InstanceAdminRole adminRole, LicenseLevel expectedLicenseLevel)
        {
            // Setup:
            var userWithPermissionsToAssignAdminRoles = Helper.CreateUserAndAuthenticate(
                TestHelper.AuthenticationTokenTypes.AccessControlToken, InstanceAdminRole.AssignInstanceAdministrators);

            var createdUser = AdminStoreHelper.GenerateRandomInstanceUser(instanceAdminRole: adminRole);

            // Execute:
            Assert.DoesNotThrow(() =>
            {
                createdUser.Id = Helper.AdminStore.AddUser(userWithPermissionsToAssignAdminRoles, createdUser);
            }, "'POST {0}' should return 201 Created for a valid session token!", USER_PATH);

            InstanceUser addedUser = null;

            Assert.DoesNotThrow(() =>
            {
                addedUser = Helper.AdminStore.GetUserById(_adminUser, createdUser.Id);
            }, "'GET {0}' should return 200 OK for a valid session token!", USER_PATH_ID);

            // Verify:
            // Update License Type for comparison
            createdUser.LicenseType = expectedLicenseLevel;

            // Update Id and CurrentVersion in CreatedUser for comparison
            AdminStoreHelper.UpdateUserIdAndIncrementCurrentVersion(createdUser, createdUser.Id);

            AdminStoreHelper.AssertAreEqual(createdUser, addedUser);
        }

        [TestCase(InstanceAdminRole.AssignInstanceAdministrators, LicenseLevel.Viewer)]
        [TestRail(308877)]
        [Description("Create an instance user with a role when Password Expiration is enabled by setting its value to '1' " + 
            "and Verify that Password gets expired based on the value set on PasswordExpirationIndays from Instances table.")]
        public void AddInstanceUser_AssigningRoleWhenPasswordExpirationEnabled_VerifyUserPasswordExpirationBasedOnPasswordExpirationInDays(
            InstanceAdminRole adminRole,
            LicenseLevel expectedLicenseLevel
            )
        {
            // Setup: Set the user that has expirable password
            var userWithPermissionsToAssignAdminRoles = Helper.CreateUserAndAuthenticate(TestHelper.AuthenticationTokenTypes.AccessControlToken, InstanceAdminRole.AssignInstanceAdministrators);

            var createdUser = AdminStoreHelper.GenerateRandomInstanceUser(
                instanceAdminRole: adminRole,
                expirePassword: true);

            // Enable the Instance password expiration feature by setting the value to '1' 
            var originalPasswordExpirationInDays = TestHelper.GetValueFromInstancesTable(PASSWORD_EXPIRATION_IN_DAYS);

            try
            {
                // Execution: Add and Get Instance Users
                Assert.DoesNotThrow(() =>
                {
                    createdUser.Id = Helper.AdminStore.AddUser(userWithPermissionsToAssignAdminRoles, createdUser);
                }, "'POST {0}' should return 201 Created for a valid session token!", USER_PATH);


                // Verify: Update License Type for comparison
                createdUser.LicenseType = expectedLicenseLevel;

                var addedUser = Helper.AdminStore.GetUserById(_adminUser, createdUser.Id);

                // Update Id and CurrentVersion in CreatedUser for comparison
                AdminStoreHelper.UpdateUserIdAndIncrementCurrentVersion(createdUser, createdUser.Id);

                AdminStoreHelper.AssertAreEqual(createdUser, addedUser);

                ValidateUserPasswordExpiration(createdUser);
            }
            finally
            {
                // Restore PasswordExpirationInDays back to original value.
                TestHelper.UpdateValueFromInstancesTable(PASSWORD_EXPIRATION_IN_DAYS, originalPasswordExpirationInDays);
            }
        }

        [TestCase]
        [Description("Verify that a user who was created with 'enabled = False' is unable to log in.")]
        [TestRail(308851)]
        public void AddInstanceUser_LoginDisabled_UserCannotLogIn()
        {
            // Setup: prepare user to be created
            var createdUser = AdminStoreHelper.GenerateRandomInstanceUser();
            createdUser.Enabled = false;

            // Execute: add the user
            Assert.DoesNotThrow(() =>
            {
                createdUser.Id = Helper.AdminStore.AddUser(_adminUser, createdUser);
            }, "'POST {0}' should return 201 CREATED!", RestPaths.Svc.AdminStore.Users.USERS);

            // Verify: that the user cannot log in
            var ex = Assert.Throws<Http401UnauthorizedException>(() =>
            {
               Helper.AdminStore.AddSession(createdUser.Login, createdUser.Password);
            }, "User should not be able to log in!");

            string expectedMessage = I18NHelper.FormatInvariant("User account is locked out for the login: {0}", createdUser.Login);
            TestHelper.ValidateServiceError(ex.RestResponse, ErrorCodes.AccountIsLocked, expectedMessage);

        }

        #endregion 201 Created Tests

        #region 400 Bad Request Tests

        [TestCase]
        [Description("Create an instance user. Try to add the user without sending the user object. " +
                     "Verify that 400 Bad Request is returned.")]
        [TestRail(303654)]
        public void AddInstanceUser_UserMissing_400BadRequest()
        {
            // Setup & Execute:
            var ex = Assert.Throws<Http400BadRequestException>(() => { Helper.AdminStore.AddUser(_adminUser, null); },
                "'POST {0}' should return 400 Bad Request!", USER_PATH);

            // Verify:
            TestHelper.ValidateServiceErrorMessage(ex.RestResponse, InstanceAdminErrorMessages.UserModelIsEmpty);
        }

        [TestCase]
        [Description("Create and add an instance user. Try to add a second user with the same login. " +
                     "Verify that 400 Bad Request is returned.")]
        [TestRail(303375)]
        public void AddInstanceUser_UserAlreadyExists_400BadRequest()
        {
            // Setup:
            var createdUser = Helper.CreateAndAddInstanceUser(_adminUser);

            var newUser = AdminStoreHelper.GenerateRandomInstanceUser();
            newUser.Login = createdUser.Login;

            // Execute:
            var ex = Assert.Throws<Http400BadRequestException>(() => { Helper.AdminStore.AddUser(_adminUser, newUser); },
                "'POST {0}' should return 400 Bad Request!", USER_PATH);

            // Verify:
            TestHelper.ValidateServiceErrorMessage(ex.RestResponse, InstanceAdminErrorMessages.LoginNameUnique);
        }

        [TestCase((UserGroupSource)0xFF, InstanceAdminErrorMessages.CreateOnlyDatabaseUsers)]
        [TestCase(UserGroupSource.Unknown, InstanceAdminErrorMessages.CreateOnlyDatabaseUsers)]
        [TestCase(UserGroupSource.Windows, InstanceAdminErrorMessages.CreateOnlyDatabaseUsers)]
        [Description("Create an instance user with an invalid user source. Try to add the user. " +
                     "Verify that 400 Bad Request is returned.")]
        [TestRail(303606)]
        public void AddInstanceUser_InvalidSource_400BadRequest(UserGroupSource? source, string errorMessage)
        {
            CreateDefaultInstanceUserWithInvalidPropertyVerify400BadRequest(
                _adminUser,
                "Source",
                source,
                errorMessage);
        }

        [TestCase("", InstanceAdminErrorMessages.PasswordMissing, Description = "Empty Password")]
        [TestCase(null, InstanceAdminErrorMessages.PasswordMissing, Description = "Null Password")]
        [TestCase("$Blue99", InstanceAdminErrorMessages.PasswordInvalidLength, Description = "Password less that 8 characters")]
        [TestCase("$Blue9999999999999999999999999999999999999999999999999999999999999999999999999999999999999999999999999999999999999999999999999999", 
            InstanceAdminErrorMessages.PasswordInvalidLength, Description = "Password greater than 128 characters")]
        [TestCase("Blueprint99", InstanceAdminErrorMessages.PasswordDoesNotHaveNonAlphanumeric, Description = "Password has no non-alphanumeric character")]
        [TestCase("$Blueprint", InstanceAdminErrorMessages.PasswordDoesNotHaveNumber, Description = "Password has no numeric character")]
        [TestCase("$blueprint99", InstanceAdminErrorMessages.PasswordDoesNotHaveUpperCase, Description = "Password has no uppercase character")]
        [Description("Create an instance user with a missing or invalid password. Try to add the user. " +
                     "Verify that 400 Bad Request is returned.")]
        [TestRail(303400)]
        public void AddInstanceUser_MissingOrInvalidPassword_400BadRequest(string password, string errorMessage)
        {
            CreateDefaultInstanceUserWithInvalidPropertyVerify400BadRequest(
                _adminUser,
                "Password",
                password,
                errorMessage);
        }

        [TestCase("Login", InstanceAdminErrorMessages.PasswordSameAsLogin, Description = "Password the same as Login")]
        [TestCase("DisplayName", InstanceAdminErrorMessages.PasswordSameAsDisplayName, Description = "Password the same as DisplayName")]
        [Description("Create instance user with Password the same as Login or DisplayName. Try to add the user. " +
                     "Verify that 400 Bad Request is returned.")]
        [TestRail(303616)]
        public void AddInstanceUser_PasswordSameAsLoginOrDisplayName_400BadRequest(string property, string errorMessage)
        {
            // Setup:
            var createdUser = AdminStoreHelper.GenerateRandomInstanceUser();

            CSharpUtilities.SetProperty(property, createdUser.Password, createdUser);

            // Execute:
            var ex = Assert.Throws<Http400BadRequestException>(() => { Helper.AdminStore.AddUser(_adminUser, createdUser); },
                "'POST {0}' should return 400 Bad Request!", USER_PATH);

            // Verify:
            TestHelper.ValidateServiceErrorMessage(ex.RestResponse, errorMessage);
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

        [TestCase("Raptor_RC_ExpiredUser", Description = "Raptor_RC_ExpiredUser")]
        [TestCase("Raptor_RC_UserLogout", Description = "Raptor_RC_UserLogout")]
        [TestCase("Raptor_RC_InvalidUser", Description = "Raptor_RC_InvalidUser")]
        [Description("Create an instance user with an reserved login name. Try to add the user. " +
                     "Verify that 400 Bad Request is returned.")]
        [TestRail(303676)]
        public void AddInstanceUser_ReservedLogin_400BadRequest(string reservedLogin)
        {
            CreateDefaultInstanceUserWithInvalidPropertyVerify400BadRequest(
                _adminUser,
                "Login",
                reservedLogin,
                InstanceAdminErrorMessages.LoginInvalid);
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

        [TestCase((uint)3, InstanceAdminErrorMessages.EmailFieldLimitation, Description = "Minimum 4 characters")]
        [TestCase((uint)256, InstanceAdminErrorMessages.EmailFieldLimitation, Description = "Maximum 255 characters")]
        [TestCase("@domain.com", InstanceAdminErrorMessages.EmailFormatIncorrect, Description = "No local part")]
        [TestCase("domain.com", InstanceAdminErrorMessages.EmailFormatIncorrect, Description = "No @ character")]
        [TestCase("user@name@domain.com", InstanceAdminErrorMessages.EmailFormatIncorrect, Description = "Only one @ allowed")]
        [TestCase("user@domain..com", InstanceAdminErrorMessages.EmailFormatIncorrect, Description = "Double dot after @")]
        [TestCase("user\"Name\"@domain.com", InstanceAdminErrorMessages.EmailFormatIncorrect, Description = "Quotes must be dot separated")]
        [TestCase("user name@domain.com", InstanceAdminErrorMessages.EmailFormatIncorrect, Description = "No spaces")]
        [TestCase("user\\name@domain.com", InstanceAdminErrorMessages.EmailFormatIncorrect, Description = "No backslashes")]
        [TestCase("user(name@domain.com", InstanceAdminErrorMessages.EmailFormatIncorrect, Description = "No left parenthesis")]
        [TestCase("user)name@domain.com", InstanceAdminErrorMessages.EmailFormatIncorrect, Description = "No right parenthesis")]
        [TestCase("user,name@domain.com", InstanceAdminErrorMessages.EmailFormatIncorrect, Description = "No comma")]
        [TestCase("user:name@domain.com", InstanceAdminErrorMessages.EmailFormatIncorrect, Description = "No colon")]
        [TestCase("user;name@domain.com", InstanceAdminErrorMessages.EmailFormatIncorrect, Description = "No semi-colon")]
        [TestCase("user<name@domain.com", InstanceAdminErrorMessages.EmailFormatIncorrect, Description = "No less than")]
        [TestCase("user>name@domain.com", InstanceAdminErrorMessages.EmailFormatIncorrect, Description = "No greater than")]
        [TestCase("user[name@domain.com", InstanceAdminErrorMessages.EmailFormatIncorrect, Description = "No left bracket")]
        [TestCase("user]name@domain.com", InstanceAdminErrorMessages.EmailFormatIncorrect, Description = "No right bracket")]
        [Description("Create an instance user with an invalid email. Try to add the user. " +
                     "Verify that 400 Bad Request is returned.")]
        [TestRail(303382)]
        public  void AddInstanceUser_InvalidEmail_400BadRequest(object value, string errorMessage)
        {
            string emailAddress;

            if (value is uint)
            {
                emailAddress = RandomGenerator.RandomAlphaNumericUpperAndLowerCase((uint) value);
            }
            else
            {
                emailAddress = (string) value;
            }

            CreateDefaultInstanceUserWithInvalidPropertyVerify400BadRequest(
                _adminUser,
                "Email",
                emailAddress,
                errorMessage
                );
        }

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

        [TestCase((uint)256, Description = "Maximum 255 characters")]
        [Description("Create an instance user with an invalid title. Try to add the user. " +
                     "Verify that 400 Bad Request is returned.")]
        [TestRail(303399)]
        public void AddInstanceUser_InvalidTitle_400BadRequest(uint numCharacters)
        {
            CreateDefaultInstanceUserWithInvalidPropertyVerify400BadRequest(
                _adminUser,
                "Title",
                RandomGenerator.RandomAlphaNumericUpperAndLowerCase(numCharacters),
                InstanceAdminErrorMessages.TitleFieldLimitation);
        }

        [TestCase]
        [Description("Create a valid instance user but when you add the user don't Base64 encode the password. " +
                     "Verify that 400 Bad Request is returned.")]
        [TestRail(305066)]
        public void AddInstanceUser_SendPlainTextPassword_400BadRequest()
        {
            // Setup:
            var userToAdd = AdminStoreHelper.GenerateRandomInstanceUser();

            // Execute:
            var ex = Assert.Throws<Http400BadRequestException>(() => AddInvalidUser(_adminUser, userToAdd),
                "'POST '{0}' should return 400 Bad Request if the Password isn't Base64 encoded.");

            // Verify:
            TestHelper.ValidateServiceError(ex.RestResponse, ErrorCodes.BadRequest, InstanceAdminErrorMessages.PasswordIsNotBase64);
        }

        #endregion 400 Bad Request Tests

        #region 401 Unauthorized Tests

        [TestCase(null, InstanceAdminErrorMessages.TokenMissingOrMalformed)]
        [TestCase("", InstanceAdminErrorMessages.TokenInvalid)]
        [TestCase(CommonConstants.InvalidToken, InstanceAdminErrorMessages.TokenInvalid)]
        [Description("Create and add an instance user with an invalid token header. " +
                     "Verify that 401 Unauthorized is returned.")]
        [TestRail(303373)]
        public  void AddInstanceUser_InvalidTokenHeader_401Unauthorized(string tokenString, string errorMessage)
        {
            // Setup:
            var createdUser = AdminStoreHelper.GenerateRandomInstanceUser();
            var userWithInvalidTokenHeader = Helper.CreateUserWithInvalidToken(
                TestHelper.AuthenticationTokenTypes.AccessControlToken,
                InstanceAdminRole.DefaultInstanceAdministrator,
                badToken: tokenString);

            // Execute & Verify:
            var ex = Assert.Throws<Http401UnauthorizedException>(() =>
            {
                Helper.AdminStore.AddUser(userWithInvalidTokenHeader, createdUser);
            }, "'POST {0}' should return 401 Unauthorized with invalid token header!", USER_PATH);

            TestHelper.ValidateServiceErrorMessage(ex.RestResponse, errorMessage);
        }

        #endregion 401 Unauthorized Tests

        #region 403 Forbidden Tests

        [TestCase(InstanceAdminPrivileges.ManageUsersOnly)]
        [TestCase(InstanceAdminPrivileges.ViewUsers)]
        [Description("Create an instance user.  Try to add the user with another user that does not have " +
                     "permission to view or manage users. Verify that 403 Forbidden is returned.")]
        [TestRail(303374)]
        public void AddInstanceUser_NoPermissionsToManageUsers_403Forbidden(InstanceAdminPrivileges privilegeToRemove)
        {
            using (var adminStoreHelper = new AdminStoreHelper())
            {
                // Setup:
                var allPrivilegesExceptManageUsers = (InstanceAdminPrivileges) int.MaxValue & ~privilegeToRemove;
                var adminRole = adminStoreHelper.AddInstanceAdminRoleToDatabase(allPrivilegesExceptManageUsers);

                var userWithNoPermissionsToManageUsers = Helper.CreateUserAndAuthenticate(
                    TestHelper.AuthenticationTokenTypes.AccessControlToken, adminRole);

                var createdUser = AdminStoreHelper.GenerateRandomInstanceUser();

                // Execute:
                var ex = Assert.Throws<Http403ForbiddenException>(() =>
                {
                    Helper.AdminStore.AddUser(userWithNoPermissionsToManageUsers, createdUser);
                }, "'POST {0}' should return 403 Forbidden when the user adding the created user has no permissions to manage users!",
                    USER_PATH);

                // Verify:
                TestHelper.ValidateServiceError(ex.RestResponse, ErrorCodes.Forbidden, InstanceAdminErrorMessages.UserDoesNotHavePermissions);
            }
        }

        #endregion 403 Forbidden Tests

        #region Private Methods

        /// <summary>
        /// Tries to add a user with the specified JSON body.  Use this to test corrupt/invalid JSON objects.
        /// </summary>
        /// <param name="adminUser">The user to authenticate with.</param>
        /// <param name="jsonBody">The JSON body to send to the REST call.</param>
        /// <returns>The ID of the created user.</returns>
        public int AddInvalidUser<T>(IUser adminUser, T jsonBody) where T : class
        {
            var restApi = new RestApiFacade(Helper.AdminStore.Address, adminUser?.Token?.AccessControlToken);
            string path = RestPaths.Svc.AdminStore.Users.USERS;

            Logger.WriteInfo("Creating user...");

            var response = restApi.SendRequestAndGetResponse(
                path,
                RestRequestMethod.POST,
                bodyObject: jsonBody,
                expectedStatusCodes: new List<HttpStatusCode> { HttpStatusCode.Created });

            return I18NHelper.Int32ParseInvariant(response.Content);
        }

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
                "'POST {0}' should return 400 Bad Request!", USER_PATH);

            // Verify:
            TestHelper.ValidateServiceErrorMessage(ex.RestResponse, expectedErrorMessage);
        }

        /// <summary>
        /// Validates that a user with expired password cannot login and get proper error response.
        /// </summary>
        /// <param name="instanceUser">user with the expired password</param>
        private void ValidateUserPasswordExpiration(
            InstanceUser instanceUser
            )
        {
            ThrowIf.ArgumentNull(instanceUser, nameof(instanceUser));

            // Login with the created user
            var session = Helper.AdminStore.AddSession(instanceUser.Login, instanceUser.Password);

            // Logout with the created user
            Helper.AdminStore.DeleteSession(session);

            // Simulate Password Expiration
            TestHelper.UpdateLastPasswordChangeTimestampFromUsersTable(
                instanceUser.Id.Value,
                DateTime.UtcNow.AddHours(-25));

            TestHelper.UpdateValueFromInstancesTable(PASSWORD_EXPIRATION_IN_DAYS, "1");

            // Login again after enable password expiration from instances
            var ex = Assert.Throws<Http401UnauthorizedException>(() =>
            {
                Helper.AdminStore.AddSession(instanceUser.Login, instanceUser.Password, force: true);
            }, "AddSession() should throw exception since the user password is expired.");

            TestHelper.ValidateServiceError(ex.RestResponse, ErrorCodes.PasswordExpired,
                "User password expired for the login: " + instanceUser.Login);
        }

        #endregion Private Methods
    }
}
