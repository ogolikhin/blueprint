using System.Threading.Tasks;
using ActionHandlerService.MessageHandlers.StateTransition;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ActionHandlerServiceTests
{
    /// <summary>
    /// Tests for the State Transition Action Helper in the Action Handler Service
    /// </summary>
    [TestClass]
    public class StateTransitionActionHelperTests
    {
        [TestMethod]
        public async Task StateTransitionActionHelper_HandleActionReturnsTrue()
        {
            var actionHelper = new StateTransitionActionHelper();
            var result = await actionHelper.HandleAction(null, null, null);
            Assert.IsTrue(result);
        }
    }
}
