﻿using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using BluePrintSys.Messaging.CrossCutting.Configuration;
using BluePrintSys.Messaging.CrossCutting.Logging;
using NServiceBus;
using NServiceBus.Transport;
using NServiceBus.Transport.SQLServer;
using RabbitMQ.Client.Exceptions;
using ServiceLibrary.Exceptions;
using ServiceLibrary.Models.Enums;
using ServiceLibrary.Models.Workflow;
using BluePrintSys.Messaging.Models.Actions;

namespace BluePrintSys.Messaging.CrossCutting.Host
{
    public abstract class NServiceBusServer<TDerivedType> where TDerivedType : INServiceBusServer, new()
    {
        protected NServiceBusServer()
        {
            ConfigHelper = new ConfigHelper();
            MessageQueue = ConfigHelper.MessageQueue;
            SendTimeoutSeconds = ConfigHelper.NServiceBusSendTimeoutSeconds;
        }

        private static readonly object Locker = new object();
        private static TDerivedType _instance;
        protected abstract Dictionary<MessageActionType, Type> GetMessageActionToHandlerMapping();

        public static TDerivedType Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (Locker)
                    {
                        if (_instance == null)
                        {
                            _instance = (TDerivedType)Activator.CreateInstance(typeof(TDerivedType), true);
                        }
                    }
                }
                return _instance;
            }
        }

        protected IConfigHelper ConfigHelper { get; set; }
        protected string MessageQueue { get; set; }
        protected string ConnectionString { get; private set; }
        protected int SendTimeoutSeconds { get; set; }
        protected const string LicenseInfo = "<?xml version=\"1.0\" encoding=\"utf-8\"?><license id=\"c79869c4-f819-48fd-8988-f0d0fcf637ac\" expiration=\"2117-04-12T18:43:05.7462219\" type=\"Standard\" ProductName=\"Royalty Free Platform License\" WorkerThreads=\"Max\" LicenseVersion=\"6.0\" MaxMessageThroughputPerSecond=\"Max\" AllowedNumberOfWorkerNodes=\"Max\" UpgradeProtectionExpiration=\"2018-04-12\" Applications=\"NServiceBus;ServiceControl;ServicePulse;\" LicenseType=\"Royalty Free Platform License\" Perpetual=\"\" Quantity=\"1\" Edition=\"Advanced \">  <name>Blueprint Software Systems</name>  <Signature xmlns=\"http://www.w3.org/2000/09/xmldsig#\">    <SignedInfo>      <CanonicalizationMethod Algorithm=\"http://www.w3.org/TR/2001/REC-xml-c14n-20010315\" />      <SignatureMethod Algorithm=\"http://www.w3.org/2000/09/xmldsig#rsa-sha1\" />      <Reference URI=\"\">        <Transforms>          <Transform Algorithm=\"http://www.w3.org/2000/09/xmldsig#enveloped-signature\" />        </Transforms>        <DigestMethod Algorithm=\"http://www.w3.org/2000/09/xmldsig#sha1\" />        <DigestValue>4fPcuVF4dP8Spy8GgrR+ebjWp8k=</DigestValue>      </Reference>    </SignedInfo>    <SignatureValue>3Q6bMQl5xsD/jzxmQjE5ji/DfP6kOqjvsrOiDiiawr3hHF9EDCdCHAPOBwmOp5zD/vLAS83baqGF23AVcwAXo75GxJNHuuxRkRuhPuL8gX8pNBC+5opaQvKkR/lZ32cErg/+sdY5SHSik2io1QGFe7IclykFhtcSLkGFi4wZ5EM=</SignatureValue>  </Signature></license>";
        protected IEndpointInstance EndpointInstance { get; set; }

        public async Task Stop()
        {
            await EndpointInstance.Stop().ConfigureAwait(false);
        }

        public async Task<string> Start(string connectionString, bool sendOnly)
        {
            if (string.IsNullOrEmpty(connectionString))
            {
                return new ArgumentNullException(nameof(connectionString)).Message;
            }
            ConnectionString = connectionString;

            try
            {
                await CreateEndPoint(MessageQueue, connectionString, sendOnly);
                Log.Debug("Started Messaging Endpoint");
                return null;
            }
            catch (Exception ex)
            {
                Log.Error("Could not start up messaging endpoint", ex);
                return ex.Message;
            }
        }

        private async Task CreateEndPoint(string name, string connectionString, bool sendOnly)
        {
            var endpointConfiguration = new EndpointConfiguration(name);
            var assembliesToExclude = new HashSet<string>
            {
                "Common.dll",
                "NServiceBus.Persistence.Sql.dll",
                "BluePrintSys.Messaging.CrossCutting.dll",
                "Dapper.StrongName.dll",
                "MailBee.NET.4.dll"
            };

            var transportType = NServiceBusValidator.GetTransportType(connectionString);
            if (transportType == NServiceBusTransportType.RabbitMq)
            {
                Log.Info($"Configuring RabbitMQ Transport for {connectionString}");
                var transport = endpointConfiguration.UseTransport<RabbitMQTransport>();
                transport.ConnectionString(connectionString);
                assembliesToExclude.Add("nservicebus.transport.sqlserver.dll");
            }
            else if (transportType == NServiceBusTransportType.Sql)
            {
                Log.Info($"Configuring SQL Server Transport for {connectionString}");
                var transport = endpointConfiguration.UseTransport<SqlServerTransport>();
                transport.Transactions(TransportTransactionMode.SendsAtomicWithReceive);
                transport.ConnectionString(connectionString);
                transport.DefaultSchema("queue");
                assembliesToExclude.Add("nservicebus.transports.rabbitmq.dll");
            }

            var assemblyScanner = endpointConfiguration.AssemblyScanner();
            assemblyScanner.ExcludeAssemblies(assembliesToExclude.ToArray());
            ExcludeMessageHandlers(assemblyScanner);

            endpointConfiguration.UsePersistence<InMemoryPersistence>();
            endpointConfiguration.UseSerialization<JsonSerializer>();
            endpointConfiguration.EnableInstallers();
            endpointConfiguration.LimitMessageProcessingConcurrencyTo(ConfigHelper.MessageProcessingMaxConcurrency);
            endpointConfiguration.SendFailedMessagesTo(ConfigHelper.ErrorQueue);
            endpointConfiguration.License(LicenseInfo);
            if (sendOnly)
            {
                endpointConfiguration.SendOnly();
            }
            var recoverability = endpointConfiguration.Recoverability();
            recoverability.DisableLegacyRetriesSatellite();
            recoverability.AddUnrecoverableException<WebhookExceptionDoNotRetry>();
            recoverability.CustomPolicy(WebhookRetryPolicy);
            recoverability.Immediate(immediate =>
            {
                immediate.NumberOfRetries(6);
            });
            recoverability.Delayed(delayed =>
            {
                delayed.NumberOfRetries(5);
                delayed.TimeIncrease(TimeSpan.FromMinutes(10));
            });

            var conventions = endpointConfiguration.Conventions();
            conventions.DefiningTimeToBeReceivedAs(
                type => {
                    if (type == typeof(StatusCheckMessage))
                    {
                        return TimeSpan.FromMinutes(1);
                    }
                    return TimeSpan.MaxValue;
            });

            var loggerDefinition = NServiceBus.Logging.LogManager.Use<LoggerDefinition>();
            loggerDefinition.Level(NServiceBus.Logging.LogLevel.Warn);

            EndpointInstance = await Endpoint.Start(endpointConfiguration);
            Log.Debug($"Started Endpoint for connection string: {connectionString} for message queue {MessageQueue} for message broker {transportType}");
        }

        private void ExcludeMessageHandlers(AssemblyScannerConfiguration assemblyScanner)
        {
            var supportedMessageTypes = ConfigHelper.SupportedActionTypes;
            if (supportedMessageTypes == MessageActionType.All)
            {
                return;
            }
            foreach (MessageActionType value in Enum.GetValues(typeof(MessageActionType)))
            {
                if (value == MessageActionType.All)
                {
                    continue;
                }
                if ((supportedMessageTypes & value) != value)
                {
                    if (GetMessageActionToHandlerMapping().ContainsKey(value))
                    {
                        assemblyScanner.ExcludeTypes(GetMessageActionToHandlerMapping()[value]);
                    }
                }
            }
        }

        public async Task GetStatus(StatusCheckMessage message)
        {
            var options = new SendOptions();
            options.SetDestination(MessageQueue);
            await EndpointInstance.Send(message, options);
        }

        public async Task Send(string tenantId, IWorkflowMessage message)
        {
            if (EndpointInstance == null)
            {
                // If the Endpoint Instance is null, throw an exception so the user gets an error message
                throw new Exception($"EndpointInstance is null. {message.ActionType} could not be sent for tenant ID {tenantId}");
            }

            var options = new SendOptions();
            options.SetDestination(MessageQueue);
            options.SetHeader(ActionMessageHeaders.TenantId, tenantId);

            Log.Info($"Sending Action Message {message.ActionType} for tenant {tenantId}");
            var sendTask = EndpointInstance.Send(message, options);

            if (await Task.WhenAny(sendTask, Task.Delay(TimeSpan.FromSeconds(SendTimeoutSeconds))) == sendTask)
            {
                if (sendTask.IsFaulted)
                {
                    var aggregateException = sendTask.Exception;
                    Log.Error("Send failed for Action Message due to an exception", aggregateException);
                    var innerException = aggregateException?.InnerException;
                    if (innerException != null)
                    {
                        Log.Error("Inner Exception", innerException);
                        if (innerException is SqlException)
                        {
                            throw new SqlServerSendException(innerException);
                        }
                        if (innerException is BrokerUnreachableException)
                        {
                            throw new RabbitMqSendException(innerException);
                        }
                        throw innerException;
                    }
                    if (aggregateException != null)
                    {
                        throw aggregateException;
                    }
                    throw new Exception("Send failed for Action Message");
                }
                Log.Info($"Action Message sent successfully for tenant {tenantId}");
            }
            else
            {
                var errorMessage = $"Send failed for Action Message due to a timeout after {SendTimeoutSeconds} seconds.";
                Log.Error(errorMessage);
                throw new Exception(errorMessage);
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1305:SpecifyIFormatProvider", MessageId = "System.String.Format(System.String,System.Object)")]
        public RecoverabilityAction WebhookRetryPolicy(RecoverabilityConfig config, ErrorContext context)
        {
            if (context.Exception is WebhookExceptionRetryPerPolicy)
            {
                // Check that the number of delayed deliveries does not exceed configurable webhook retry count
                if (context.DelayedDeliveriesPerformed < ConfigHelper.WebhookRetryCount)
                {
                    // Set delayed retry internal to that set by the configurable webhook retry internal
                    return RecoverabilityAction.DelayedRetry(TimeSpan.FromSeconds(ConfigHelper.WebhookRetryInterval));
                }
                // If the webhook could not be delivered within the specified number of retry attempts. Log error and send to ErrorQueue
                string errorMsg = $"Failed to send webhook after {context.DelayedDeliveriesPerformed} attempts.";
                Log.Error(errorMsg);
                return RecoverabilityAction.MoveToError(errorMsg);
            }
            // For all other exceptions, fall back to default policy
            return DefaultRecoverabilityPolicy.Invoke(config, context);
        }
    }

    public class GenericServiceBusServer : NServiceBusServer<GenericServiceBusServer>, INServiceBusServer
    {
        protected override Dictionary<MessageActionType, Type> GetMessageActionToHandlerMapping()
        {
            return new Dictionary<MessageActionType, Type>();
        }
    }
}
