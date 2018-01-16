using System.Threading.Tasks;
using ServiceLibrary.Models.Workflow;
using BluePrintSys.Messaging.CrossCutting.Models;

namespace BluePrintSys.Messaging.CrossCutting.Host
{
    public interface INServiceBusServer
    {
        Task Send(string tenantId, IWorkflowMessage message);

        Task GetStatus(StatusCheckMessage message);
        Task<string> Start(string connectionString, bool sendOnly);
        Task Stop();
    }
}