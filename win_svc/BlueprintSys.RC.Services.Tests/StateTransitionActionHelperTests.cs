using System.Threading.Tasks;
using BlueprintSys.RC.Services.MessageHandlers.StateTransition;
using BlueprintSys.RC.Services.Models;
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
        public async Task StateTransitionActionHelper_TenantInfoIsNull_HandleActionReturnsTrue()
        {
            var actionHelper = new StateTransitionActionHelper();
            var result = await actionHelper.HandleAction(null, null, null);
            Assert.IsFalse(result);
        }

        [TestMethod]
        public async Task StateTransitionActionHelper_MessageIsNull_HandleActionReturnsTrue()
        {
            var actionHelper = new StateTransitionActionHelper();
            var result = await actionHelper.HandleAction(new TenantInformation(), null, null);
            Assert.IsFalse(result);
        }
    }
}
