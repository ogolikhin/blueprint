using System.Collections.Generic;
using CustomAttributes;
using Helper;
using Model;
using Model.Impl;
using NUnit.Framework;
using TestCommon;

namespace AdminStoreTests.UsersTests
{
    [TestFixture]
    [Category(Categories.AdminPortal)]
    [Category(Categories.AdminStore)]

    public class GetInstanceUserTests : TestBase
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
        [TestRail(303341)]
        public void GetInstanceUser_ValidUser_ReturnsCorrectUser()
        {
            // Setup:
            var user = Helper.CreateUserAndAuthenticate(TestHelper.AuthenticationTokenTypes.AccessControlToken);

            InstanceUser instanceUser = null;

            // Execute:
            Assert.DoesNotThrow(() =>
            {
                instanceUser = Helper.AdminStore.GetUserById(_adminUser, user.Id);
            }, "'GET {0}' should return 200 OK for a valid session token!", USER_PATH);

            //Verify:
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
            // Setup:
            List<InstanceUser> instanceUsers = null;

            // Execute:
            Assert.DoesNotThrow(() =>
            {
                instanceUsers = Helper.AdminStore.GetUsers(_adminUser);
            }, "'GET {0}' should return 200 OK for a valid session token!", USERS_PATH);

            //Verify:
            Assert.Greater(instanceUsers.Count, 1, "Temporary message - under QA development");
        }

    }
}
