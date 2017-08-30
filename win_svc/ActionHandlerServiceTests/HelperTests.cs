using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ActionHandlerService;
using ActionHandlerService.Helpers;
using ActionHandlerService.Models;
using ActionHandlerService.Repositories;
using BluePrintSys.Messaging.CrossCutting.Configuration;
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
        public async Task TenantInfoRetriever_ReturnsTenants()
        {
            var mockConfigHelper = new Mock<IConfigHelper>();
            mockConfigHelper.Setup(m => m.CacheExpirationMinutes).Returns(1);
            var mockActionHandlerServiceRepository = new Mock<IActionHandlerServiceRepository>();
            var sqlTenants = new List<TenantInformation>();
            for (int i = 0; i < 5; i++)
            {
                sqlTenants.Add(
                    new TenantInformation
                    {
                        TenantId = $"TenantId{i}",
                        BlueprintConnectionString = $"BlueprintConnectionString{i}",
                        AdminStoreLog = $"AdminStoreLog{i}",
                        ExpirationDate = DateTime.MaxValue,
                        PackageLevel = i,
                        PackageName = $"PackageName{i}",
                        StartDate = DateTime.MinValue,
                        TenantName = $"TenantName{i}"
                    });
            }
            mockActionHandlerServiceRepository.Setup(m => m.GetTenantsFromTenantsDb()).ReturnsAsync(sqlTenants);
            var tenantInfoRetriever = new TenantInfoRetriever(mockActionHandlerServiceRepository.Object, mockConfigHelper.Object);
            var tenants = await tenantInfoRetriever.GetTenants();
            Assert.AreEqual(sqlTenants.Count, tenants.Count);
        }

        [TestMethod]
        public async Task NServiceBus_ReturnsArgumentNullExceptionMessage_WhenConnectionStringIsNull()
        {
            var exceptionMessage = await WorkflowServiceBusServer.Instance.Start(null, true);
            Assert.IsTrue(exceptionMessage.Contains("connectionString"));
        }
    }
}
