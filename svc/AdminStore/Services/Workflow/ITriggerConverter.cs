using System.Collections.Generic;
using AdminStore.Models.Workflow;
using ServiceLibrary.Models.Workflow;

namespace AdminStore.Services.Workflow
{
    public interface ITriggerConverter
    {
        XmlWorkflowEventTriggers ToXmlModel(IEnumerable<IeTrigger> ieTriggers, IDictionary<string, int> artifactTypeMap,
            IDictionary<string, int> propertyTypeMap, IDictionary<string, int> stateMap);

        XmlWorkflowEventTrigger ToXmlModel(IeTrigger ieTrigger, IDictionary<string, int> artifactTypeMap,
            IDictionary<string, int> propertyTypeMap, IDictionary<string, int> stateMap);

    }
}