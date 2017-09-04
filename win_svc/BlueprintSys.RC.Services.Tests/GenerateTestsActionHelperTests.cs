using System.Threading.Tasks;
using BlueprintSys.RC.Services.MessageHandlers.GenerateTests;
using BlueprintSys.RC.Services.Models;
using BluePrintSys.Messaging.Models.Actions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BlueprintSys.RC.Services.Tests
{
    /// <summary>
    /// Tests for the Generate Tests Action Helper in the Action Handler Service
    /// </summary>
    [TestClass]
    public class GenerateTestsActionHelperTests
    {
        [TestMethod]
        public async Task GenerateTestsActionHelper_MessageIsNull_HandleActionReturnsFalse()
        {
            var actionHelper = new GenerateTestsActionHelper();
            var result = await actionHelper.HandleAction(new TenantInformation(), null, null);
            Assert.IsFalse(result);
        }

        [TestMethod]
        public async Task GenerateTestsActionHelper_TenantInfoIsNull_HandleActionReturnsFalse()
        {
            var actionHelper = new GenerateTestsActionHelper();
            var result = await actionHelper.HandleAction(null, new GenerateTestsMessage(), null);
            Assert.IsFalse(result);
        }
    }
}
