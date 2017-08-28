using System.Linq;
using System.Threading;
using ActionHandlerService.Logging;
using BluePrintSys.Messaging.CrossCutting.Configuration;
using BluePrintSys.Messaging.CrossCutting.Host;
using BluePrintSys.Messaging.CrossCutting.Logging;
using log4net.Appender;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace ActionHandlerServiceTests
{
    /// <summary>
    /// Tests for Action Handler Service Transport
    /// </summary>
    [TestClass]
    public class TransportTests
    {
        [TestMethod]
        public void RabbitMQTransportHost_StartsSuccessfully()
        {
            var nServiceBusServerMock = new Mock<INServiceBusServer>();
            var rabbitMqTransportHost = new TransportHost(null, nServiceBusServerMock.Object);
            rabbitMqTransportHost.Start(false);
        }

        [TestMethod]
        public void RabbitMQTransportHost_LogsError_WhenStartFails()
        {
            LogManager.Manager.AddListener(Log4NetStandardLogListener.Instance);
            var appender = new MemoryAppender();
            log4net.Config.BasicConfigurator.Configure(appender);

            var nServiceBusServerMock = new Mock<INServiceBusServer>();
            const string errorMessage = "error message";
            nServiceBusServerMock.Setup(m => m.Start(It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(errorMessage);
            var rabbitMqTransportHost = new TransportHost(null, nServiceBusServerMock.Object);
            rabbitMqTransportHost.Start(false, () => true);

            //Give the logging time to finish
            Thread.Sleep(1000);
            var logEntries = appender.GetEvents();
            Log4NetStandardLogListener.Clear();
            LogManager.Manager.ClearListeners();
            Assert.AreEqual(1, logEntries.Length);
            Assert.AreEqual(errorMessage, logEntries.Single().RenderedMessage);
        }

        [TestMethod]
        public void RabbitMQTransportHost_StopsSuccessfully()
        {
            var nServiceBusServerMock = new Mock<INServiceBusServer>();
            var rabbitMqTransportHost = new TransportHost(null, nServiceBusServerMock.Object);
            rabbitMqTransportHost.Stop();
        }

        [TestMethod]
        public void SqlTransportHost_StartsSuccessfully()
        {
            var nServiceBusServerMock = new Mock<INServiceBusServer>();
            var sqlTransportHost = new TransportHost(null, nServiceBusServerMock.Object);
            sqlTransportHost.Start(false);
        }

        [TestMethod]
        public void SqlTransportHost_LogsError_WhenStartFails()
        {
            LogManager.Manager.AddListener(Log4NetStandardLogListener.Instance);
            var appender = new MemoryAppender();
            log4net.Config.BasicConfigurator.Configure(appender);

            var nServiceBusServerMock = new Mock<INServiceBusServer>();
            const string errorMessage = "error message";
            nServiceBusServerMock.Setup(m => m.Start(It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(errorMessage);
            var sqlTransportHost = new TransportHost(null, nServiceBusServerMock.Object);
            sqlTransportHost.Start(false, () => true);

            //Give the logging time to finish
            Thread.Sleep(1000);
            var logEntries = appender.GetEvents();
            Log4NetStandardLogListener.Clear();
            LogManager.Manager.ClearListeners();
            Assert.AreEqual(1, logEntries.Length);
            Assert.AreEqual(errorMessage, logEntries.Single().RenderedMessage);
        }

        [TestMethod]
        public void SqlTransportHost_StopsSuccessfully()
        {
            var nServiceBusServerMock = new Mock<INServiceBusServer>();
            var sqlTransportHost = new TransportHost(null, nServiceBusServerMock.Object);
            sqlTransportHost.Stop();
        }
    }
}
