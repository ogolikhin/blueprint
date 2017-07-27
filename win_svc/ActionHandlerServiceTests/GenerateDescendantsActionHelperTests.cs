using System.Threading.Tasks;
using ActionHandlerService.MessageHandlers.GenerateDescendants;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ActionHandlerServiceTests
{
    /// <summary>
    /// Tests for the Generate Descendants Action Helper in the Action Handler Service
    /// </summary>
    [TestClass]
    public class GenerateDescendantsActionHelperTests
    {
        [TestMethod]
        public async Task GenerateDescendantsActionHelper_HandleActionReturnsTrue()
        {
            var actionHelper = new GenerateDescendantsActionHelper();
            var result = await actionHelper.HandleAction(null, null, null);
            Assert.IsTrue(result);
        }
    }
}
