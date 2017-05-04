using System;
using CustomAttributes;
using Helper;
using Model;
using Model.Impl;
using NUnit.Framework;
using TestCommon;
using Utilities;

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

        [Explicit(IgnoreReasons.UnderDevelopmentQaDev)]
        [TestCase]
        [Description("Create and add an instance user. Get the added user using the id of the user that was just created. " +
             "Verify the same user that was created is returned.")]
        [TestRail(303340)]
        public void AddInstanceUser_ValidUser_ReturnsCorrectUser()
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

        #endregion 201 Created Tests

        #region 400 Bad Request Tests

        [Explicit(IgnoreReasons.UnderDevelopmentQaDev)]
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
            var ex = Assert.Throws<Http400BadRequestException>(() =>
            {
                Helper.AdminStore.AddUser(_adminUser, createdUser);
            }, "'GET {0}' should return 400 Bad Request when the user already exists!", USER_PATH);

            // Verify:
            TestHelper.ValidateServiceErrorMessage(ex.RestResponse, ErrorMessages.LoginNameUnique);
        }

        [Explicit(IgnoreReasons.UnderDevelopmentQaDev)]
        [TestCase("", Description = "Password is empty")]
        [TestCase(null, Description = "Password is null")]
        [Description("Create an instance user with a missing password. Try to add the user. " +
                     "Verify that 400 Bad Request is returned.")]
        [TestRail(303401)]
        public void AddInstanceUser_MissingPassword_400BadRequest(string password)
        {
            // Setup:
            var createdUser = AdminStoreHelper.GenerateRandomInstanceUser();
            createdUser.Password = password;

            // Execute:
            var ex = Assert.Throws<Http400BadRequestException>(() =>
            {
                Helper.AdminStore.AddUser(_adminUser, createdUser);
            }, "'GET {0}' should return 400 Bad Request when the password is missing!", USER_PATH);

            // Verify:
            TestHelper.ValidateServiceErrorMessage(ex.RestResponse, ErrorMessages.PasswordMissing);
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

        [Explicit(IgnoreReasons.UnderDevelopmentQaDev)]
        [TestCase ("", Description = "Login is empty")]
        [TestCase (null, Description = "Login is null")]
        [Description("Create an instance user with a missing login name. Try to add the user. " +
                     "Verify that 400 Bad Request is returned.")]
        [TestRail(303379)]
        public void AddInstanceUser_MissingLogin_400BadRequest(string login)
        {
            // Setup:
            var createdUser = AdminStoreHelper.GenerateRandomInstanceUser();
            createdUser.Login = login;

            // Execute:
            var ex = Assert.Throws<Http400BadRequestException>(() =>
            {
                Helper.AdminStore.AddUser(_adminUser, createdUser);
            }, "'GET {0}' should return 400 Bad Request when the login is missing!", USER_PATH);

            // Verify:
            TestHelper.ValidateServiceErrorMessage(ex.RestResponse, ErrorMessages.LoginRequired);
        }

        [Explicit(IgnoreReasons.UnderDevelopmentQaDev)]
        [TestCase]
        [Description("Create an instance user with an invalid login name. Try to add the user. " +
                     "Verify that 400 Bad Request is returned.")]
        [TestRail(303380)]
        public static void AddInstanceUser_InvalidLogin_400BadRequest()
        {
            throw new NotImplementedException();
        }

        [Explicit(IgnoreReasons.UnderDevelopmentQaDev)]
        [TestCase]
        [Description("Create an instance user with a missing display name. Try to add the user. " +
                     "Verify that 400 Bad Request is returned.")]
        [TestRail(303381)]
        public static void AddInstanceUser_MissingDisplayName_400BadRequest()
        {
            throw new NotImplementedException();
        }

        [Explicit(IgnoreReasons.UnderDevelopmentQaDev)]
        [TestCase]
        [Description("Create an instance user with an invalid email. Try to add the user. " +
                     "Verify that 400 Bad Request is returned.")]
        [TestRail(303382)]
        public static void AddInstanceUser_InvalidEmail_400BadRequest()
        {
            throw new NotImplementedException();
        }

        [Explicit(IgnoreReasons.UnderDevelopmentQaDev)]
        [TestCase]
        [Description("Create an instance user with a missing user source. Try to add the user. " +
                     "Verify that 400 Bad Request is returned.")]
        [TestRail(303385)]
        public static void AddInstanceUser_MissingSource_400BadRequest()
        {
            throw new NotImplementedException();
        }

        [Explicit(IgnoreReasons.UnderDevelopmentQaDev)]
        [TestCase]
        [Description("Create an instance user with an invalid user source. Try to add the user. " +
                     "Verify that 400 Bad Request is returned.")]
        [TestRail(303383)]
        public static void AddInstanceUser_InvalidSource_400BadRequest()
        {
            throw new NotImplementedException();
        }

        [Explicit(IgnoreReasons.UnderDevelopmentQaDev)]
        [TestCase]
        [Description("Create an instance user with a missing license level. Try to add the user. " +
                     "Verify that 400 Bad Request is returned.")]
        [TestRail(303386)]
        public static void AddInstanceUser_MissingLicenseLevel_400BadRequest()
        {
            throw new NotImplementedException();
        }

        [Explicit(IgnoreReasons.UnderDevelopmentQaDev)]
        [TestCase]
        [Description("Create an instance user with an invalid license level. Try to add the user. " +
                     "Verify that 400 Bad Request is returned.")]
        [TestRail(303384)]
        public static void AddInstanceUser_InvalidLicense_400BadRequest()
        {
            throw new NotImplementedException();
        }

        [Explicit(IgnoreReasons.UnderDevelopmentQaDev)]
        [TestCase]
        [Description("Create an instance user with an invalid admin role. Try to add the user. " +
                     "Verify that 400 Bad Request is returned.")]
        [TestRail(303388)]
        public static void AddInstanceUser_InvalidAdminRole_400BadRequest()
        {
            throw new NotImplementedException();
        }

        [Explicit(IgnoreReasons.UnderDevelopmentQaDev)]
        [TestCase]
        [Description("Create an instance user with an invalid first name. Try to add the user. " +
                     "Verify that 400 Bad Request is returned.")]
        [TestRail(303396)]
        public static void AddInstanceUser_InvalidFirstName_400BadRequest()
        {
            throw new NotImplementedException();
        }

        [Explicit(IgnoreReasons.UnderDevelopmentQaDev)]
        [TestCase]
        [Description("Create an instance user with an invalid last name. Try to add the user. " +
                     "Verify that 400 Bad Request is returned.")]
        [TestRail(303397)]
        public static void AddInstanceUser_InvalidLastName_400BadRequest()
        {
            throw new NotImplementedException();
        }

        [Explicit(IgnoreReasons.UnderDevelopmentQaDev)]
        [TestCase]
        [Description("Create an instance user with an invalid department. Try to add the user. " +
                     "Verify that 400 Bad Request is returned.")]
        [TestRail(303398)]
        public static void AddInstanceUser_InvalidDepartment_400BadRequest()
        {
            throw new NotImplementedException();
        }

        [Explicit(IgnoreReasons.UnderDevelopmentQaDev)]
        [TestCase]
        [Description("Create an instance user with an invalid title. Try to add the user. " +
                     "Verify that 400 Bad Request is returned.")]
        [TestRail(303399)]
        public static void AddInstanceUser_InvalidTitle_400BadRequest()
        {
            throw new NotImplementedException();
        }

        #endregion 400 Bad Request Tests

        #region 401 Unauthorized Tests

        [Explicit(IgnoreReasons.UnderDevelopmentQaDev)]
        [TestCase]
        [Description("Create and add an instance user without a valid session. " +
                     "Verify that 401 Unauthorized is returned.")]
        [TestRail(303372)]
        public static void AddInstanceUser_InvalidSession_401Unauthorized()
        {
            throw new NotImplementedException();
        }

        [Explicit(IgnoreReasons.UnderDevelopmentQaDev)]
        [TestCase]
        [Description("Create and add an instance user without a token header. " +
                     "Verify that 401 Unauthorized is returned.")]
        [TestRail(30373)]
        public static void AddInstanceUser_MissingTokenHeader_401Unauthorized()
        {
            throw new NotImplementedException();
        }

        #endregion 401 Unauthorized Tests

        #region 403 Forbidden Tests

        [Explicit(IgnoreReasons.UnderDevelopmentQaDev)]
        [TestCase]
        [Description("Create an instance user.  Try to add the user with another user that does not have" +
                     "permission to manage users. Verify that 401 Unauthorized is returned.")]
        [TestRail(303374)]
        public static void AddInstanceUser_NoPermissionsToManageUsers_403Forbidden()
        {
            throw new NotImplementedException();
        }

        [Explicit(IgnoreReasons.UnderDevelopmentQaDev)]
        [TestCase]
        [Description("Create an instance user.  Try to add the user with another user that does not have" +
                     "admin privileges. Verify that 401 Unauthorized is returned.")]
        [TestRail(303423)]
        public static void AddInstanceUser_AssignInstanceAdminRoleWithNoInstanceAdminPrivileges_403Forbidden()
        {
            throw new NotImplementedException();
        }

        #endregion 403 Forbidden Tests
    }
}
