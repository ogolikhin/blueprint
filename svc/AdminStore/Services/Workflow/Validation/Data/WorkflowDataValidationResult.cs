using System.Collections.Generic;
using System.Linq;
using ServiceLibrary.Models;
using ServiceLibrary.Models.ProjectMeta;

namespace AdminStore.Services.Workflow.Validation.Data
{
    public class WorkflowDataValidationResult
    {
        public bool HasErrors => Errors.Any();

        private List<WorkflowDataValidationError> _errors;
        public List<WorkflowDataValidationError> Errors => _errors ?? (_errors = new List<WorkflowDataValidationError>());

        public HashSet<int> ValidProjectIds { get; } = new HashSet<int>();
        public HashSet<int> AssociatedArtifactTypeIds { get; } = new HashSet<int>();

        public ProjectTypes StandardTypes { get; set; }
        public Dictionary<string, ItemType> StandardArtifactTypeMapByName { get; } = new Dictionary<string, ItemType>();
        public Dictionary<string, PropertyType> StandardPropertyTypeMapByName { get; } = new Dictionary<string, PropertyType>();
        public Dictionary<int, ItemType> StandardArtifactTypeMapById { get; } = new Dictionary<int, ItemType>();
        public Dictionary<int, PropertyType> StandardPropertyTypeMapById { get; } = new Dictionary<int, PropertyType>();
        public List<SqlUser> Users { get; } = new List<SqlUser>();
        public List<SqlGroup> Groups { get; } = new List<SqlGroup>();
    }
}