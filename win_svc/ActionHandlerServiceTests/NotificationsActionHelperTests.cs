using System.Threading.Tasks;
using ActionHandlerService.MessageHandlers.Notifications;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ActionHandlerServiceTests
{
    /// <summary>
    /// Tests for the Notifications Action Helper in the Action Handler Service
    /// </summary>
    [TestClass]
    public class NotificationsActionHelperTests
    {
        [TestMethod]
        public async Task NotificationsActionHelper_HandleActionReturnsTrue()
        {
            var actionHelper = new NotificationsActionHelper();
            var result = await actionHelper.HandleAction(null, null, null);
            Assert.IsTrue(result);
        }
    }
}
