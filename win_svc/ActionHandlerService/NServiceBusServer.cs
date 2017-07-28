using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ActionHandlerService.Helpers;
using ActionHandlerService.MessageHandlers.ArtifactPublished;
using ActionHandlerService.MessageHandlers.GenerateDescendants;
using ActionHandlerService.MessageHandlers.GenerateTests;
using ActionHandlerService.MessageHandlers.GenerateUserStories;
using ActionHandlerService.MessageHandlers.Notifications;
using ActionHandlerService.MessageHandlers.PropertyChange;
using ActionHandlerService.MessageHandlers.StateTransition;
using BluePrintSys.Messaging.CrossCutting.Logging;
using BluePrintSys.Messaging.Models.Actions;
using NServiceBus;

namespace ActionHandlerService
{
    public interface INServiceBusServer
    {
        void Send(string tenantId, ActionMessage message);
        Task<string> Start(string connectionString);
        Task Stop();
    }

    public class NServiceBusServer : INServiceBusServer
    {
        private IConfigHelper ConfigHelper { get; }
        private string MessageQueue { get; }
        private const string LicenseInfo = "<?xml version=\"1.0\" encoding=\"utf-8\"?><license id=\"c79869c4-f819-48fd-8988-f0d0fcf637ac\" expiration=\"2117-04-12T18:43:05.7462219\" type=\"Standard\" ProductName=\"Royalty Free Platform License\" WorkerThreads=\"Max\" LicenseVersion=\"6.0\" MaxMessageThroughputPerSecond=\"Max\" AllowedNumberOfWorkerNodes=\"Max\" UpgradeProtectionExpiration=\"2018-04-12\" Applications=\"NServiceBus;ServiceControl;ServicePulse;\" LicenseType=\"Royalty Free Platform License\" Perpetual=\"\" Quantity=\"1\" Edition=\"Advanced \">  <name>Blueprint Software Systems</name>  <Signature xmlns=\"http://www.w3.org/2000/09/xmldsig#\">    <SignedInfo>      <CanonicalizationMethod Algorithm=\"http://www.w3.org/TR/2001/REC-xml-c14n-20010315\" />      <SignatureMethod Algorithm=\"http://www.w3.org/2000/09/xmldsig#rsa-sha1\" />      <Reference URI=\"\">        <Transforms>          <Transform Algorithm=\"http://www.w3.org/2000/09/xmldsig#enveloped-signature\" />        </Transforms>        <DigestMethod Algorithm=\"http://www.w3.org/2000/09/xmldsig#sha1\" />        <DigestValue>4fPcuVF4dP8Spy8GgrR+ebjWp8k=</DigestValue>      </Reference>    </SignedInfo>    <SignatureValue>3Q6bMQl5xsD/jzxmQjE5ji/DfP6kOqjvsrOiDiiawr3hHF9EDCdCHAPOBwmOp5zD/vLAS83baqGF23AVcwAXo75GxJNHuuxRkRuhPuL8gX8pNBC+5opaQvKkR/lZ32cErg/+sdY5SHSik2io1QGFe7IclykFhtcSLkGFi4wZ5EM=</SignatureValue>  </Signature></license>";
        private IEndpointInstance _endpointInstance;

        public static NServiceBusServer Instance = new NServiceBusServer();

        private NServiceBusServer(IConfigHelper configHelper = null)
        {
            ConfigHelper = configHelper ?? new ConfigHelper();
            MessageQueue = ConfigHelper.MessageQueue;
        }

        public async Task Stop()
        {
            await _endpointInstance.Stop().ConfigureAwait(false);
        }

        public async Task<string> Start(string connectionString)
        {
            if (string.IsNullOrEmpty(connectionString))
            {
                return new ArgumentNullException(nameof(connectionString)).Message;
            }

            try
            {
                await CreateEndPoint(MessageQueue, connectionString);
                return null;
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        private async Task CreateEndPoint(string name, string connectionString)
        {
            var endpointConfiguration = new EndpointConfiguration(name);

            var transport = endpointConfiguration.UseTransport<RabbitMQTransport>();
            transport.ConnectionString(connectionString);
            // for RabbitMQ transport only
            var delayedDelivery = transport.DelayedDelivery();
            delayedDelivery.DisableTimeoutManager();

            var assemblyScanner = endpointConfiguration.AssemblyScanner();
            assemblyScanner.ExcludeAssemblies("Common.dll", 
                "NServiceBus.Persistence.Sql.dll",
                "BluePrintSys.Messaging.CrossCutting.dll", 
                "Dapper.StrongName.dll");
            ExcludeMessageHandlers(assemblyScanner);

            endpointConfiguration.UsePersistence<InMemoryPersistence>();
            endpointConfiguration.UseSerialization<JsonSerializer>();
            endpointConfiguration.EnableInstallers();
            endpointConfiguration.LimitMessageProcessingConcurrencyTo(ConfigHelper.MessageProcessingMaxConcurrency);
            endpointConfiguration.SendFailedMessagesTo(ConfigHelper.ErrorQueue);
            endpointConfiguration.License(LicenseInfo);

            var loggerDefinition = NServiceBus.Logging.LogManager.Use<LoggerDefinition>();
            loggerDefinition.Level(NServiceBus.Logging.LogLevel.Warn);

            _endpointInstance = await Endpoint.Start(endpointConfiguration);
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
                    assemblyScanner.ExcludeTypes(NServiceBusServerHelper.MessageActionToHandlerMapping[value]);
                }
            }
        }

        public async void Send(string tenantId, ActionMessage message)
        {
            try
            {
                if (_endpointInstance == null)
                {
                    return;
                }
                var options = new SendOptions();
                options.SetDestination(MessageQueue);
                options.SetHeader(ActionMessageHeaders.TenantId, tenantId);
                Log.Info($"Sending {message.ActionType.ToString()} message for tenant {tenantId}");
                await _endpointInstance.Send(message, options);
            }
            catch (Exception ex)
            {
                Log.Error($"Failed to send {message.ActionType.ToString()} message for tenant {tenantId} due to an exception: {ex.Message}", ex);
            }
        }
    }

    class NServiceBusServerHelper
    {
        internal static readonly Dictionary<MessageActionType, Type> MessageActionToHandlerMapping = new Dictionary
            <MessageActionType, Type>()
        {
            {MessageActionType.ArtifactsPublished, typeof (ArtifactsPublishedMessageHandler)},
            {MessageActionType.GenerateDescendants, typeof (GenerateDescendantsMessageHandler)},
            {MessageActionType.GenerateTests, typeof (GenerateTestsMessageHandler)},
            {MessageActionType.GenerateUserStories, typeof (GenerateUserStoriesMessageHandler)},
            {MessageActionType.Notification, typeof (NotificationMessageHandler)},
            {MessageActionType.Property, typeof (PropertyChangeMessageHandler)},
            {MessageActionType.StateChange, typeof (StateTransitionMessageHandler)}
        };
    }
}
