﻿using System.Configuration;
using System.Threading.Tasks;
using NServiceBus;

namespace NServiceBusSpike
{
    public class EndpointCreator
    {
        private const string LicenseInfo = "<?xml version=\"1.0\" encoding=\"utf-8\"?><license id=\"c79869c4-f819-48fd-8988-f0d0fcf637ac\" expiration=\"2117-04-12T18:43:05.7462219\" type=\"Standard\" ProductName=\"Royalty Free Platform License\" WorkerThreads=\"Max\" LicenseVersion=\"6.0\" MaxMessageThroughputPerSecond=\"Max\" AllowedNumberOfWorkerNodes=\"Max\" UpgradeProtectionExpiration=\"2018-04-12\" Applications=\"NServiceBus;ServiceControl;ServicePulse;\" LicenseType=\"Royalty Free Platform License\" Perpetual=\"\" Quantity=\"1\" Edition=\"Advanced \">  <name>Blueprint Software Systems</name>  <Signature xmlns=\"http://www.w3.org/2000/09/xmldsig#\">    <SignedInfo>      <CanonicalizationMethod Algorithm=\"http://www.w3.org/TR/2001/REC-xml-c14n-20010315\" />      <SignatureMethod Algorithm=\"http://www.w3.org/2000/09/xmldsig#rsa-sha1\" />      <Reference URI=\"\">        <Transforms>          <Transform Algorithm=\"http://www.w3.org/2000/09/xmldsig#enveloped-signature\" />        </Transforms>        <DigestMethod Algorithm=\"http://www.w3.org/2000/09/xmldsig#sha1\" />        <DigestValue>4fPcuVF4dP8Spy8GgrR+ebjWp8k=</DigestValue>      </Reference>    </SignedInfo>    <SignatureValue>3Q6bMQl5xsD/jzxmQjE5ji/DfP6kOqjvsrOiDiiawr3hHF9EDCdCHAPOBwmOp5zD/vLAS83baqGF23AVcwAXo75GxJNHuuxRkRuhPuL8gX8pNBC+5opaQvKkR/lZ32cErg/+sdY5SHSik2io1QGFe7IclykFhtcSLkGFi4wZ5EM=</SignatureValue>  </Signature></license>";
        public static async Task<IEndpointInstance> CreateEndPoint(string connectionString, string queueName)
        {
            var endpointConfiguration = new EndpointConfiguration(queueName);
            //var performanceCounters = endpointConfiguration.EnableWindowsPerformanceCounters();
            //performanceCounters.EnableSLAPerformanceCounters(TimeSpan.FromMinutes(3));
            //performanceCounters.UpdateCounterEvery(TimeSpan.FromSeconds(2));

            var transport = endpointConfiguration.UseTransport<RabbitMQTransport>();
            transport.ConnectionString(connectionString);
            // for RabbitMQ transport only
            var delayedDelivery = transport.DelayedDelivery();
            delayedDelivery.DisableTimeoutManager();

            var assemblyScanner = endpointConfiguration.AssemblyScanner();
            assemblyScanner.ExcludeAssemblies("Common.dll", "NServiceBus.Persistence.Sql.dll");
            assemblyScanner.ExcludeTypes(typeof(ArtifactsPublishedMessageHandler));

            endpointConfiguration.UsePersistence<InMemoryPersistence>();
            endpointConfiguration.UseSerialization<JsonSerializer>();
            endpointConfiguration.EnableInstallers();
            endpointConfiguration.LimitMessageProcessingConcurrencyTo(1);
            endpointConfiguration.SendFailedMessagesTo("errors");
            endpointConfiguration.License(LicenseInfo);
            endpointConfiguration.SendOnly();

            return await Endpoint.Start(endpointConfiguration);
        }
    }
}
