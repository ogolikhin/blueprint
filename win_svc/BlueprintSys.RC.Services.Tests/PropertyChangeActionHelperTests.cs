using System.Threading.Tasks;
using BlueprintSys.RC.Services.MessageHandlers.PropertyChange;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BlueprintSys.RC.Services.Tests
{
    /// <summary>
    /// Tests for the PropertyChange Change Action Helper in the Action Handler Service
    /// </summary>
    [TestClass]
    public class PropertyChangeActionHelperTests
    {
        [TestMethod]
        public async Task PropertyChangeActionHelper_HandleActionReturnsTrue()
        {
            var actionHelper = new PropertyChangeActionHelper();
            var result = await actionHelper.HandleAction(null, null, null);
            Assert.IsTrue(result);
        }
    }
}
