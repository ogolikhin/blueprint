﻿using BlueprintSys.RC.Services.Helpers;
using BluePrintSys.Messaging.CrossCutting.Configuration;
using BluePrintSys.Messaging.CrossCutting.Host;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ServiceLibrary.Exceptions;
using ServiceLibrary.Models.Enums;

namespace BlueprintSys.RC.Services.Tests
{
    /// <summary>
    /// Tests for the Config Helper in the Action Handler Service
    /// </summary>
    [TestClass]
    public class ConfigHelperTests
    {
        private ConfigHelper _configHelper;
        private ExtendedConfigHelper _extendedConfigHelper;

        [TestInitialize]
        public void TestInitialize()
        {
            _configHelper = new ConfigHelper();
            _extendedConfigHelper = new ExtendedConfigHelper();
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
            var configValue = _extendedConfigHelper.ServiceName;
            const string defaultValue = ExtendedConfigHelper.ServiceNameDefault;
            Assert.AreEqual(defaultValue, configValue);
        }

        [TestMethod]
        public void ConfigHelper_ReturnsDefault_WhenGettingSingleTenancyConnectionString()
        {
            var configValue = _configHelper.TenantsDatabase;
            const string defaultValue = null;
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
        [ExpectedException(typeof(NServiceBusConnectionException))]
        public void NServiceBusValidator_ThrowsException_WhenGettingTransportTypeForEmptyConnectionString()
        {
            var connectionString = string.Empty;
            var configValue = NServiceBusValidator.GetTransportType(connectionString);
            Assert.IsTrue(configValue != NServiceBusTransportType.RabbitMq && configValue != NServiceBusTransportType.Sql);
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
    }
}
