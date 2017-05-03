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
    public class GetInstanceRolesTests : TestBase
    {
        private const string USERS_PATH = RestPaths.Svc.AdminStore.Users.USERS;

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

        [Explicit(IgnoreReasons.UnderDevelopmentDev)]
        [TestCase]
        [Description("Retrieve all existing instance admin roles. " +
                     "Verify the same user that was created is returned.")]
        [TestRail(303472)]
        public void GetInstanceAdminRoles_ValidRoles_RolesRetrieved()
        {
            // Setup:

            // Execute:
            List<AdminRole> adminRoles = null;

            Assert.DoesNotThrow(() =>
            {
                adminRoles = Helper.AdminStore.GetInstanceRoles(_adminUser);
            }, "'POST {0}' should return 200 OK for a valid session token!", USERS_PATH);


            // Verify:
            Assert.GreaterOrEqual(adminRoles.Count, 1, "The instance admin role count ({0}) must be greater than {1}!",
                adminRoles.Count, 1);
        }
    }
}
