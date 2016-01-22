using System.Net;
using NUnit.Framework;
using CustomAttributes;
using Model;
using Model.Factories;
using System.Collections.Generic;

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
            Assert.DoesNotThrow(() =>
            {
                _adminStore.GetStatus();
            });
        }
    }
}
