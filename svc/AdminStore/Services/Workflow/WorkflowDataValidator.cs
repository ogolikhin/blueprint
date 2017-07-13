using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AdminStore.Models.Workflow;
using AdminStore.Repositories;
using AdminStore.Repositories.Workflow;
using ArtifactStore.Helpers;

namespace AdminStore.Services.Workflow
{
    public class WorkflowDataValidator : IWorkflowDataValidator
    {
        private readonly IWorkflowRepository _workflowRepository;
        private readonly IUserRepository _userRepository;

        public WorkflowDataValidator(IWorkflowRepository workflowRepository, IUserRepository userRepository)
        {
            _workflowRepository = workflowRepository;
            _userRepository = userRepository;
        }

        public async Task<WorkflowDataValidationResult> ValidateData(IeWorkflow workflow)
        {
            if (workflow == null)
            {
                throw new ArgumentNullException(nameof(workflow));
            }

            var result = new WorkflowDataValidationResult();
            await ValidateProjectsData(result, workflow);
            await ValidateGroupsData(result, workflow);
            await ValidateTriggersData(result, workflow);
            await ValidateActionsData(result, workflow);

            return result;
        }

        private async Task<WorkflowDataValidationResult> ValidateProjectsData(WorkflowDataValidationResult result, IeWorkflow workflow)
        {
            result.ValidProjectIds.Clear();
            Dictionary<int, string> projectPaths = new Dictionary<int, string>();
            HashSet<string> projectPathsToLookup = new HashSet<string>();
            workflow.Projects.ForEach(project =>
            {
                if (project.Id.HasValue)
                {
                    projectPaths[project.Id.Value] = project.Path;
                }
                else
                {
                    if (!string.IsNullOrEmpty(project.Path))
                    {
                        projectPathsToLookup.Add(project.Path);
                    }
                }
            });

            if (projectPathsToLookup.Any())
            {
                //look up ID of projects that have no ID provided
                foreach (var sqlProjectPathPair in await _workflowRepository.GetProjectIdsByProjectPaths(projectPathsToLookup))
                {
                    projectPaths[sqlProjectPathPair.ProjectId] = sqlProjectPathPair.ProjectPath;
                }
            }

            if (projectPaths.Count != workflow.Projects.Count)
            {
                //generate a list of all projects in the workflow who are either missing from id list or were not looked up by path
                var listOfBadProjects = string.Join(",", workflow.Projects
                    .Where(proj => projectPaths.All(
                        path => proj.Id.HasValue
                            ? path.Key != proj.Id.Value
                            : !path.Value.Equals(proj.Path))
                    ).Select(proj => proj.Id?.ToString() ?? proj.Path));
                result.Errors.Add(new WorkflowDataValidationError
                {
                    Info = $"The following projects could not be found: {listOfBadProjects}",
                    ErrorCode = WorkflowDataValidationErrorCodes.ProjectNotFound
                });
            }
            
            result.ValidProjectIds.AddRange(projectPaths.Select(p => p.Key).ToHashSet());

            return result;
        }

        private async Task<WorkflowDataValidationResult> ValidateGroupsData(WorkflowDataValidationResult result, IeWorkflow workflow)
        {
            result.ValidGroups.Clear();
            HashSet<string> listOfAllGroups = new HashSet<string>();
            workflow.Triggers.OfType<IeTransitionTrigger>().ForEach(transition =>
            {
                transition.PermissionGroups.ForEach(group =>
                {
                    if (!listOfAllGroups.Contains(group.Name))
                    {
                        listOfAllGroups.Add(group.Name);
                    }
                });
            });
            var existingGroupNames = (await _userRepository.GetExistingInstanceGroupsByNames(listOfAllGroups)).ToArray();
            if (existingGroupNames.Length != listOfAllGroups.Count)
            {
                var listOfBadGroups = string.Join(",", listOfAllGroups.Where(
                        li => existingGroupNames.All(g => g.Name != li)
                    ));
                result.Errors.Add(new WorkflowDataValidationError
                {
                    Info = $"The following groups were not found: {listOfBadGroups}",
                    ErrorCode = WorkflowDataValidationErrorCodes.GroupsNotFound
                });
            }

            result.ValidGroups.AddRange(existingGroupNames.ToHashSet());

            return result;
        }

        private async Task<WorkflowDataValidationResult> ValidateTriggersData(WorkflowDataValidationResult result, IeWorkflow workflow)
        {
            //validate property name in property change triggers

            return await Task.FromResult(result);
        }

        private async Task<WorkflowDataValidationResult> ValidateActionsData(WorkflowDataValidationResult result, IeWorkflow workflow)
        {
            //validate artifact type for generate actions of type Children

            //validate propertyName in email notification actions if one is provided

            //validadate propertyName and propertyValue type in property change actions

            return await Task.FromResult(result);
        }
    }
}