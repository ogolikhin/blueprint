﻿using System;
using ActionHandlerService.Helpers;
using ActionHandlerService.Models;
using ActionHandlerService.Models.Enums;
using BluePrintSys.Messaging.CrossCutting.Logging;
using Topshelf;
using Log4NetStandardLogListener = ActionHandlerService.Logging.Log4NetStandardLogListener;

namespace ActionHandlerService
{
    public class ActionHandlerService : ServiceControl
    {
        private IMessageTransportHost _messageTransportHost;

        private static ActionHandlerService _instance;
        public static ActionHandlerService Instance => _instance ?? (_instance = new ActionHandlerService());

        private ActionHandlerService() { }

        public bool Start(HostControl hostControl)
        {
            LogManager.Manager.AddListener(Log4NetStandardLogListener.Instance);
            Log.Info("Action Handler Service is starting.");
            if (ConfigHelper.Transport == MessageTransport.RabbitMQ)
            {
                _messageTransportHost = new RabbitMQTransportHost();
                _messageTransportHost.Start(() => Stop(null));
                Log.Info("Action Handler Service started.");
            }
            else
            {
                Log.Error("Only RabbitMQ transport layer is supported");
            }
            
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
