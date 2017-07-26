﻿using ActionHandlerService.Helpers;
using ActionHandlerService.Models.Enums;
using BluePrintSys.Messaging.Models.Actions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ActionHandlerServiceTests
{
    /// <summary>
    /// Tests for the Config Helper in the Action Handler Service
    /// </summary>
    [TestClass]
    public class ConfigHelperTests
    {
        private readonly ConfigHelper _configHelper;

        public ConfigHelperTests()
        {
            _configHelper = new ConfigHelper();
        }
        
        [TestMethod]
        public void ConfigHelper_ReturnsDefault_WhenGettingCacheExpirationMinutes()
        {
            var configValue = _configHelper.CacheExpirationMinutes;
            const int defaultValue = ConfigHelper.CacheExpirationMinutesDefault;
            Assert.AreEqual(defaultValue, configValue);
        }

        [TestMethod]
        public void ConfigHelper_ReturnsDefault_WhenGettingErrorQueue()
        {
            var configValue = _configHelper.ErrorQueue;
            const string defaultValue = ConfigHelper.ErrorQueueDefault;
            Assert.AreEqual(defaultValue, configValue);
        }

        [TestMethod]
        public void ConfigHelper_ReturnsDefault_WhenGettingServiceName()
        {
            var configValue = _configHelper.ServiceName;
            const string defaultValue = ConfigHelper.ServiceNameDefault;
            Assert.AreEqual(defaultValue, configValue);
        }

        [TestMethod]
        public void ConfigHelper_ReturnsDefault_WhenGettingSingleTenancyConnectionString()
        {
            var configValue = _configHelper.SingleTenancyConnectionString;
            const string defaultValue = null;
            Assert.AreEqual(defaultValue, configValue);
        }

        [TestMethod]
        public void ConfigHelper_ReturnsDefault_WhenGettingMessageBroker()
        {
            var configValue = _configHelper.MessageBroker;
            const MessageBroker defaultValue = ConfigHelper.MessageBrokerDefault;
            Assert.AreEqual(defaultValue, configValue);
        }

        [TestMethod]
        public void ConfigHelper_ReturnsDefault_WhenGettingMessageProcessingMaxConcurrency()
        {
            var configValue = _configHelper.MessageProcessingMaxConcurrency;
            const int defaultValue = ConfigHelper.MessageProcessingMaxConcurrencyDefault;
            Assert.AreEqual(defaultValue, configValue);
        }

        [TestMethod]
        public void ConfigHelper_ReturnsDefault_WhenGettingMessageQueue()
        {
            var configValue = _configHelper.MessageQueue;
            const string defaultValue = ConfigHelper.MessageQueueDefault;
            Assert.AreEqual(defaultValue, configValue);
        }

        [TestMethod]
        public void ConfigHelper_ReturnsDefault_WhenGettingNServiceBusConnectionString()
        {
            var configValue = _configHelper.NServiceBusConnectionString;
            const string defaultValue = ConfigHelper.NServiceBusConnectionStringDefault;
            Assert.AreEqual(defaultValue, configValue);
        }

        [TestMethod]
        public void ConfigHelper_ReturnsDefault_WhenGettingNServiceBusInstanceId()
        {
            var configValue = _configHelper.NServiceBusInstanceId;
            const string defaultValue = ConfigHelper.NServiceBusInstanceIdDefault;
            Assert.AreEqual(defaultValue, configValue);
        }

        [TestMethod]
        public void ConfigHelper_ReturnsDefault_WhenGettingSupportedActionTypes()
        {
            var configValue = _configHelper.SupportedActionTypes;
            const MessageActionType defaultValue = ConfigHelper.SupportedActionTypesDefault;
            Assert.AreEqual(defaultValue, configValue);
        }

        [TestMethod]
        public void ConfigHelper_ReturnsDefault_WhenGettingTenancy()
        {
            var configValue = _configHelper.Tenancy;
            const Tenancy defaultValue = ConfigHelper.TenancyDefault;
            Assert.AreEqual(defaultValue, configValue);
        }
    }
}
