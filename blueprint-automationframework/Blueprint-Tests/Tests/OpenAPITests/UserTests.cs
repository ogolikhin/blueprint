using System.Collections.Generic;
using Common;
using CustomAttributes;
using Helper;
using Model;
using Model.Factories;
using NUnit.Framework;
using TestCommon;

namespace OpenAPITests
{
    [TestFixture]
    [Category(Categories.OpenApi)]
    public class UserTests : TestBase
    {
        private IUser _user = null;

        [SetUp]
        public void SetUp()
        {
            Helper = new TestHelper();
            _user = Helper.CreateUserAndAddToDatabase();
        }

        [TearDown]
        public void TearDown()
        {
            Helper?.Dispose();
        }

        [TestCase]
        public void CreateUserInDatabase_VerifyUserExists()
        {
            List<IUser> users = UserFactory.GetUsers();
            string username = _user.Username;

            Assert.That(users.Exists(x => x.Username == username), "Couldn't find user '{0}' in database after adding the user to the database!", username);
        }

        [TestCase]
        public void DeleteUser_VerifyUserIsDeleted()
        {
            List<IUser> users = UserFactory.GetUsers();
            string username = _user.Username;

            Assert.That(users.Exists(x => x.Username == username), "Couldn't find user '{0}' in database after adding the user to the database!", username);

            // Now delete the user.
            _user.DeleteUser();

            // Verify that the user was deleted.
            users = UserFactory.GetUsers();

            Assert.IsFalse(users.Exists(x => x.Username == username), "We found user '{0}' in database after deleting the user!", username);
        }

        [TestCase]
        public static void GetUserInfoFromDatabase_VerifyUserExists()
        {
            List<IUser> users = UserFactory.GetUsers();

            foreach (var user in users)
            {
                Logger.WriteDebug(user.ToString());
            }

            // We assume that every Blueprint installation has an 'admin' user by default.
            Assert.That(users.Exists(x => x.Username == "admin"), "Couldn't find 'admin' user in database!");
        }
    }
}
