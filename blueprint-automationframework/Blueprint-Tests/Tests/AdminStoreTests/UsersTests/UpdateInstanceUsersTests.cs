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

    public class UpdateInstanceUserTests : TestBase
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
        [Description("Create a user directly to database. Get the created user using the id of the user that was just created. " +
             "Verify the same user that was created is returned.")]
        [TestRail(303364)]
        public void UpdateInstanceUser_ValidUser_UserUpdatesCorrectly()
        {
            // Setup:
            var createdUser = AdminStoreHelper.GenerateRandomInstanceUser();

            int addedUserId = 0;

            Assert.DoesNotThrow(() =>
            {
                addedUserId = Helper.AdminStore.AddUser(_adminUser, createdUser);
            }, "'POST {0}' should return 201 OK for a valid session token!", USER_PATH);

            InstanceUser returnedUser = null;

            Assert.DoesNotThrow(() =>
            {
                returnedUser = Helper.AdminStore.GetUserById(_adminUser, addedUserId);
            }, "'GET {0}' should return 200 OK for a valid session token!", USER_PATH);

            Assert.AreEqual(returnedUser.Id, addedUserId, "The returned InstanceUser does not match the expected!");

            // Execute:
            var newFirstName = RandomGenerator.RandomAlphaNumericUpperAndLowerCase(8);

            returnedUser.FirstName = newFirstName;

            Assert.DoesNotThrow(() =>
            {
                Helper.AdminStore.UpdateUser(_adminUser, returnedUser);
            }, "'GET {0}' should return 200 OK for a valid session token!", USER_PATH);


            // Verify:
            InstanceUser updatedUser = null;

            Assert.DoesNotThrow(() =>
            {
                updatedUser = Helper.AdminStore.GetUserById(_adminUser, addedUserId);
            }, "'GET {0}' should return 200 OK for a valid session token!", USER_PATH);

            Assert.AreEqual(returnedUser.FirstName, updatedUser.FirstName, "The InstanceUser was not updated!");
        }
    }
}
