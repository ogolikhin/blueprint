using System.Threading.Tasks;
using ServiceLibrary.Models.Workflow;

namespace BluePrintSys.Messaging.CrossCutting.Host
{
    public interface INServiceBusServer
    {
        Task Send(string tenantId, IWorkflowMessage message);
        Task<string> Start(string connectionString, bool sendOnly);
        Task Stop();
    }
}