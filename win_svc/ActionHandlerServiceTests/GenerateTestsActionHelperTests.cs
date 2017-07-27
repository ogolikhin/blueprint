using System.Threading.Tasks;
using ActionHandlerService.MessageHandlers.GenerateTests;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ActionHandlerServiceTests
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
