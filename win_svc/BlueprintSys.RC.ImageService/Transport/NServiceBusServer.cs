﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using BlueprintSys.RC.ImageService.Helpers;
using BluePrintSys.Messaging.CrossCutting.Host;
using BluePrintSys.Messaging.CrossCutting.Logging;
using BluePrintSys.Messaging.Models.ProcessImageGeneration;
using NServiceBus;
using NServiceBus.Transport.SQLServer;

namespace BlueprintSys.RC.ImageService.Transport
{
    public class NServiceBusServer
    {
        private static string Handler = "ImageGen.ImageGenServer";
        private IEndpointInstance _endpointInstance;
        private readonly string _licenseInfo = "<?xml version=\"1.0\" encoding=\"utf-8\"?><license id=\"c79869c4-f819-48fd-8988-f0d0fcf637ac\" expiration=\"2117-04-12T18:43:05.7462219\" type=\"Standard\" ProductName=\"Royalty Free Platform License\" WorkerThreads=\"Max\" LicenseVersion=\"6.0\" MaxMessageThroughputPerSecond=\"Max\" AllowedNumberOfWorkerNodes=\"Max\" UpgradeProtectionExpiration=\"2018-04-12\" Applications=\"NServiceBus;ServiceControl;ServicePulse;\" LicenseType=\"Royalty Free Platform License\" Perpetual=\"\" Quantity=\"1\" Edition=\"Advanced \">  <name>Blueprint Software Systems</name>  <Signature xmlns=\"http://www.w3.org/2000/09/xmldsig#\">    <SignedInfo>      <CanonicalizationMethod Algorithm=\"http://www.w3.org/TR/2001/REC-xml-c14n-20010315\" />      <SignatureMethod Algorithm=\"http://www.w3.org/2000/09/xmldsig#rsa-sha1\" />      <Reference URI=\"\">        <Transforms>          <Transform Algorithm=\"http://www.w3.org/2000/09/xmldsig#enveloped-signature\" />        </Transforms>        <DigestMethod Algorithm=\"http://www.w3.org/2000/09/xmldsig#sha1\" />        <DigestValue>4fPcuVF4dP8Spy8GgrR+ebjWp8k=</DigestValue>      </Reference>    </SignedInfo>    <SignatureValue>3Q6bMQl5xsD/jzxmQjE5ji/DfP6kOqjvsrOiDiiawr3hHF9EDCdCHAPOBwmOp5zD/vLAS83baqGF23AVcwAXo75GxJNHuuxRkRuhPuL8gX8pNBC+5opaQvKkR/lZ32cErg/+sdY5SHSik2io1QGFe7IclykFhtcSLkGFi4wZ5EM=</SignatureValue>  </Signature></license>";
        private static readonly string NServiceBusInstanceId = ServiceHelper.NServiceBusInstanceId;

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
                await CreateEndPoint(Handler, connectionString);
                return null;
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        private async Task CreateEndPoint(string name, string connectionString)
        {
            NServiceBusTransportType transportType = NServiceBusValidator.GetTransportType(connectionString);
            var instanceId = NServiceBusInstanceId;
            if (string.IsNullOrEmpty(instanceId))
            {
                instanceId = $"{Dns.GetHostName()}-{ServiceHelper.ServiceName}";
            }
            var endpointConfiguration = new EndpointConfiguration(name);
            endpointConfiguration.MakeInstanceUniquelyAddressable(instanceId);

            object transport;
            if (transportType == NServiceBusTransportType.RabbitMq)
            {
                transport = endpointConfiguration.UseTransport<RabbitMQTransport>();
                ((TransportExtensions<RabbitMQTransport>)transport).ConnectionString(connectionString);
                // for RabbitMQ transport only
                var delayedDelivery = ((TransportExtensions<RabbitMQTransport>)transport).DelayedDelivery();
                delayedDelivery.DisableTimeoutManager();
            }
            else if (transportType == NServiceBusTransportType.Sql)
            {
                transport = endpointConfiguration.UseTransport<SqlServerTransport>();
                ((TransportExtensions<SqlServerTransport>)transport).Transactions(TransportTransactionMode.SendsAtomicWithReceive);
                ((TransportExtensions<SqlServerTransport>)transport).ConnectionString(connectionString);
                ((TransportExtensions<SqlServerTransport>)transport).DefaultSchema("queue");
            }

            var assemblyScanner = endpointConfiguration.AssemblyScanner();
            HashSet<string> assembliesToExclude = new HashSet<string> { "Common.dll", "NServiceBus.Persistence.Sql.dll" };
            if (transportType == NServiceBusTransportType.RabbitMq)
            {
                assembliesToExclude.Add("nservicebus.transport.sqlserver.dll");
            }
            else if (transportType == NServiceBusTransportType.Sql)
            {
                assembliesToExclude.Add("nservicebus.transports.rabbitmq.dll");
            }
            assemblyScanner.ExcludeAssemblies(assembliesToExclude.ToArray());

            endpointConfiguration.UsePersistence<InMemoryPersistence>();
            endpointConfiguration.UseSerialization<JsonSerializer>();
            endpointConfiguration.EnableInstallers();
            endpointConfiguration.EnableCallbacks(false);
            endpointConfiguration.LimitMessageProcessingConcurrencyTo(ServiceHelper.BrowserPoolMaxSize);
            endpointConfiguration.SendFailedMessagesTo("errors");
            endpointConfiguration.License(_licenseInfo);

            var conventions = endpointConfiguration.Conventions();
            conventions.DefiningTimeToBeReceivedAs(
                type =>
                {
                    if (type == typeof(GenerateImageMessage))
                    {
                        return TimeSpan.FromMinutes(5);
                    }
                    return TimeSpan.MaxValue;
                });

            var loggerDefinition = NServiceBus.Logging.LogManager.Use<LoggerDefinition>();
            loggerDefinition.Level(NServiceBus.Logging.LogLevel.Warn);

            _endpointInstance = await Endpoint.Start(endpointConfiguration);
        }


    }


}
