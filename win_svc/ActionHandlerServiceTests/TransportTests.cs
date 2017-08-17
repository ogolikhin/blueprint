using System.Linq;
using System.Threading;
using ActionHandlerService;
using ActionHandlerService.Helpers;
using ActionHandlerService.Logging;
using ActionHandlerService.Models;
using ActionHandlerService.Models.Enums;
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
        public void MessageTransportHostFactory_ReturnsRabbitMQTransportHost_WhenMessageBrokerIsRabbitMQ()
        {
            var configHelperMock = new Mock<IConfigHelper>();
            configHelperMock.Setup(m => m.MessageBroker).Returns(MessageBroker.RabbitMQ);
            var messageTransportHost = MessageTransportHostFactory.GetMessageTransportHost(configHelperMock.Object);
            Assert.IsTrue(messageTransportHost is RabbitMqTransportHost);
        }

        [TestMethod]
        public void MessageTransportHostFactory_ReturnsSqlTransportHost_WhenMessageBrokerIsSQL()
        {
            var configHelperMock = new Mock<IConfigHelper>();
            configHelperMock.Setup(m => m.MessageBroker).Returns(MessageBroker.SQL);
            var messageTransportHost = MessageTransportHostFactory.GetMessageTransportHost(configHelperMock.Object);
            Assert.IsTrue(messageTransportHost is SqlTransportHost);
        }

        [TestMethod]
        public void RabbitMQTransportHost_StartsSuccessfully()
        {
            var nServiceBusServerMock = new Mock<INServiceBusServer>();
            var rabbitMqTransportHost = new RabbitMqTransportHost(null, nServiceBusServerMock.Object);
            rabbitMqTransportHost.Start();
        }

        [TestMethod]
        public void RabbitMQTransportHost_LogsError_WhenStartFails()
        {
            LogManager.Manager.AddListener(Log4NetStandardLogListener.Instance);
            var appender = new MemoryAppender();
            log4net.Config.BasicConfigurator.Configure(appender);

            var nServiceBusServerMock = new Mock<INServiceBusServer>();
            const string errorMessage = "error message";
            nServiceBusServerMock.Setup(m => m.Start(It.IsAny<string>())).ReturnsAsync(errorMessage);
            var rabbitMqTransportHost = new RabbitMqTransportHost(null, nServiceBusServerMock.Object);
            rabbitMqTransportHost.Start(() => true);

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
            var rabbitMqTransportHost = new RabbitMqTransportHost(null, nServiceBusServerMock.Object);
            rabbitMqTransportHost.Stop();
        }

        [TestMethod]
        public void SqlTransportHost_StartsSuccessfully()
        {
            var nServiceBusServerMock = new Mock<INServiceBusServer>();
            var sqlTransportHost = new SqlTransportHost(null, nServiceBusServerMock.Object);
            sqlTransportHost.Start();
        }

        [TestMethod]
        public void SqlTransportHost_LogsError_WhenStartFails()
        {
            LogManager.Manager.AddListener(Log4NetStandardLogListener.Instance);
            var appender = new MemoryAppender();
            log4net.Config.BasicConfigurator.Configure(appender);

            var nServiceBusServerMock = new Mock<INServiceBusServer>();
            const string errorMessage = "error message";
            nServiceBusServerMock.Setup(m => m.Start(It.IsAny<string>())).ReturnsAsync(errorMessage);
            var sqlTransportHost = new SqlTransportHost(null, nServiceBusServerMock.Object);
            sqlTransportHost.Start(() => true);

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
            var sqlTransportHost = new SqlTransportHost(null, nServiceBusServerMock.Object);
            sqlTransportHost.Stop();
        }
    }
}
