using BluePrintSys.Messaging.CrossCutting.Models;
using NServiceBus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BluePrintSys.Messaging.CrossCutting.Host
{
    public class StatusMessageHandler : IHandleMessages<StatusCheckMessage>
    {
        public async Task Handle(StatusCheckMessage message, IMessageHandlerContext context)
        {
            return;
        }
    }
}
