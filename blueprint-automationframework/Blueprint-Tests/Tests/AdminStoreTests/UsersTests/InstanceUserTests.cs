using System.Collections.Generic;
using System.Net;
using CustomAttributes;
using Helper;
using Model;
using Model.Factories;
using Model.Impl;
using NUnit.Framework;
using TestCommon;
using Utilities.Factories;

namespace AdminStoreTests.UsersTests
{
    [TestFixture]
    [Category(Categories.AdminPortal)]
    [Category(Categories.AdminStore)]

    public class InstanceUserTests : TestBase
    {
        private const string USERS_PATH = RestPaths.Svc.AdminStore.Users.USERS;
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

        [TestCase]
        [Description("Create and add an instance user. Get the added user using the id of the user that was just created. " +
             "Verify the same user that was created is returned.")]
        [TestRail(303340)]
        public void AddInstanceUser_ValidUser_ReturnsCorrectUser()
        {
            var createdUser = AdminStoreHelper.GenerateRandomInstanceUser();

            int createdUserId = 0;

            Assert.DoesNotThrow(() =>
            {
                createdUserId = Helper.AdminStore.AddUser(_adminUser, createdUser, new List<HttpStatusCode> { HttpStatusCode.Created });
            }, "'POST {0}' should return 201 OK for a valid session token!", USER_PATH);

            InstanceUser addedUser = null;
            
            Assert.DoesNotThrow(() =>
            {
                addedUser = Helper.AdminStore.GetUserById(_adminUser, createdUserId);
            }, "'GET {0}' should return 200 OK for a valid session token!", USER_PATH);

            Assert.AreEqual(createdUserId, addedUser.Id, "The added InstanceUser id {0} does not match the expected id {1}!", 
                addedUser.Id, createdUserId);
        }

        [TestCase]
        [Description("Create a user directly to database. Get the created user using the id of the user that was just created. " +
                     "Verify the same user that was created is returned.")]
        [TestRail(303341)]
        public void GetInstanceUser_ValidUser_ReturnsCorrectUser()
        {
            var user = Helper.CreateUserAndAuthenticate(TestHelper.AuthenticationTokenTypes.AccessControlToken);

            InstanceUser instanceUser = null;

            Assert.DoesNotThrow(() =>
            {
                instanceUser = Helper.AdminStore.GetUserById(_adminUser, user.Id);
            }, "'GET {0}' should return 200 OK for a valid session token!", USER_PATH);

            Assert.AreEqual(user.Id, instanceUser.Id, "The returned InstanceUser id {0} does not match the expected id {1}!",
                instanceUser.Id, user.Id);
        }

        [Explicit(IgnoreReasons.UnderDevelopmentDev)]
        [TestCase]
        [Description("Create users directly to database. Get a list of all users. Verify that the list of all users included the " +
                     "newly created users.")]
        [TestRail(303342)]
        public void GetInstanceUsers_ValidUsers_ReturnsCorrectUsers()
        {
            List<InstanceUser> instanceUsers = null;

            Assert.DoesNotThrow(() =>
            {
                instanceUsers = Helper.AdminStore.GetUsers(_adminUser);
            }, "'GET {0}' should return 200 OK for a valid session token!", USERS_PATH);

            Assert.Greater(instanceUsers.Count, 1, "Temporary message - under QA development");
        }

        [TestCase]
        [Description("Create a user directly to database. Get the created user using the id of the user that was just created. " +
             "Verify the same user that was created is returned.")]
        [TestRail(303364)]
        public void UpdateInstanceUser_ValidUser_UserUpdatesCorrectly()
        {
            var createdUser = AdminStoreHelper.GenerateRandomInstanceUser();

            int addedUserId = 0;

            Assert.DoesNotThrow(() =>
            {
                addedUserId = Helper.AdminStore.AddUser(_adminUser, createdUser, new List<HttpStatusCode> { HttpStatusCode.Created });
            }, "'POST {0}' should return 201 OK for a valid session token!", USER_PATH);

            InstanceUser returnedUser = null;

            Assert.DoesNotThrow(() =>
            {
                returnedUser = Helper.AdminStore.GetUserById(_adminUser, addedUserId);
            }, "'GET {0}' should return 200 OK for a valid session token!", USER_PATH);

            Assert.AreEqual(returnedUser.Id, addedUserId, "The returned InstanceUser does not match the expected!");

            var newFirstName = RandomGenerator.RandomAlphaNumericUpperAndLowerCase(8);

            returnedUser.FirstName = newFirstName;

            Assert.DoesNotThrow(() =>
            {
                Helper.AdminStore.UpdateUser(_adminUser, returnedUser);
            }, "'GET {0}' should return 200 OK for a valid session token!", USER_PATH);

            InstanceUser updatedUser = null;

            Assert.DoesNotThrow(() =>
            {
                updatedUser = Helper.AdminStore.GetUserById(_adminUser, addedUserId);
            }, "'GET {0}' should return 200 OK for a valid session token!", USER_PATH);

            Assert.AreEqual(returnedUser.FirstName, updatedUser.FirstName, "The InstanceUser was not updated!");
        }

        [Explicit(IgnoreReasons.UnderDevelopmentDev)]
        [TestCase]
        [Description("Get the list of instance administrator roles.")]
        [TestRail(999999)]
        public void GetInstanceRoles_ValidUsers_ReturnsCorrectUsers()
        {
            List<AdminRole> adminRoles = null;

            Assert.DoesNotThrow(() =>
            {
                adminRoles = Helper.AdminStore.GetInstanceRoles(_adminUser);
            }, "'GET {0}' should return 200 OK for a valid session token!", USERS_PATH);

            Assert.Greater(adminRoles.Count, 1, "Temporary message - under QA development");
        }
    }
}
