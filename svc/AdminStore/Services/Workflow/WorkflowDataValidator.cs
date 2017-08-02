﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AdminStore.Models.Workflow;
using AdminStore.Repositories;
using AdminStore.Repositories.Workflow;
using ArtifactStore.Helpers;
using ServiceLibrary.Models.Workflow;
using ServiceLibrary.Repositories.ProjectMeta;

namespace AdminStore.Services.Workflow
{
    public class WorkflowDataValidator : IWorkflowDataValidator
    {
        private readonly IWorkflowRepository _workflowRepository;
        private readonly IUserRepository _userRepository;
        private readonly ISqlProjectMetaRepository _projectMetaRepository;

        public WorkflowDataValidator(IWorkflowRepository workflowRepository, IUserRepository userRepository,
            ISqlProjectMetaRepository projectMetaRepository)
        {
            _workflowRepository = workflowRepository;
            _userRepository = userRepository;
            _projectMetaRepository = projectMetaRepository;
        }

        public async Task<WorkflowDataValidationResult> ValidateData(IeWorkflow workflow)
        {
            if (workflow == null)
            {
                throw new ArgumentNullException(nameof(workflow));
            }

            var result = new WorkflowDataValidationResult();
            await ValidateWorkflowNameForUniqueness(result, workflow);

            result.StandardTypes = await _projectMetaRepository.GetStandardProjectTypesAsync();
            ISet<string> groupsToLookup;
            ISet<string> usersToLookup;
            CollectUsersAndGroupsToLookup(workflow, out usersToLookup, out groupsToLookup);
            result.Users.AddRange(await _userRepository.GetExistingUsersByNames(usersToLookup));
            result.Groups.AddRange(await _userRepository.GetExistingGroupsByNames(groupsToLookup, false));

            await ValidateProjectsData(result, workflow);
            await ValidateArtifactTypesData(result, workflow);
            await ValidateGroupsData(result, workflow);
            await ValidateTriggersData(result, workflow);
            await ValidateActionsData(result, workflow);

            return result;
        }

        private void CollectUsersAndGroupsToLookup(IeWorkflow workflow, out ISet<string> usersToLookup,
            out ISet<string> groupsToLookup)
        {
            var users = new HashSet<string>();
            var groups = new HashSet<string>();

            workflow.TransitionEvents?.ForEach(t =>
            {
                t?.PermissionGroups?.ForEach(g => groups.Add(g.Name));
            });

            workflow.TransitionEvents?.ForEach(te => CollectUsersAndGroupsToLookup(te, users, groups));
            workflow.PropertyChangeEvents?.ForEach(pce => CollectUsersAndGroupsToLookup(pce, users, groups));
            workflow.NewArtifactEvents?.ForEach(nae => CollectUsersAndGroupsToLookup(nae, users, groups));

            usersToLookup = new HashSet<string>(users);
            groupsToLookup = new HashSet<string>(groups);
        }

        private void CollectUsersAndGroupsToLookup(IeEvent wEvent, ISet<string> users, ISet<string> groups)
        {
            wEvent?.Triggers?.ForEach(t =>
            {
                if (t?.Action?.ActionType == ActionTypes.PropertyChange)
                {
                    var action = (IePropertyChangeAction)t.Action;
                    action.UsersGroups?.ForEach(ug =>
                    {
                        if (ug.IsGroup.GetValueOrDefault())
                        {
                            groups.Add(ug.Name);
                        }
                        else
                        {
                            users.Add(ug.Name);
                        }
                    });
                }
            });
        }

        private async Task ValidateWorkflowNameForUniqueness(WorkflowDataValidationResult result, IeWorkflow workflow)
        {
            var duplicateNames = await _workflowRepository.CheckLiveWorkflowsForNameUniqueness(new[] { workflow.Name });
            if (duplicateNames.Any())
            {
                result.Errors.Add(new WorkflowDataValidationError
                {
                    Element = workflow,
                    ErrorCode = WorkflowDataValidationErrorCodes.WorkflowNameNotUnique
                });
            }
        }

        private async Task ValidateProjectsData(WorkflowDataValidationResult result, IeWorkflow workflow)
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
                foreach (var project in workflow.Projects
                    .Where(proj => projectPaths.All(
                        path => proj.Id.HasValue
                            ? path.Key != proj.Id.Value
                            : !path.Value.Equals(proj.Path)))
                        .Select(proj => proj.Id?.ToString() ?? proj.Path)){
                    result.Errors.Add(new WorkflowDataValidationError
                    {
                        Element = project,
                        ErrorCode = WorkflowDataValidationErrorCodes.ProjectNotFound
                    });
                }
            }
            
            result.ValidProjectIds.AddRange(projectPaths.Select(p => p.Key).ToHashSet());
        }

        private async Task ValidateArtifactTypesData(WorkflowDataValidationResult result, IeWorkflow workflow)
        {
            if (workflow.ArtifactTypes.Any() && result.ValidProjectIds.Any())
            {
                result.ValidArtifactTypeNames.Clear();
                var artifactTypesInfos = (await _workflowRepository.GetExistingStandardArtifactTypesForWorkflows(
                    workflow.ArtifactTypes.Select(at => at.Name),
                    result.ValidProjectIds)).ToArray();
                //check if all types are valid
                if (artifactTypesInfos.Length != workflow.ArtifactTypes.Count*result.ValidProjectIds.Count)
                {
                    //get all artifact types and project pairs that are missing
                    var crossJoin = from at in workflow.ArtifactTypes
                        from pid in result.ValidProjectIds
                        select new {artifactTypeName = at.Name, projectId = pid};
                    foreach (var missingArtifactTypeInfo in crossJoin.Where(el => artifactTypesInfos.Any(ati =>
                        ati.Name == el.artifactTypeName &&
                        ati.VersionProjectId == el.projectId)))
                    {
                        result.Errors.Add(new WorkflowDataValidationError
                        {
                            Element = new Tuple<string, int>(missingArtifactTypeInfo.artifactTypeName, missingArtifactTypeInfo.projectId),
                            ErrorCode = WorkflowDataValidationErrorCodes.ArtifactTypeNotFoundInProject
                        });
                    }
                }

                foreach (var artifactTypesInfo in artifactTypesInfos){
                    //check if any types are associated with a workflow already
                    if (artifactTypesInfo.WorkflowId.HasValue)
                    {
                        result.Errors.Add(new WorkflowDataValidationError
                        {
                            Element = workflow.ArtifactTypes.FirstOrDefault(at => at.Name == artifactTypesInfo.Name),
                            ErrorCode = WorkflowDataValidationErrorCodes.ArtifactTypeAlreadyAssociatedWithWorkflow
                        });
                    }
                }
            }

            result.ValidArtifactTypeNames.AddRange(workflow.ArtifactTypes.Select(at => at.Name));
        }

        private async Task ValidateGroupsData(WorkflowDataValidationResult result, IeWorkflow workflow)
        {
            result.ValidGroups.Clear();
            HashSet<string> listOfAllGroups = new HashSet<string>();
            workflow.TransitionEvents.OfType<IeTransitionEvent>().ForEach(transition =>
            {
                transition.PermissionGroups.ForEach(group =>listOfAllGroups.Add(group.Name));
            });
            var existingInstanceGroupNames = (await _userRepository.GetExistingGroupsByNames(listOfAllGroups, true)).ToArray();
            if (existingInstanceGroupNames.Length != listOfAllGroups.Count)
            {
                foreach (var group in listOfAllGroups.Where(
                    li => existingInstanceGroupNames.All(g => g.Name != li)
                )){
                    result.Errors.Add(new WorkflowDataValidationError
                    {
                        Element = group,
                        ErrorCode = WorkflowDataValidationErrorCodes.GroupsNotFound
                    });
                }
            }

            result.ValidGroups.AddRange(existingInstanceGroupNames.ToHashSet());
        }

        private async Task ValidateTriggersData(WorkflowDataValidationResult result, IeWorkflow workflow)
        {
            //validate property name in property change events
            var listOfPropertyNames = workflow.PropertyChangeEvents.Select(pce => pce.PropertyName).ToHashSet();
            if (!listOfPropertyNames.Any())
            {
                return;
            }
            var existingPropertyNames = (await _workflowRepository.GetExistingPropertyTypesByName(listOfPropertyNames)).ToArray();

            if (existingPropertyNames.Length != listOfPropertyNames.Count)
            {
                foreach (var propertyName in listOfPropertyNames.Where(pn => !existingPropertyNames.Contains(pn)))
                {
                    result.Errors.Add(new WorkflowDataValidationError
                    {
                        Element = propertyName,
                        ErrorCode = WorkflowDataValidationErrorCodes.PropertyNotFound
                    });
                }
            }
        }

        private async Task ValidateActionsData(WorkflowDataValidationResult result, IeWorkflow workflow)
        {
            //validate artifact type for generate actions of type Children

            //validate propertyName in email notification actions if one is provided

            //validate propertyName and propertyValue type in property change actions

            // TODO: Temporary place holder for async method, will be removed.
            await Task.Delay(1);
        }
    }
}