using System;
using System.Threading.Tasks;
using ServiceLibrary.Models.Workflow;

namespace BluePrintSys.Messaging.CrossCutting.Host
{
    public interface INServiceBusServer
    {
        Task Send(string tenantId, IWorkflowMessage message);

        Task GetStatus(Messaging.Models.Actions.StatusCheckMessage message);
        Task<string> Start(string connectionString, bool sendOnly, Action criticalErrorCallback = null);
        Task Stop();
    }
}