using CustomAttributes;
using Helper.Factories;
using Model;
using NUnit.Framework;
using Utilities;

namespace AdminStoreTests
{
    [TestFixture]
    [Category(Categories.AdminStore)]
    public class LoginTests
    {
        private IAdminStore _adminStore = AdminStoreFactory.GetAdminStoreFromTestConfig();
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
        public void Login_ValidUser_Verify200OK()
        {
            _adminStore.AddSession(_user.Username, _user.Password);
        }

        [Test]
        public void Login_ValidUserBadPassword_Verify401Error()
        {
            IServiceErrorMessage expectedServiceErrorMessage = ServiceErrorMessageFactory.CreateServiceErrorMessage(2000, "Invalid username or password");

            Assert.Throws<Http401UnauthorizedException>(() =>
            {
                _adminStore.AddSession(_user.Username, "bad-password", expectedServiceErrorMessage: expectedServiceErrorMessage);
            });
        }
    }
}
