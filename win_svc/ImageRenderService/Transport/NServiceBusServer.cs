using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImageRenderService.Helpers;
using NServiceBus;
using NServiceBus.Features;

//using JsonSerializer = Newtonsoft.Json.JsonSerializer;

namespace ImageRenderService.Transport
{
    public class NServiceBusServer
    {
        public static string Client = "ImageGen.Client";
        public static string Handler = "ImageGen.Handler";
        private IEndpointInstance _endpointInstance;

       /* public async void Start(string instanceId, string virtualHost)
        {
            Console.Title = $"Jobs.{virtualHost},{instanceId}";
            var name = "blueprintsys.job";
            var endpointConfiguration = new EndpointConfiguration(name);
            //endpointConfiguration.MakeInstanceUniquelyAddressable(instanceId);
            endpointConfiguration.ConfigureTransport(virtualHost);

            endpointConfiguration.ConfigurePersistence(name);
            //endpointConfiguration.DisableFeature<TimeoutManager>();

            endpointConfiguration.UseSerialization<JsonSerializer>();
            endpointConfiguration.EnableInstallers();
            //endpointConfiguration.UsePersistence<InMemoryPersistence>();
            endpointConfiguration.SendFailedMessagesTo("error");

            var startable = await Endpoint.Create(endpointConfiguration).ConfigureAwait(false);
            _endpointInstance = await startable.Start().ConfigureAwait(false);

        }*/

        public async Task Stop()
        {
            await _endpointInstance.Stop().ConfigureAwait(false);
        }

        /*public async Task Start(string connectionString, string instanceId)
        {
            if (string.IsNullOrEmpty(connectionString))
                throw new ArgumentNullException(nameof(connectionString));

            if (string.IsNullOrWhiteSpace(instanceId))
                throw new ArgumentException("Cannot be null or whitespace", nameof(instanceId));
            await CreateEndPoint(Client, connectionString, instanceId);

        }*/

        public async Task Start(string connectionString, string instanceId)
        {
            if (string.IsNullOrEmpty(connectionString))
                throw new ArgumentNullException(nameof(connectionString));

            if (string.IsNullOrWhiteSpace(instanceId))
                throw new ArgumentException("Cannot be null or whitespace", nameof(instanceId));

            await CreateEndPoint(Handler, connectionString, instanceId);
        }

        private async Task CreateEndPoint(string name, string connectionString, string instanceId)
        {
            var endpointConfiguration = new EndpointConfiguration(name);
            endpointConfiguration.MakeInstanceUniquelyAddressable(instanceId);

            var transport = endpointConfiguration.UseTransport<RabbitMQTransport>();
            transport.ConnectionString(connectionString);

            var assemblyScanner = endpointConfiguration.AssemblyScanner();
            assemblyScanner.ExcludeAssemblies("Common.dll", "NServiceBus.Persistence.Sql.dll");

            endpointConfiguration.UsePersistence<InMemoryPersistence>();
            endpointConfiguration.UseSerialization<JsonSerializer>();
            endpointConfiguration.EnableInstallers();
            endpointConfiguration.SendFailedMessagesTo("error");

            _endpointInstance = await Endpoint.Start(endpointConfiguration);
        }


    }

    
}
