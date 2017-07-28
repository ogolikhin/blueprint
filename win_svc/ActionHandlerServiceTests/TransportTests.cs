using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
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
            var result = Task.FromResult(errorMessage);
            nServiceBusServerMock.Setup(m => m.Start(It.IsAny<string>())).Returns(result);
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
        [ExpectedException(typeof(NotImplementedException))]
        public void SqlTransportHost_StartThrowsNotImplementedExceptionForNow()
        {
            var sqlTransportHost = new SqlTransportHost();
            sqlTransportHost.Start();
        }

        [TestMethod]
        [ExpectedException(typeof(NotImplementedException))]
        public void SqlTransportHost_StopThrowsNotImplementedExceptionForNow()
        {
            var sqlTransportHost = new SqlTransportHost();
            sqlTransportHost.Stop();
        }
    }
}
