﻿using System;
using System.Threading.Tasks;
using BluePrintSys.Messaging.CrossCutting.Configuration;
using BluePrintSys.Messaging.CrossCutting.Host;
using BluePrintSys.Messaging.CrossCutting.Logging;
using Topshelf;
using Log4NetStandardLogListener = BlueprintSys.RC.Services.Logging.Log4NetStandardLogListener;
using ServiceLibrary.Helpers;

namespace BlueprintSys.RC.Services
{
    public class ActionHandlerService : ServiceControl
    {
        private IMessageTransportHost _messageTransportHost;

        private static ActionHandlerService _instance;
        public static ActionHandlerService Instance => _instance ?? (_instance = new ActionHandlerService());

        private ActionHandlerService()
        {
            // Enable TLS 1.1/1.2
            HttpsSecurity.Configure();
        }

        public bool Start(HostControl hostControl)
        {
            LogManager.Manager.AddListener(Log4NetStandardLogListener.Instance);

            Log.Info("Action Handler Service is starting.");
            _messageTransportHost = new TransportHost(new ConfigHelper(), WorkflowServiceBusServer.Instance);
            Task.Run(() => _messageTransportHost.Start(false, () => Stop(null)));
            Log.Info("Action Handler Service started.");

            return true;
        }

        public bool Stop(HostControl hostControl)
        {
            try
            {
                Log.Info("Action Handler Service is stopping.");
                _messageTransportHost.Stop();
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
