using System.Collections.Generic;
using CustomAttributes;
using Helper.Factories;
using Model;
using NUnit.Framework;
using Logger = Logging.Logger;

namespace OpenAPITests
{
    [TestFixture]
    [Category(Categories.OpenAPI)]
    public class UserTests
    {
        private IUser _user = null;

        [SetUp]
        public void SetUp()
        {
            _user = UserFactory.CreateUserAndAddToDatabase();
        }

        [TearDown]
        public void TearDown()
        {
            if (_user != null)
            {
                _user.DeleteUser(deleteFromDatabase: true);
                _user = null;
            }
        }

        [Test]
        public void CreateUserInDatabase_VerifyUserExists()
        {
            List<IUser> users = UserFactory.GetUsers();
            string username = _user.Username;

            Assert.That(users.Exists(x => x.Username == username), "Couldn't find user '{0}' in database after adding the user to the database!", username);
        }

        [Test]
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

        [Test]
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
