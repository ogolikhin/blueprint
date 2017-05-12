using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NServiceBus;
using NServiceBus.Persistence.Sql;

namespace ImageRenderService.Helpers
{
    public static class ConfigureTransportExtensions
    {
        public static readonly string PersistenceConnectionString = @"Data Source=BlueprintDevDB;Database=Shared;Integrated Security=True";
        private static readonly string SqlQueueSchema = "queue";

        private static bool UseRabbitMQ = true; //false means - use SqlTransport

        public static void ConfigureTransport(this EndpointConfiguration endpointConfiguration, string virtualHost = null)
        {
            endpointConfiguration.AssemblyScanner().ExcludeAssemblies("NServiceBus.Callbacks.dll");
            if (UseRabbitMQ)
            {
                endpointConfiguration.ConfigureRabbitMqTransport(virtualHost);
            }
            else
            {
                //endpointConfiguration.ConfigureSqlServerTransport();
            }
        }

        public static void ConfigurePersistence(this EndpointConfiguration endpointConfiguration, string endPointName)
        {


            var persistence = endpointConfiguration.UsePersistence<SqlPersistence>();

            persistence.TablePrefix($"{SqlQueueSchema}.{endPointName.Replace('.', '_')}_");

            var connection = PersistenceConnectionString;
            persistence.SqlVariant(SqlVariant.MsSqlServer);
            persistence.ConnectionBuilder(
                connectionBuilder: () =>
                {
                    return new SqlConnection(connection);
                });

            if (UseRabbitMQ)
            {
                endpointConfiguration.EnableOutbox();
            }
            if (endPointName != null)
            {
                endpointConfiguration.UsePersistence<InMemoryPersistence>();
            }
        }

        /*internal static void ConfigureSqlServerTransport(this EndpointConfiguration endpointConfiguration)
        {
            var transport = endpointConfiguration.UseTransport<SqlServerTransport>();

            transport.DefaultSchema(SqlQueueSchema);
            transport.UseSchemaForQueue("error", SqlQueueSchema);
            transport.UseSchemaForQueue("audit", SqlQueueSchema);
            transport.ConnectionString(() => PersistenceConnectionString);

            // Only for Publish/Subscribe - https://docs.particular.net/nservicebus/messaging/routing
            var routingSettings = transport.Routing();
            routingSettings.RegisterPublisher(typeof(ArtifactPublished), EndPoints.Web);
        }*/

        internal static void ConfigureRabbitMqTransport(this EndpointConfiguration endpointConfiguration, string virtualHost = null)
        {
            var transport = endpointConfiguration.UseTransport<RabbitMQTransport>();

            //var host = "192.168.60.40";
            //var host = "192.168.0.112";
            var host = "titan.blueprintsys.net";

            var virtualHostSetting = string.IsNullOrEmpty(virtualHost)
                ? string.Empty
                : $";VirtualHost={virtualHost}";

            var connectionString = $"host={host}{virtualHostSetting};username=admin;password=$admin2011";

            System.Console.WriteLine($"ConnectionString: {connectionString}");

            transport.ConnectionString(connectionString);

            transport.Transactions(TransportTransactionMode.ReceiveOnly);
        }
    }
}
