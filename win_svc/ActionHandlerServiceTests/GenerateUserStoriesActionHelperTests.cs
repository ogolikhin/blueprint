using System.Threading.Tasks;
using ActionHandlerService.MessageHandlers.GenerateUserStories;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ActionHandlerServiceTests
{
    /// <summary>
    /// Tests for the Generate User Stories Action Helper in the Action Handler Service
    /// </summary>
    [TestClass]
    public class GenerateUserStoriesActionHelperTests
    {
        [TestMethod]
        public async Task GenerateUserStoriesActionHelper_HandleActionReturnsTrue()
        {
            var actionHelper = new GenerateUserStoriesActionHelper();
            var result = await actionHelper.HandleAction(null, null, null);
            Assert.IsTrue(result);
        }
    }
}
