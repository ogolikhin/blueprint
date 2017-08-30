using System.Threading.Tasks;
using BlueprintSys.RC.Services.MessageHandlers.GenerateDescendants;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BlueprintSys.RC.Services.Tests
{
    /// <summary>
    /// Tests for the Generate Descendants Action Helper in the Action Handler Service
    /// </summary>
    [TestClass]
    public class GenerateDescendantsActionHelperTests
    {
        [TestMethod]
        public async Task GenerateDescendantsActionHelper_NullMessage_HandleActionReturnsFalse()
        {
            var actionHelper = new GenerateDescendantsActionHelper();
            var result = await actionHelper.HandleAction(null, null, null);
            Assert.IsFalse(result, "Action should have failed for null message");
        }
    }
}
