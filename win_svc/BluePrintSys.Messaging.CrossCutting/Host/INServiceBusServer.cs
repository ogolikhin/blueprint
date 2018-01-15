using System.Threading.Tasks;
using ServiceLibrary.Models.Workflow;
using BluePrintSys.Messaging.Models.Actions;

namespace BluePrintSys.Messaging.CrossCutting.Host
{
    public interface INServiceBusServer
    {
        Task Send(string tenantId, IWorkflowMessage message);

        Task CheckStatus(StatusCheckMessage message);
        Task<string> Start(string connectionString, bool sendOnly);
        Task Stop();
    }
}