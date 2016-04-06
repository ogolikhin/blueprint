using NUnit.Framework;
using CustomAttributes;
using Model;
using Model.Factories;

namespace AdminStoreTests
{
    [TestFixture]
    [Category(Categories.AdminStore)]
    public class StatusTests
    {
        private IAdminStore _adminStore = AdminStoreFactory.GetAdminStoreFromTestConfig();

        [Test]
        [Description("Calls the /status/upcheck endpoint for AdminStore and verifies that it returns 200 OK")]
        public void GetStatusUpcheck_OK()
        {
            Assert.DoesNotThrow(() =>
            {
                _adminStore.GetStatusUpcheck();
            });
        }
    }
}
