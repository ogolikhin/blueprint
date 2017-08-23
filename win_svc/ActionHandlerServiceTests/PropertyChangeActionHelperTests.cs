using System.Threading.Tasks;
using ActionHandlerService.MessageHandlers.PropertyChange;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ActionHandlerServiceTests
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
