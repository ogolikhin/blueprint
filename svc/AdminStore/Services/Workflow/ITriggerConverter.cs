﻿using System.Collections.Generic;
using AdminStore.Models.Workflow;
using ServiceLibrary.Models.Workflow;

namespace AdminStore.Services.Workflow
{
    public interface ITriggerConverter
    {
        XmlWorkflowEventTriggers ToXmlModel(IEnumerable<IeTrigger> ieTriggers, IDictionary<string, int> artifactTypeMap,
            IDictionary<string, int> propertyTypeMap, IDictionary<string, int> stateMap,
            IDictionary<string, int> userMap, IDictionary<string, int> groupMap, int currentUserId);

        XmlWorkflowEventTrigger ToXmlModel(IeTrigger ieTrigger, IDictionary<string, int> artifactTypeMap,
            IDictionary<string, int> propertyTypeMap, IDictionary<string, int> stateMap,
            IDictionary<string, int> userMap, IDictionary<string, int> groupMap, int currentUserId);       

        IEnumerable<IeTrigger> FromXmlModel(XmlWorkflowEventTriggers xmlTriggers,
            IDictionary<string, int> artifactTypeMap,
            IDictionary<string, int> propertyTypeMap, 
            IDictionary<string, int> groupMap,
            IDictionary<string, int> stateMap);

        IeTrigger FromXmlModel(XmlWorkflowEventTrigger xmlTrigger,
           IDictionary<string, int> artifactTypeMap,
           IDictionary<string, int> propertyTypeMap, 
           IDictionary<string, int> groupMap,
           IDictionary<string, int> stateMap);
    }
}