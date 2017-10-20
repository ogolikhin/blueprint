using System;
using System.Collections.Generic;
using AdminStore.Models.Workflow;
using ServiceLibrary.Models.Workflow;

namespace AdminStore.Services.Workflow
{
    public interface ITriggerConverter
    {
        XmlWorkflowEventTriggers ToXmlModel(IEnumerable<IeTrigger> ieTriggers, WorkflowDataMaps dataMaps);

        IEnumerable<IeTrigger> FromXmlModel(XmlWorkflowEventTriggers xmlTriggers, WorkflowDataNameMaps dataMaps,
            ISet<int> userIdsToCollect, ISet<int> groupIdsToCollect);
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

        // Key - Tuple where Item1 - Group Name, Item2 - Project Id, Value - Group Id
        private IDictionary<Tuple<string, int?>, int> _groupMap;
        public IDictionary<Tuple<string, int?>, int> GroupMap => _groupMap ?? (_groupMap = new Dictionary<Tuple<string, int?>, int>());

        // Key - Choice Property Type Id, Value - (Key - Valid Value, Valid Value Id)
        private IDictionary<int, IDictionary<string, int>> _validValueMap;
        public IDictionary<int, IDictionary<string, int>> ValidValueMap => _validValueMap ?? (_validValueMap = new Dictionary<int, IDictionary<string, int>>());
    }

    public class WorkflowDataNameMaps
    {
        private IDictionary<int, string> _artifactTypeMap;
        public IDictionary<int, string> ArtifactTypeMap => _artifactTypeMap ?? (_artifactTypeMap = new Dictionary<int, string>());

        private IDictionary<int, string> _propertyTypeMap;
        public IDictionary<int, string> PropertyTypeMap => _propertyTypeMap ?? (_propertyTypeMap = new Dictionary<int, string>());

        private IDictionary<int, string> _stateMap;
        public IDictionary<int, string> StateMap => _stateMap ?? (_stateMap = new Dictionary<int, string>());

        // private IDictionary<int, IDictionary<int, string>> _validValueMap;
        // public IDictionary<int, IDictionary<int, string>> ValidValueMap => _validValueMap ?? (_validValueMap = new Dictionary<int, IDictionary<int, string>>());
        private IDictionary<int, string> _validValueMap;
        public IDictionary<int, string> ValidValueMap => _validValueMap ?? (_validValueMap = new Dictionary<int, string>());

    }
}