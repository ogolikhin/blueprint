using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BlueprintSys.RC.Services.Logging;
using BluePrintSys.Messaging.CrossCutting.Configuration;
using BluePrintSys.Messaging.CrossCutting.Host;
using BluePrintSys.Messaging.CrossCutting.Logging;
using BluePrintSys.Messaging.Models.Actions;
using log4net.Appender;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using ServiceLibrary.Models.Workflow;

namespace BlueprintSys.RC.Services.Tests
{
    /// <summary>
    /// Tests for Action Handler Service Transport
    /// </summary>
    [TestClass]
    public class TransportTests
    {
        private Mock<INServiceBusServer> _mockNServiceBusServer;
        private TransportHost _transportHost;

        [TestInitialize]
        public void TestInitialize()
        {
            _mockNServiceBusServer = new Mock<INServiceBusServer>(MockBehavior.Strict);
            _transportHost = new TransportHost(new ConfigHelper(), _mockNServiceBusServer.Object);
        }

        [TestMethod]
        public void TransportHost_StartsSuccessfully()
        {
            _transportHost.Start(false);
        }

        [TestMethod]
        public void TransportHost_LogsError_WhenStartFails()
        {
            LogManager.Manager.AddListener(Log4NetStandardLogListener.Instance);
            var appender = new MemoryAppender();
            log4net.Config.BasicConfigurator.Configure(appender);

            const string errorMessage = "error message";
            _mockNServiceBusServer.Setup(m => m.Start(It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(errorMessage);
            _transportHost.Start(false, () => true);

            //Give the logging time to finish
            Thread.Sleep(1000);
            var logEntries = appender.GetEvents();
            Log4NetStandardLogListener.Clear();
            LogManager.Manager.ClearListeners();
            Assert.AreEqual(1, logEntries.Length);
            Assert.AreEqual(errorMessage, logEntries.Single().RenderedMessage);
        }

        [TestMethod]
        public async Task TransportHost_SendsSuccessfully()
        {
            _mockNServiceBusServer.Setup(m => m.Send(It.IsAny<string>(), It.IsAny<IWorkflowMessage>())).Returns(Task.FromResult(true));
            await _transportHost.SendAsync("tenantId", new NotificationMessage());
        }

        [TestMethod]
        public void TransportHost_StopsSuccessfully()
        {
            _mockNServiceBusServer.Setup(m => m.Stop()).Returns(Task.FromResult(true));
            _transportHost.Stop();
        }
    }
}
