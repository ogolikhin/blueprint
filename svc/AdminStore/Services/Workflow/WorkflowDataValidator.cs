using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AdminStore.Models.Workflow;
using AdminStore.Repositories;
using AdminStore.Repositories.Workflow;
using ArtifactStore.Helpers;
using ServiceLibrary.Helpers;
using ServiceLibrary.Models.Enums;
using ServiceLibrary.Models.ProjectMeta;
using ServiceLibrary.Models.Workflow;
using ServiceLibrary.Repositories.ProjectMeta;

namespace AdminStore.Services.Workflow
{
    public class WorkflowDataValidator : IWorkflowDataValidator
    {
        private readonly IWorkflowRepository _workflowRepository;
        private readonly IUserRepository _userRepository;
        private readonly ISqlProjectMetaRepository _projectMetaRepository;
        private readonly IWorkflowActionPropertyValueValidator _propertyValueValidator;

        public WorkflowDataValidator(IWorkflowRepository workflowRepository, IUserRepository userRepository,
            ISqlProjectMetaRepository projectMetaRepository, IWorkflowActionPropertyValueValidator propertyValueValidator)
        {
            _workflowRepository = workflowRepository;
            _userRepository = userRepository;
            _projectMetaRepository = projectMetaRepository;
            _propertyValueValidator = propertyValueValidator;
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
            result.StandardTypes.ArtifactTypes?.RemoveAll(at => at.PredefinedType != null
                && !at.PredefinedType.Value.IsRegularArtifactType());
            result.StandardArtifactTypeMap.AddRange(result.StandardTypes.ArtifactTypes.ToDictionary(pt => pt.Name));
            result.StandardPropertyTypeMap.AddRange(result.StandardTypes.PropertyTypes.ToDictionary(pt => pt.Name));
            ISet<string> groupsToLookup;
            ISet<string> usersToLookup;
            CollectUsersAndGroupsToLookup(workflow, out usersToLookup, out groupsToLookup);
            result.Users.AddRange(await _userRepository.GetExistingUsersByNames(usersToLookup));
            result.Groups.AddRange(await _userRepository.GetExistingGroupsByNames(groupsToLookup, false));

            await ValidateProjectsData(result, workflow);
            await ValidateArtifactTypesData(result, workflow);
            ValidateEventsData(result, workflow);

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
            Dictionary<string, IeProject> projectPathsToLookup = new Dictionary<string, IeProject>();
            if (workflow.Projects.Any())
            {
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
                            projectPathsToLookup.Add(project.Path, project);
                        }
                    }
                });

                if (projectPathsToLookup.Any())
                {
                    //look up ID of projects that have no ID provided
                    foreach (
                        var sqlProjectPathPair in
                            await _workflowRepository.GetProjectIdsByProjectPaths(projectPathsToLookup.Keys))
                    {
                        projectPaths[sqlProjectPathPair.ProjectId] = sqlProjectPathPair.ProjectPath;
                        // Assign ProjectId to projects without it.
                        IeProject project;
                        if (projectPathsToLookup.TryGetValue(sqlProjectPathPair.ProjectPath, out project))
                        {
                            project.Id = sqlProjectPathPair.ProjectId;
                        }
                    }
                }

                if (projectPaths.Count != workflow.Projects.Count)
                {
                    //generate a list of all projects in the workflow that are either missing from id list or were not looked up by path
                    foreach (var project in workflow.Projects
                        .Where(proj => projectPaths.All(
                            path => proj.Id.HasValue
                                ? path.Key != proj.Id.Value
                                : !path.Value.Equals(proj.Path)))
                        .Select(proj => proj.Id?.ToString() ?? proj.Path))
                    {
                        result.Errors.Add(new WorkflowDataValidationError
                        {
                            Element = project,
                            ErrorCode = WorkflowDataValidationErrorCodes.ProjectByPathNotFound
                        });
                    }
                }

                var projectIds = projectPaths.Select(p => p.Key).ToHashSet();
                var validProjectIds = (await _workflowRepository.GetExistingProjectsByIds(projectIds)).ToArray();
                if (validProjectIds.Length != projectIds.Count)
                {
                    foreach (var invalidId in projectIds.Where(pid => !validProjectIds.Contains(pid)))
                    {
                        result.Errors.Add(new WorkflowDataValidationError
                        {
                            Element = invalidId,
                            ErrorCode = WorkflowDataValidationErrorCodes.ProjectByIdNotFound
                        });
                    }
                }

                if (workflow.Projects.GroupBy(p => p.Id).Any(g => g.Count() > 1))
                {
                    result.Errors.Add(new WorkflowDataValidationError
                    {
                        ErrorCode = WorkflowDataValidationErrorCodes.ProjectDuplicate
                    });
                }

                result.ValidProjectIds.AddRange(validProjectIds);
            }
        }

        private async Task ValidateArtifactTypesData(WorkflowDataValidationResult result, IeWorkflow workflow)
        {
            if (!workflow.Projects.IsEmpty() && result.ValidProjectIds.Any())
            {
                var artifactTypesInProjects = workflow.Projects.SelectMany(p => p.ArtifactTypes.Select(at => at.Name)).ToList();

                var standardArtifactTypes = result.StandardTypes.ArtifactTypes.Select(sat => sat.Name).ToHashSet();
                artifactTypesInProjects.ForEach(at =>
                {
                    if (!standardArtifactTypes.Contains(at))
                    {
                        result.Errors.Add(new WorkflowDataValidationError
                        {
                            Element = at,
                            ErrorCode = WorkflowDataValidationErrorCodes.StandardArtifactTypeNotFound
                        });
                    }
                });

                // TODO: Change the stored proc GetExistingStandardArtifactTypesForWorkflows
                // TODO: to accept Project Id and Artifact Type Name pairs
                var artifactTypeInWorkflowInfos = (await _workflowRepository.GetExistingStandardArtifactTypesForWorkflows(
                    artifactTypesInProjects, result.ValidProjectIds)).Where(i => i.WorkflowId.HasValue).
                    Select(i => Tuple.Create(i.VersionProjectId, i.Name)).ToHashSet();

                workflow.Projects?.ForEach(p => p?.ArtifactTypes.ForEach(at => 
                {
                    if(artifactTypeInWorkflowInfos.Contains(Tuple.Create(p.Id.GetValueOrDefault(), at.Name)))
                    {
                        result.Errors.Add(new WorkflowDataValidationError
                        {
                            Element = Tuple.Create(p.Id.GetValueOrDefault(), at.Name),
                            ErrorCode = WorkflowDataValidationErrorCodes.ArtifactTypeInProjectAlreadyAssociatedWithWorkflow
                        });
                    }
                }));
            }
        }

        private void ValidateEventsData(WorkflowDataValidationResult result, IeWorkflow workflow)
        {
            workflow.TransitionEvents?.ForEach(t => ValidateTransitionData(result, t));
            workflow.PropertyChangeEvents?.ForEach(pce => ValidatePropertyChangeEventData(result, pce));
            workflow.NewArtifactEvents?.ForEach(nae => ValidateNewArtifactEventData(result, nae));
        }

        private void ValidateTransitionData(WorkflowDataValidationResult result, IeTransitionEvent transition)
        {
            if (transition == null)
            {
                return;
            }

            ValidatePermissionGroupsData(result, transition.PermissionGroups);
            transition.Triggers?.ForEach(t => ValidateTriggerData(result, t));
        }

        private void ValidatePropertyChangeEventData(WorkflowDataValidationResult result, IePropertyChangeEvent pcEvent)
        {
            if (pcEvent == null)
            {
                return;
            }

            if (!result.StandardPropertyTypeMap.ContainsKey(pcEvent.PropertyName))
            {
                result.Errors.Add(new WorkflowDataValidationError
                {
                    Element = pcEvent.PropertyName,
                    ErrorCode = WorkflowDataValidationErrorCodes.PropertyNotFound
                });
            }

            pcEvent.Triggers?.ForEach(t => ValidateTriggerData(result, t));
        }

        private void ValidateNewArtifactEventData(WorkflowDataValidationResult result, IeNewArtifactEvent naEvent)
        {
            if (naEvent == null)
            {
                return;
            }

            naEvent.Triggers?.ForEach(t => ValidateTriggerData(result, t));
        }

        private static void ValidatePermissionGroupsData(WorkflowDataValidationResult result, List<IeGroup> groups)
        {
            if (groups.IsEmpty())
            {
                return;
            }

            var instanceGroupSet = result.Groups.Where(g => g.IsInstance).Select(g => g.Name).ToHashSet();

            groups.ForEach(g =>
            {
                if (!instanceGroupSet.Contains(g.Name))
                {
                    result.Errors.Add(new WorkflowDataValidationError
                    {
                        Element = g.Name,
                        ErrorCode = WorkflowDataValidationErrorCodes.InstanceGroupNotFound
                    });
                }
            });
        }

        private void ValidateTriggerData(WorkflowDataValidationResult result, IeTrigger trigger)
        {
            if (trigger == null)
            {
                return;
            }

            ValidateConditionData(result, trigger.Condition);
            ValidateActionData(result, trigger.Action);
        }

        private void ValidateConditionData(WorkflowDataValidationResult result, IeCondition condition)
        {
            if (condition == null)
            {
                return;
            }

            // For now the only condition IeStateCondition does not require data validation.
        }

        public virtual void ValidateActionData(WorkflowDataValidationResult result, IeBaseAction action)
        {
            if (action == null)
            {
                return;
            }

            switch (action.ActionType)
            {
                case ActionTypes.EmailNotification:
                    ValidateEmailNotificationActionData(result, (IeEmailNotificationAction) action);
                    break;
                case ActionTypes.PropertyChange:
                    ValidatePropertyChangeActionData(result, (IePropertyChangeAction) action);
                    break;
                case ActionTypes.Generate:
                    ValidateGenerateActionData(result, (IeGenerateAction) action);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(action.ActionType));
            }
        }

        public virtual void ValidateEmailNotificationActionData(WorkflowDataValidationResult result, IeEmailNotificationAction action)
        {
            if (action?.PropertyName != null
                && !result.StandardPropertyTypeMap.ContainsKey(action.PropertyName)
                && action.PropertyName != WorkflowConstants.PropertyNameName
                && action.PropertyName != WorkflowConstants.PropertyNameDescription)
            {
                result.Errors.Add(new WorkflowDataValidationError
                {
                    Element = action.PropertyName,
                    ErrorCode = WorkflowDataValidationErrorCodes.EmailNotificationActionPropertyTypeNotFound
                });
            }
        }

        public virtual void ValidatePropertyChangeActionData(WorkflowDataValidationResult result, IePropertyChangeAction action)
        {
            if (action == null)
            {
                return;
            }

            PropertyType propertyType;
            if (!result.StandardPropertyTypeMap.TryGetValue(action.PropertyName, out propertyType))
            {
                result.Errors.Add(new WorkflowDataValidationError
                {
                    Element = action.PropertyName,
                    ErrorCode = WorkflowDataValidationErrorCodes.PropertyChangeActionPropertyTypeNotFound
                });

                return;
            }

            WorkflowDataValidationErrorCodes? errorCode;
            if (!_propertyValueValidator.ValidatePropertyValue(action, propertyType,
                result.Users.Select(u => u.Login).ToHashSet(), result.Groups.Select(g => g.Name).ToHashSet(),
                out errorCode))
            {
                result.Errors.Add(new WorkflowDataValidationError
                {
                    Element = action.PropertyName,
                    ErrorCode = errorCode.Value
                });
            }
        }


        public virtual void ValidateGenerateActionData(WorkflowDataValidationResult result, IeGenerateAction action)
        {
            if (action == null)
            {
                return;
            }

            switch (action.GenerateActionType)
            {
                case GenerateActionTypes.Children:
                    ValidateGenerateChildArtifactsActionData(result, action);
                    break;
                case GenerateActionTypes.UserStories:
                case GenerateActionTypes.TestCases:
                    // No data validation is required.
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(action.GenerateActionType));
            }
        }

        public virtual void ValidateGenerateChildArtifactsActionData(WorkflowDataValidationResult result, IeGenerateAction action)
        {
            if (action == null)
            {
                return;
            }

            if (!result.StandardArtifactTypeMap.ContainsKey(action.ArtifactType))
            {
                result.Errors.Add(new WorkflowDataValidationError
                {
                    Element = action.ArtifactType,
                    ErrorCode = WorkflowDataValidationErrorCodes.GenerateChildArtifactsActionArtifactTypeNotFound
                });
            }
        }
    }
}