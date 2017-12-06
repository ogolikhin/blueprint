﻿using System.Collections.Generic;
using ServiceLibrary.Models.Enums;

namespace BluePrintSys.Messaging.Models.Actions
{
    public class ArtifactsPublishedMessage : ActionMessage
    {
        public override MessageActionType ActionType { get; } = MessageActionType.ArtifactsPublished;

        public string UserName { get; set; }

        public IEnumerable<PublishedArtifactInformation> Artifacts { get; set; }
    }
}
