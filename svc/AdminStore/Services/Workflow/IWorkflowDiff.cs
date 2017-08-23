using System.Collections.Generic;
using System.Linq;
using AdminStore.Models.Workflow;

namespace AdminStore.Services.Workflow
{
    public interface IWorkflowDiff
    {
        // Id in IeProjects and GroupProjectId for groups in IeUserGroup should be filled in.
        WorkflowDiffResult DiffWorkflows(IeWorkflow workflow, IeWorkflow currentWorkflow);
    }

    public class WorkflowDiffResult
    {

        public bool HasChanges => IsWorkflowPropertiesChanged
                                   || AddedStates.Any()
                                   || DeletedStates.Any()
                                   || ChangedStates.Any()
                                   || NotFoundStates.Any()
                                   || AddedEvents.Any()
                                   || DeletedEvents.Any()
                                   || ChangedEvents.Any()
                                   || NotFoundEvents.Any()
                                   || AddedProjectArtifactTypes.Any()
                                   || DeletedProjectArtifactTypes.Any()
                                   || NotFoundProjectArtifactTypes.Any();

        public bool IsWorkflowPropertiesChanged { get; set; }

        public List<IeState> AddedStates { get; } = new List<IeState>();
        // From the current workflow
        public List<IeState> DeletedStates { get; } = new List<IeState>();
        public List<IeState> ChangedStates { get; } = new List<IeState>();
        public List<IeState> NotFoundStates { get; } = new List<IeState>();
        public List<IeState> UnchangedStates { get; } = new List<IeState>();

        public List<IeEvent> AddedEvents { get; } = new List<IeEvent>();
        // From the current workflow
        public List<IeEvent> DeletedEvents { get; } = new List<IeEvent>();
        public List<IeEvent> ChangedEvents { get; } = new List<IeEvent>();
        public List<IeEvent> NotFoundEvents { get; } = new List<IeEvent>();
        public List<IeEvent> UnchangedEvents { get; } = new List<IeEvent>();

        public List<IeArtifactType> AddedProjectArtifactTypes { get; } = new List<IeArtifactType>();
        // From the current workflow
        public List<IeArtifactType> DeletedProjectArtifactTypes { get; } = new List<IeArtifactType>();
        public List<IeArtifactType> NotFoundProjectArtifactTypes { get; } = new List<IeArtifactType>();
        public List<IeArtifactType> UnchangedProjectArtifactTypes { get; } = new List<IeArtifactType>();
    }
}