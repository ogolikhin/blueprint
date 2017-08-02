using System.Collections.Generic;
using AdminStore.Models.Workflow;
using ServiceLibrary.Models.Workflow;

namespace AdminStore.Services.Workflow
{
    public interface ITriggerConverter
    {
        XmlWorkflowEventTriggers ToXmlModel(IEnumerable<IeTrigger> ieTriggers, WorkflowDataMaps dataMaps, int currentUserId);

        XmlWorkflowEventTrigger ToXmlModel(IeTrigger ieTrigger, WorkflowDataMaps dataMaps, int currentUserId);
    }

    public class WorkflowDataMaps
    {
        private IDictionary<string, int> _artifactTypeMap;
        public IDictionary<string, int> ArtifactTypeMap => _artifactTypeMap ?? (_artifactTypeMap = new Dictionary<string, int>());

        private IDictionary<string, int> _propertyTypeMap;
        public IDictionary<string, int> PropertyTypeMap => _propertyTypeMap ?? (_propertyTypeMap = new Dictionary<string, int>());

        private IDictionary<string, int> _stateMap;
        public IDictionary<string, int> StateMap => _stateMap ?? (_stateMap = new Dictionary<string, int>());

        private IDictionary<string, int> _userMap;
        public IDictionary<string, int> UserMap => _userMap ?? (_userMap = new Dictionary<string, int>());

        private IDictionary<string, int> _groupMap;
        public IDictionary<string, int> GroupMap => _groupMap ?? (_groupMap = new Dictionary<string, int>());

        // Key - Choice Property Type Id, Value - (Key - Valid Value, Valid Value Id)
        private IDictionary<int, IDictionary<string, int>> _validValueMap;
        public IDictionary<int, IDictionary<string, int>> ValidValueMap => _validValueMap ?? (_validValueMap = new Dictionary<int, IDictionary<string, int>>());
    }
}