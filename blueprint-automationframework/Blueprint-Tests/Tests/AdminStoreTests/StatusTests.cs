using System.Net;
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
        public void GetStatus_OK()
        {
            var statusCode = _adminStore.GetStatus();
            Assert.AreEqual(statusCode, HttpStatusCode.OK, "'GET /status' should return 200 OK, but failed with {0}", statusCode);
        }
    }
}
