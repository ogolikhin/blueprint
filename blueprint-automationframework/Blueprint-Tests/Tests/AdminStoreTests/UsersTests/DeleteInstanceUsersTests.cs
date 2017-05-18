using System.Collections.Generic;
using CustomAttributes;
using Helper;
using Model;
using NUnit.Framework;
using TestCommon;
using Utilities;

namespace AdminStoreTests.UsersTests
{
    [TestFixture]
    [Category(Categories.AdminPortal)]
    [Category(Categories.AdminStore)]

    public class DeleteInstanceUserTests : TestBase
    {
        private const string USER_PATH = RestPaths.Svc.AdminStore.Users.USERS_id_;

        private IUser _adminUser;

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

        [Explicit(IgnoreReasons.UnderDevelopmentDev)]
        [TestCase]
        [Description("Create and add an instance user. Using the id of the created user, delete the user. " +
                     "Verify the user has been deleted.")]
        [TestRail(303462)]
        public void DeleteInstanceUser_ValidUser_UserDeleted()
        {
            // Setup:
            var createdUser = AdminStoreHelper.GenerateRandomInstanceUser();

            var createdUserId = 0;

            Assert.DoesNotThrow(() =>
            {
                createdUserId = Helper.AdminStore.AddUser(_adminUser, createdUser);
            }, "'POST {0}' should return 201 Created for a valid session token!", USER_PATH);

            // Execute:
            Assert.DoesNotThrow(() =>
            {
                Helper.AdminStore.DeleteUsers(_adminUser, new List<int> { createdUserId });
            }, "'DELETE {0}' should return 204 No Content for a valid session token!", USER_PATH);

            // Verify:
            Assert.Throws<Http404NotFoundException>(() =>
            {
                Helper.AdminStore.GetUserById(_adminUser, createdUserId);
            }, "'GET {0}' should return 404 Not Found for a valid session token!", USER_PATH);
        }
    }
}
