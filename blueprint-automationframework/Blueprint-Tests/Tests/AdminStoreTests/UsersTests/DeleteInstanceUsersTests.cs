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

    public class DeleteInstanceUserTests : TestBase
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
                     "Delete the user that was just created.  Verify the user has been deleted.")]
        [TestRail(999999)]
        public void DeleteInstanceUser_ValidUser_ReturnsCorrectUser()
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
        }
    }
}
