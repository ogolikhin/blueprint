using Microsoft.VisualStudio.TestTools.UnitTesting;
using NServiceBus.Testing;
using PocService;

namespace PocTests
{
    [TestClass]
    public class UnitTests
    {
        [TestMethod]
        public void PocSpawnMessageHandler_CompletesSuccessfully()
        {
            var handler = new PocSpawnMessageHandler();
            var message = new PocSpawnMessage();
            Test.Handler(handler).SetIncomingHeader(PocHeaders.TenantId, "tenant0").OnMessage(message);
        }
    }
}
