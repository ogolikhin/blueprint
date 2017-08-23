﻿using ServiceLibrary.Models.Enums;

namespace BluePrintSys.Messaging.Models.Actions
{
    public class GenerateDescendantsMessage : ActionMessage
    {
        public override MessageActionType ActionType { get; } = MessageActionType.GenerateChildren;

        public int ChildCount { get; set; } = 10;

        public int? ArtifactTypeId { get; set; }

        public int RevisionId { get; set; }

        public int ArtifactId { get; set; }
    }
}
