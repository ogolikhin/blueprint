using System.Threading.Tasks;
using ActionHandlerService;
using ActionHandlerService.Helpers;
using BluePrintSys.Messaging.CrossCutting.Configuration;
using BluePrintSys.Messaging.CrossCutting.Models.Enums;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace ActionHandlerServiceTests
{
    /// <summary>
    /// Tests for Action Handler Service Helpers
    /// </summary>
    [TestClass]
    public class HelperTests
    {
        [TestMethod]
        public async void TenantInfoRetriever_ReturnsTenants_WhenUsingSingleTenancy()
        {
            var configHelperMock = new Mock<IConfigHelper>();
            configHelperMock.Setup(m => m.Tenancy).Returns(Tenancy.Single);
            configHelperMock.Setup(m => m.CacheExpirationMinutes).Returns(1);
            var tenantInfoRetriever = new TenantInfoRetriever(null, configHelperMock.Object);
            var tenants = await tenantInfoRetriever.GetTenants();
            Assert.IsNotNull(tenants);
        }

        [TestMethod]
        public async void TenantInfoRetriever_ReturnsTenants_WhenUsingMultipleTenancy()
        {
            var configHelperMock = new Mock<IConfigHelper>();
            configHelperMock.Setup(m => m.Tenancy).Returns(Tenancy.Multiple);
            configHelperMock.Setup(m => m.CacheExpirationMinutes).Returns(1);
            var tenantInfoRetriever = new TenantInfoRetriever(null, configHelperMock.Object);
            var tenants = await tenantInfoRetriever.GetTenants();
            Assert.IsNotNull(tenants);
        }

        [TestMethod]
        public async Task NServiceBus_ReturnsArgumentNullExceptionMessage_WhenConnectionStringIsNull()
        {
            var exceptionMessage = await WorkflowServiceBusServer.Instance.Start(null, true);
            Assert.IsTrue(exceptionMessage.Contains("connectionString"));
        }
    }
}
