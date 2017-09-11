using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BlueprintSys.RC.Services.Helpers;
using BlueprintSys.RC.Services.Models;
using BlueprintSys.RC.Services.Repositories;
using BluePrintSys.Messaging.CrossCutting.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace BlueprintSys.RC.Services.Tests
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
            //arrange
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

            //act
            var tenants = await tenantInfoRetriever.GetTenants();

            //assert
            Assert.AreEqual(sqlTenants.Count, tenants.Count);
            foreach (var sqlTenant in sqlTenants)
            {
                var tenant = tenants[sqlTenant.TenantId];
                Assert.AreEqual(sqlTenant.TenantId, tenant.TenantId);
                Assert.AreEqual(sqlTenant.BlueprintConnectionString, tenant.BlueprintConnectionString);
                Assert.AreEqual(sqlTenant.AdminStoreLog, tenant.AdminStoreLog);
                Assert.AreEqual(sqlTenant.ExpirationDate, tenant.ExpirationDate);
                Assert.AreEqual(sqlTenant.PackageLevel, tenant.PackageLevel);
                Assert.AreEqual(sqlTenant.PackageName, tenant.PackageName);
                Assert.AreEqual(sqlTenant.StartDate, tenant.StartDate);
                Assert.AreEqual(sqlTenant.TenantName, tenant.TenantName);
            }
        }

        [TestMethod]
        public async Task NServiceBus_ReturnsArgumentNullExceptionMessage_WhenConnectionStringIsNull()
        {
            var exceptionMessage = await WorkflowServiceBusServer.Instance.Start(null, true);
            Assert.IsTrue(exceptionMessage.Contains("connectionString"));
        }
    }
}
