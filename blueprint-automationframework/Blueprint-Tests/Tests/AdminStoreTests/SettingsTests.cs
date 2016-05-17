using NUnit.Framework;
using CustomAttributes;
using Model;
using Helper;
using TestCommon;

namespace AdminStoreTests
{
    [TestFixture]
    [Category(Categories.AdminStore)]
    public class SettingsTests : TestBase
    {
        private IUser _user = null;

        [SetUp]
        public void SetUp()
        {
            Helper = new TestHelper();
            _user = Helper.CreateUserAndAuthenticate(TestHelper.AuthenticationTokenTypes.AccessControlToken);
        }

        [TearDown]
        public void TearDown()
        {
            Helper?.Dispose();
        }

        [Test]//currently method returns empty dictionary
        public void GetSettings_OK()
        {
            Assert.DoesNotThrow(() =>
            {
                Helper.AdminStore.GetSettings(_user);
            });
        }

        [Test]//currently method is under development
        public void GetConfigJS_OK() // TODO: add check for returned content
        {
            Assert.DoesNotThrow(() =>
            {
                Helper.AdminStore.GetConfigJs(_user);
            });
        }
    }
}
