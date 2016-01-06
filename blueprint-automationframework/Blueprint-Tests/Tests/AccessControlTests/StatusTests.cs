using System.Net;
using NUnit.Framework;
using CustomAttributes;
using Model;
using Model.Factories;

namespace AccessControlTests
{
    [TestFixture]
    [Category(Categories.AccessControl)]
    public class StatusTests
    {
        private IAccessControl _accessControl = AccessControlFactory.GetAccessControlFromTestConfig();

        [Test]
        public void GetStatus_OK()
        {
            var statusCode = _accessControl.GetStatus();
            Assert.AreEqual(statusCode, HttpStatusCode.OK, "'GET /status' should return 200 OK, but failed with {0}", statusCode);
        }
    }
}
