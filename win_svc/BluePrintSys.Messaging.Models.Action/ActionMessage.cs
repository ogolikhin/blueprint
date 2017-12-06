﻿using NServiceBus;
using ServiceLibrary.Models.Enums;
using ServiceLibrary.Models.Workflow;

namespace BluePrintSys.Messaging.Models.Actions
{
    public abstract class ActionMessage : IMessage, IWorkflowMessage
    {
        public int UserId { get; set; }
        public abstract MessageActionType ActionType { get; }
        public int RevisionId { get; set; }
    }

    public abstract class ProjectContainerActionMessage : ActionMessage
    {
        public int ProjectId { get; set; }
    }
}
