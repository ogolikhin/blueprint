using System.Threading.Tasks;
using NServiceBus;

namespace BluePrintSys.Messaging.CrossCutting.Host
{
    public interface INServiceBusServer
    {
        Task Send(string tenantId, IMessage message);
        Task<string> Start(string connectionString, bool sendOnly);
        Task Stop();
    }
}