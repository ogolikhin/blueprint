using NUnit.Framework;
using CustomAttributes;
using Model;
using Helper;
using TestCommon;

namespace AdminStoreTests
{
    [TestFixture]
    [Category(Categories.AdminStore)]
    public class LicensesTests : TestBase
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

        [Test]
        public void GetLicenseTransactions_200OK()
        {
            int numberOfDays = 1;
            //right now we test that REST call returns valid list of License Activities
            //we don't check specific values for License Activities
            Assert.DoesNotThrow(() =>
            {
                Helper.AdminStore.GetLicenseTransactions(_user, numberOfDays);
            });
        }
    }
}