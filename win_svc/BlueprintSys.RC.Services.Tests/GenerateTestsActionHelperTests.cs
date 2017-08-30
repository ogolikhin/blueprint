using System.Threading.Tasks;
using BlueprintSys.RC.Services.MessageHandlers.GenerateTests;
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
        public async Task GenerateTestsActionHelper_HandleActionReturnsTrue()
        {
            var actionHelper = new GenerateTestsActionHelper();
            var result = await actionHelper.HandleAction(null, null, null);
            Assert.IsTrue(result);
        }
    }
}
