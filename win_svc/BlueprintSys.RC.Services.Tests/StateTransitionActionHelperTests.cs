using System.Threading.Tasks;
using BlueprintSys.RC.Services.MessageHandlers.StateTransition;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BlueprintSys.RC.Services.Tests
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
