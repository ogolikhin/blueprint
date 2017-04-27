using System;
using System.Collections.Generic;
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

        //[Explicit(IgnoreReasons.UnderDevelopmentQaDev)]
        //[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1806:DoNotIgnoreMethodResults", MessageId = "System.Int32.TryParse(System.String,System.Int32@)")]
        //[TestCase]
        //[Description("Create an instance user. Get the created user using the id of the user that was just created. " +
        //     "Verify the same user that was created is returned.")]
        //[TestRail(303340)]
        //public void CreateInstanceUser_ValidUser_ReturnsCorrectUser()
        //{
        //    int userId;

        //    var createdUser = AdminStoreHelper.GenerateRandomInstanceUser();

        //    var response = Helper.AdminStore.CreateUser(_adminUser, createdUser);

        //    Assert.DoesNotThrow(() =>
        //    {
        //        createdUser = Helper.AdminStore.GetUserById(_adminUser, userId);
        //    }, "'GET {0}' should return 200 OK for a valid session token!", USER_PATH);

        //    InstanceUser retrievedUser = null;

        //    Assert.DoesNotThrow(() =>
        //    {
        //        retrievedUser = Helper.AdminStore.GetUserById(_adminUser, userId);
        //    }, "'GET {0}' should return 200 OK for a valid session token!", USER_PATH);

        //    Assert.AreEqual(retrievedUser.Id, userId, "The returned InstanceUser does not match the expected!");
        //}

        [Explicit(IgnoreReasons.UnderDevelopmentQaDev)]
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

            Assert.AreEqual(instanceUser.Id, user.Id, "The returned InstanceUser does not match the expected!");
        }

        [Explicit(IgnoreReasons.UnderDevelopmentQaDev)]
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
    }
}
