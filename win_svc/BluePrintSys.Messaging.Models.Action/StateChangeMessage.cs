﻿using ServiceLibrary.Models.Enums;

namespace BluePrintSys.Messaging.Models.Actions
{
    public class StateChangeMessage : ActionMessage
    {
        public override MessageActionType ActionType { get; } = MessageActionType.StateChange;

        public int ArtifactId { get; set; }
        public string ArtifactType { get; set; }
        public int CurrentStateId { get; set; }
        public int PreviousStateId { get; set; }
    }
}
