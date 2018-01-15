﻿using System;
using System.Threading.Tasks;
using ServiceLibrary.Models.Workflow;
using BluePrintSys.Messaging.Models.Actions;

namespace BluePrintSys.Messaging.CrossCutting.Host
{
    public interface IMessageTransportHost
    {
        Task Start(bool sendOnly, Func<bool> errorCallback = null);

        void Stop();

        Task SendAsync(string tenantId, IWorkflowMessage message);

        Task CheckStatusAsync();
    }
}
