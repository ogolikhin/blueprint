﻿using ServiceLibrary.Models.Enums;

namespace AdminStore.Models.DiagramWorkflow
{
    public class DWebhookAction : DBaseAction
    {
        public override ActionTypes ActionType => ActionTypes.Webhook;

        public string Url { get; set; }
    }
}