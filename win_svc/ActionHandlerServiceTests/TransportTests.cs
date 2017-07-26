using System;
using ActionHandlerService;
using ActionHandlerService.Helpers;
using ActionHandlerService.Models;
using ActionHandlerService.Models.Enums;
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
        public void MessageTransportHostFactory_InstantiatesSuccessfully()
        {
            var messageTransportHostFactory = new MessageTransportHostFactory();
            Assert.IsNotNull(messageTransportHostFactory);
        }

        [TestMethod]
        public void MessageTransportHostFactory_ReturnsRabbitMQTransportHost_WhenMessageBrokerIsRabbitMQ()
        {
            var configHelperMock = new Mock<IConfigHelper>();
            configHelperMock.Setup(m => m.MessageBroker).Returns(MessageBroker.RabbitMQ);
            var messageTransportHostFactory = new MessageTransportHostFactory(configHelperMock.Object);
            var messageTransportHost = messageTransportHostFactory.GetMessageTransportHost();
            Assert.IsTrue(messageTransportHost is RabbitMqTransportHost);
        }

        [TestMethod]
        public void MessageTransportHostFactory_ReturnsSqlTransportHost_WhenMessageBrokerIsSQL()
        {
            var configHelperMock = new Mock<IConfigHelper>();
            configHelperMock.Setup(m => m.MessageBroker).Returns(MessageBroker.SQL);
            var messageTransportHostFactory = new MessageTransportHostFactory(configHelperMock.Object);
            var messageTransportHost = messageTransportHostFactory.GetMessageTransportHost();
            Assert.IsTrue(messageTransportHost is SqlTransportHost);
        }

        [TestMethod]
        public void RabbitMQTransportHost_StartsAndStopsSuccessfully()
        {
            var nServiceBusServerMock = new Mock<INServiceBusServer>();
            var rabbitMqTransportHost = new RabbitMqTransportHost(null, nServiceBusServerMock.Object);
            rabbitMqTransportHost.Start();
            rabbitMqTransportHost.Stop();
        }

        [TestMethod]
        [ExpectedException(typeof(NotImplementedException))]
        public void SqlTransportHost_ThrowsNotImplementedExceptionForNow()
        {
            var sqlTransportHost = new SqlTransportHost();
            NotImplementedException ex = null;
            try
            {
                sqlTransportHost.Start();
            }
            catch (NotImplementedException e)
            {
                ex = e;
            }
            Assert.IsNotNull(ex);
            sqlTransportHost.Stop();
        }
    }
}
