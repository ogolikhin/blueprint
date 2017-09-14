using System;
using System.Collections.Generic;
using System.Linq;
using AdminStore.Models.Workflow;
using ServiceLibrary.Helpers;

namespace AdminStore.Services.Workflow
{
    public class WorkflowDiff : IWorkflowDiff
    {
        #region Interface Implementation

        // Id in IeProjects and GroupProjectId for groups in IeUserGroup should be filled in.
        public WorkflowDiffResult DiffWorkflows(IeWorkflow workflow, IeWorkflow currentWorkflow)
        {
            var result = new WorkflowDiffResult();
            if (workflow == null) throw new ArgumentNullException(nameof(workflow));
            if (currentWorkflow == null) throw new ArgumentNullException(nameof(currentWorkflow));

            result.IsWorkflowPropertiesChanged = IsWorkflowPropertiesChanged(workflow, currentWorkflow);

            DiffWorkflowEntities(workflow.States, currentWorkflow.States, result.AddedStates,
                result.DeletedStates, result.ChangedStates, result.NotFoundStates, result.UnchangedStates);

            var events = workflow.TransitionEvents?.Select(e => e as IeEvent).ToList();
            var currentEvents = currentWorkflow.TransitionEvents?.Select(e => e as IeEvent).ToList();
            DiffWorkflowEntities(events, currentEvents, result.AddedEvents,
                 result.DeletedEvents, result.ChangedEvents, result.NotFoundEvents, result.UnchangedEvents);

            events = workflow.PropertyChangeEvents?.Select(te => te as IeEvent).ToList();
            currentEvents = currentWorkflow.PropertyChangeEvents?.Select(te => te as IeEvent).ToList();
            DiffWorkflowEntities(events, currentEvents, result.AddedEvents,
                 result.DeletedEvents, result.ChangedEvents, result.NotFoundEvents, result.UnchangedEvents);

            events = workflow.NewArtifactEvents?.Select(te => te as IeEvent).ToList();
            currentEvents = currentWorkflow.NewArtifactEvents?.Select(te => te as IeEvent).ToList();
            DiffWorkflowEntities(events, currentEvents, result.AddedEvents,
                 result.DeletedEvents, result.ChangedEvents, result.NotFoundEvents, result.UnchangedEvents);

            DiffProjectArtifactTypes(workflow.Projects, currentWorkflow.Projects, result);
            return result;
        }

        #endregion

        #region Private methods

        private static bool IsWorkflowPropertiesChanged(IeWorkflow workflow, IeWorkflow currentWorkflow)
        {
            // Ignore IsActive
            return workflow.Id.GetValueOrDefault() != currentWorkflow.Id.GetValueOrDefault()
                   || !string.Equals(workflow.Name, currentWorkflow.Name)
                   || !string.Equals(workflow.Description, currentWorkflow.Description);
        }

        private static void DiffWorkflowEntities <T>(ICollection<T> entities, ICollection<T> currentEntities,
            ICollection<T> added, ICollection<T> deleted, ICollection<T> changed, ICollection<T> notFound, ICollection<T> unchanged)
            where T : IIeWorkflowEntityWithId
        {
            var stateIds = (entities?.Where(s => s.Id.HasValue).Select(s => s.Id.Value).ToHashSet()) ?? new HashSet<int>();
            var currentStateIds = (currentEntities?.Where(s => s.Id.HasValue).Select(s => s.Id.Value).ToHashSet()) ?? new HashSet<int>();

            entities?.ForEach(s =>
            {
                ICollection<T> colToAddTo;
                if (!s.Id.HasValue)
                {
                    colToAddTo = added;
                }
                else if (!currentStateIds.Contains(s.Id.Value))
                {
                    colToAddTo = notFound;
                }
                else
                {
                    var currentState = currentEntities.First(cs => cs.Id == s.Id);
                    colToAddTo = s.Equals(currentState) ? unchanged : changed;
                }

                colToAddTo.Add(s);
            });

            currentEntities?.Where(s => s.Id.HasValue && !stateIds.Contains(s.Id.Value)).ForEach(deleted.Add);
        }

        private static void DiffProjectArtifactTypes(List<IeProject> projects, List<IeProject> currentProjects, WorkflowDiffResult result)
        {
            var pAtIds = new HashSet<Tuple<int, int>>();
            projects?.Where(p => p.Id.HasValue).ForEach(p =>
            {
                p.ArtifactTypes?.Where(at => at.Id.HasValue)
                    .ForEach(at => pAtIds.Add(Tuple.Create(p.Id.Value, at.Id.Value)));
            });
            var cpAtIds = new HashSet<Tuple<int, int>>();
            currentProjects?.Where(p => p.Id.HasValue).ForEach(p =>
            {
                p.ArtifactTypes?.Where(at => at.Id.HasValue)
                    .ForEach(at => cpAtIds.Add(Tuple.Create(p.Id.Value, at.Id.Value)));
            });

            var notSpecifiedAtIds = new HashSet<int>();
            projects?.Where(p => p.Id.HasValue).ForEach(p => p.ArtifactTypes?.ForEach(at =>
            {
                // I this case the workflow data validator logs an error.
                if (!at.Id.HasValue)
                {
                    return;
                }

                // A negative artifact type Id means Id is not specified in xml.
                var isSpecifiedInXml = at.Id.GetValueOrDefault() > 0;
                // Make Id positive
                if (!isSpecifiedInXml)
                {
                    at.Id *= -1;
                    notSpecifiedAtIds.Add(at.Id.Value);
                }

                var colToAddTo = cpAtIds.Contains(Tuple.Create(p.Id.Value, at.Id.Value))
                    ? result.UnchangedProjectArtifactTypes
                    : (isSpecifiedInXml
                        ? result.NotFoundProjectArtifactTypes
                        : result.AddedProjectArtifactTypes);

                colToAddTo.Add(new KeyValuePair<int, IeArtifactType>(p.Id.Value, at));
            }));

            currentProjects?.Where(p => p.Id.HasValue).ForEach(p => p.ArtifactTypes?
                .Where(at => at.Id.HasValue
                    && !notSpecifiedAtIds.Contains(at.Id.Value)
                    && !pAtIds.Contains(Tuple.Create(p.Id.Value, at.Id.Value)))
                .ForEach(at => result.DeletedProjectArtifactTypes.Add(new KeyValuePair<int, IeArtifactType>(p.Id.Value, at))));
        }

        #endregion

    }
}