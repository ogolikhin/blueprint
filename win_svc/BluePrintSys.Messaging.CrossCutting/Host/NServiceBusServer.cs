using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BluePrintSys.Messaging.CrossCutting.Configuration;
using BluePrintSys.Messaging.CrossCutting.Logging;
using NServiceBus;
using NServiceBus.Transport.SQLServer;
using ServiceLibrary.Models.Enums;
using ServiceLibrary.Models.Workflow;

namespace BluePrintSys.Messaging.CrossCutting.Host
{
    public abstract class NServiceBusServer<TDerivedType> where TDerivedType : INServiceBusServer, new()
    {
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
                            _instance = (TDerivedType) Activator.CreateInstance(typeof(TDerivedType), true);
                        }
                    }
                }
                return _instance;
            }
        }

        protected IConfigHelper ConfigHelper { get; set; }
        protected string MessageQueue { get; set; }
        protected string ConnectionString { get; private set; }

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

            var messageBroker = ConfigHelper.GetMessageBroker();
            if (messageBroker == MessageBroker.RabbitMQ)
            {
                Log.Info("Configuring RabbitMQ Transport");
                var transport = endpointConfiguration.UseTransport<RabbitMQTransport>();
                transport.ConnectionString(connectionString);
                assembliesToExclude.Add("nservicebus.transport.sqlserver.dll");
            }
            else if (messageBroker == MessageBroker.SQL)
            {
                Log.Info("Configuring SQL Server Transport");
                var transport = endpointConfiguration.UseTransport<SqlServerTransport>();
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
            recoverability.Immediate(immediate =>
            {
                immediate.NumberOfRetries(6);
            });
            recoverability.Delayed(delayed =>
            {
                delayed.NumberOfRetries(5);
                delayed.TimeIncrease(TimeSpan.FromMinutes(10));
            });

            var loggerDefinition = NServiceBus.Logging.LogManager.Use<LoggerDefinition>();
            loggerDefinition.Level(NServiceBus.Logging.LogLevel.Warn);

            EndpointInstance = await Endpoint.Start(endpointConfiguration);
            Log.Debug($"Started Endpoint for connection string: {connectionString} for message queue {MessageQueue} for message broker {messageBroker}");
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

        public async Task Send(string tenantId, IWorkflowMessage message)
        {
            try
            {
                if (EndpointInstance == null)
                {
                    return;
                }
                var options = new SendOptions();
                options.SetDestination(MessageQueue);
                options.SetHeader(ActionMessageHeaders.TenantId, tenantId);

                LogInfo(tenantId, message, null);
                await EndpointInstance.Send(message, options);
            }
            catch (Exception ex)
            {
                LogInfo(tenantId, message, ex);
                throw;
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1305:SpecifyIFormatProvider", MessageId = "System.String.Format(System.String,System.Object,System.Object)")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1305:SpecifyIFormatProvider", MessageId = "System.String.Format(System.String,System.Object)")]
        protected virtual void LogInfo(string tenantId, IWorkflowMessage message, Exception exception)
        {
            if (exception == null)
            {
                Log.Info($"Sending message for tenant {tenantId}");
            }
            else
            {
                Log.Error($"Failed to send message for tenant {tenantId} due to an exception: {exception.Message}", exception);
            }
        }
    }

    public class GenericServiceBusServer : NServiceBusServer<GenericServiceBusServer>, INServiceBusServer
    {
        protected override Dictionary<MessageActionType, Type> GetMessageActionToHandlerMapping()
        {
            return new Dictionary <MessageActionType, Type>();
        }

        public GenericServiceBusServer()
        {
            ConfigHelper = new ConfigHelper();
            MessageQueue = ConfigHelper.MessageQueue;
        }
    }
}
