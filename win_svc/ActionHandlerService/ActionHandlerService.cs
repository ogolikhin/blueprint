using System;
using System.Threading.Tasks;
using BluePrintSys.Messaging.CrossCutting.Logging;
using Topshelf;
using Log4NetStandardLogListener = ActionHandlerService.Logging.Log4NetStandardLogListener;

namespace ActionHandlerService
{
    public class ActionHandlerService : ServiceControl
    {
        private static readonly string NServiceBusConnectionString = ConfigHelper.NServiceBusConnectionString;
        private readonly NServiceBusServer _nServiceBusServer = new NServiceBusServer();

        private static ActionHandlerService _instance;
        public static ActionHandlerService Instance => _instance ?? (_instance = new ActionHandlerService());

        private ActionHandlerService() { }

        public bool Start(HostControl hostControl)
        {
            LogManager.Manager.AddListener(Log4NetStandardLogListener.Instance);
            Log.Info("Action Handler Service is starting.");
            Task.Run(() => _nServiceBusServer.Start(NServiceBusConnectionString))
                .ContinueWith(startTask =>
                {
                    if (!string.IsNullOrEmpty(startTask.Result))
                    {
                        Log.Error(startTask.Result);
                        Stop(null);
                    }
                });
            Log.Info("Action Handler Service started.");
            return true;
        }

        public bool Stop(HostControl hostControl)
        {
            try
            {
                Log.Info("Action Handler Service is stopping.");
                _nServiceBusServer.Stop().Wait();
            }
            catch (Exception e)
            {
                Log.Error(e);
            }
            finally
            {
                // Remove Log Listener
                Log4NetStandardLogListener.Clear();
                LogManager.Manager.ClearListeners();
            }
            return true;
        }
    }
}
