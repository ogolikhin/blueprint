﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AdminStore.Helpers.Workflow;
using AdminStore.Models.Workflow;
using AdminStore.Repositories.Workflow;
using AdminStore.Services.Workflow.Validation.Data.PropertyValue;
using ServiceLibrary.Helpers;
using ServiceLibrary.Models.Enums;
using ServiceLibrary.Models.ProjectMeta;
using ServiceLibrary.Repositories;
using ServiceLibrary.Repositories.ProjectMeta;

namespace AdminStore.Services.Workflow.Validation.Data
{
    public class WorkflowDataValidator : IWorkflowDataValidator
    {
        private readonly IWorkflowRepository _workflowRepository;
        private readonly IUsersRepository _userRepository;
        private readonly IProjectMetaRepository _projectMetaRepository;
        private readonly IPropertyValueValidatorFactory _propertyValueValidatorFactory;

        public WorkflowDataValidator(
            IWorkflowRepository workflowRepository,
            IUsersRepository userRepository,
            IProjectMetaRepository projectMetaRepository,
            IPropertyValueValidatorFactory propertyValueValidatorFactory)
        {
            _workflowRepository = workflowRepository;
            _userRepository = userRepository;
            _projectMetaRepository = projectMetaRepository;
            _propertyValueValidatorFactory = propertyValueValidatorFactory;
        }

        #region Interface Implementation

        public async Task<WorkflowDataValidationResult> ValidateDataAsync(IeWorkflow workflow)
        {
            if (workflow == null)
            {
                throw new ArgumentNullException(nameof(workflow));
            }

            var result = await InitializeDataValidationResultAsync(workflow, true);

            await ValidateWorkflowNameForUniquenessAsync(result, workflow);
            await ValidateProjectsDataAsync(result, workflow.Projects, false);
            ValidateArtifactTypesDataAsync(result, workflow.Projects, true);
            await ValidateEventsDataAsync(result, workflow, true);

            return result;
        }

        // During the update data validation names of elements with Id are assigned according to the meta data.
        public async Task<WorkflowDataValidationResult> ValidateUpdateDataAsync(IeWorkflow workflow, ProjectTypes standardTypes)
        {
            if (workflow == null)
            {
                throw new ArgumentNullException(nameof(workflow));
            }

            var result = await InitializeDataValidationResultAsync(workflow, false, standardTypes);

            await ValidateWorkflowNameForUniquenessAsync(result, workflow, workflow.Id);
            await ValidateProjectsDataAsync(result, workflow.Projects, true);
            ValidateArtifactTypesDataAsync(result, workflow.Projects, false);
            await ValidateEventsDataAsync(result, workflow, false);

            return result;
        }

        #endregion

        #region Private methods

        private async Task<WorkflowDataValidationResult> InitializeDataValidationResultAsync(IeWorkflow workflow, bool ignoreIds, ProjectTypes standardTypes = null)
        {
            var result = new WorkflowDataValidationResult();

            result.StandardTypes = standardTypes ?? await _projectMetaRepository.GetStandardProjectTypesAsync();
            result.StandardTypes.ArtifactTypes?.RemoveAll(at => at.PredefinedType != null
                                                                && !at.PredefinedType.Value.IsRegularArtifactType());
            result.StandardArtifactTypeMapByName.AddRange(result.StandardTypes.ArtifactTypes.ToDictionary(pt => pt.Name));
            result.StandardPropertyTypeMapByName.AddRange(result.StandardTypes.PropertyTypes.ToDictionary(pt => pt.Name));
            ISet<string> groupNamesToLookup;
            ISet<string> userNamesToLookup;
            ISet<int> groupIdsToLookup;
            ISet<int> userIdsToLookup;
            CollectUsersAndGroupsToLookup(workflow, out userNamesToLookup, out groupNamesToLookup,
                out userIdsToLookup, out groupIdsToLookup, ignoreIds);
            result.Users.AddRange(await _userRepository.GetExistingUsersByNamesAsync(userNamesToLookup));
            result.Groups.AddRange(await _userRepository.GetExistingGroupsByNamesAsync(groupNamesToLookup, false));

            if (!ignoreIds)
            {
                result.StandardArtifactTypeMapById.AddRange(result.StandardTypes.ArtifactTypes.ToDictionary(pt => pt.Id));
                result.StandardPropertyTypeMapById.AddRange(result.StandardTypes.PropertyTypes.ToDictionary(pt => pt.Id));
                result.Users.AddRange(await _userRepository.GetExistingUsersByIdsAsync(userIdsToLookup));
                result.Groups.AddRange(await _userRepository.GetExistingGroupsByIds(groupIdsToLookup, false));
            }

            return result;
        }

        private static void CollectUsersAndGroupsToLookup(IeWorkflow workflow, out ISet<string> userNamesToLookup,
            out ISet<string> groupNamesToLookup, out ISet<int> userIdsToLookup, out ISet<int> groupIdsToLookup, bool ignoreIds)
        {
            var userNames = new HashSet<string>();
            var groupNames = new HashSet<string>();
            var userIds = new HashSet<int>();
            var groupIds = new HashSet<int>();

            workflow.TransitionEvents?.ForEach(t =>
            {
                t?.PermissionGroups?.ForEach(g =>
                {
                    if (!ignoreIds && g.Id.HasValue)
                    {
                        groupIds.Add(g.Id.Value);
                    }
                    else
                    {
                        groupNames.Add(g.Name);
                    }
                });
            });

            workflow.TransitionEvents?.ForEach(te => CollectUsersAndGroupsToLookup(te, userNames, groupNames,
                userIds, groupIds, ignoreIds));
            workflow.PropertyChangeEvents?.ForEach(pce => CollectUsersAndGroupsToLookup(pce, userNames, groupNames,
                userIds, groupIds, ignoreIds));
            workflow.NewArtifactEvents?.ForEach(nae => CollectUsersAndGroupsToLookup(nae, userNames, groupNames,
                userIds, groupIds, ignoreIds));

            userNamesToLookup = userNames;
            groupNamesToLookup = groupNames;
            userIdsToLookup = ignoreIds ? null : userIds;
            groupIdsToLookup = ignoreIds ? null : groupIds;
        }

        private static void CollectUsersAndGroupsToLookup(IeEvent wEvent, ISet<string> userNames, ISet<string> groupNames,
            ISet<int> userIds, ISet<int> groupIds, bool ignoreIds)
        {
            wEvent?.Triggers?.ForEach(t =>
            {
                if (t?.Action?.ActionType == ActionTypes.PropertyChange)
                {
                    var action = (IePropertyChangeAction)t.Action;
                    action.UsersGroups?.UsersGroups?.ForEach(ug =>
                    {
                        if (ug.IsGroup.GetValueOrDefault())
                        {
                            if (!ignoreIds && ug.Id.HasValue)
                            {
                                groupIds.Add(ug.Id.Value);
                            }
                            else
                            {
                                groupNames.Add(ug.Name);
                            }
                        }
                        else
                        {
                            if (!ignoreIds && ug.Id.HasValue)
                            {
                                userIds.Add(ug.Id.Value);
                            }
                            else
                            {
                                userNames.Add(ug.Name);
                            }
                        }
                    });
                }
            });
        }

        private async Task ValidateWorkflowNameForUniquenessAsync(WorkflowDataValidationResult result, IeWorkflow workflow, int? exceptWorkflowId = null)
        {
            var duplicateNames = await _workflowRepository.CheckLiveWorkflowsForNameUniquenessAsync(new[] { workflow.Name }, exceptWorkflowId);
            if (duplicateNames.Any())
            {
                result.Errors.Add(new WorkflowDataValidationError
                {
                    Element = workflow,
                    ErrorCode = WorkflowDataValidationErrorCodes.WorkflowNameNotUnique
                });
            }
        }

        private async Task ValidateProjectsDataAsync(WorkflowDataValidationResult result, List<IeProject> projects,
            bool doNotLookupProjectPaths)
        {
            result.ValidProjectIds.Clear();
            var projectPaths = new Dictionary<int, string>();
            var projectPathsToLookup = new Dictionary<string, IeProject>();
            if (!projects.IsEmpty())
            {
                projects.ForEach(project =>
                {
                    if (project.Id.HasValue)
                    {
                        projectPaths[project.Id.Value] = project.Path;
                    }
                    else if (!doNotLookupProjectPaths)
                    {
                        if (!string.IsNullOrEmpty(project.Path))
                        {
                            projectPathsToLookup.Add(project.Path, project);
                        }
                    }
                });

                if (!doNotLookupProjectPaths && projectPathsToLookup.Any())
                {
                    // look up ID of projects that have no ID provided
                    foreach (
                        var sqlProjectPathPair in
                            await _workflowRepository.GetProjectIdsByProjectPathsAsync(projectPathsToLookup.Keys))
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

                foreach (var project in projects
                    .Where(p => !string.IsNullOrEmpty(p.Path) && !p.Id.HasValue))
                {
                    result.Errors.Add(new WorkflowDataValidationError
                    {
                        Element = project.Path,
                        ErrorCode = WorkflowDataValidationErrorCodes.ProjectByPathNotFound
                    });
                }

                var projectIds = projectPaths.Select(p => p.Key).ToHashSet();
                var validProjectIds = projectIds.Any()
                    ? (await _workflowRepository.GetExistingProjectsByIdsAsync(projectIds)).ToArray()
                    : new int[0];
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

                if (projects.GroupBy(p => p.Id).Any(g => g.Count() > 1))
                {
                    result.Errors.Add(new WorkflowDataValidationError
                    {
                        ErrorCode = WorkflowDataValidationErrorCodes.ProjectDuplicate
                    });
                }

                result.ValidProjectIds.AddRange(validProjectIds);
            }
        }

        private static void ValidateArtifactTypesDataAsync(WorkflowDataValidationResult result, List<IeProject> projects, bool ignoreIds)
        {
            if (projects.IsEmpty() || result.ValidProjectIds.IsEmpty())
            {
                return;
            }

            // Update Name where Id is present (to null if Id is not found)
            if (!ignoreIds)
            {
                projects.ForEach(p => p.ArtifactTypes?.ForEach(at =>
                {
                    ItemType itemType;
                    if (at.Id.HasValue)
                    {
                        if (!result.StandardArtifactTypeMapById.TryGetValue(at.Id.Value, out itemType))
                        {
                            result.Errors.Add(new WorkflowDataValidationError
                            {
                                Element = at.Id.Value,
                                ErrorCode = WorkflowDataValidationErrorCodes.StandardArtifactTypeNotFoundById
                            });
                        }
                        at.Name = itemType?.Name;
                    }
                    else
                    {
                        // Assing Id to artifact types for the workflow diffing.
                        // A negative artifact type Id means Id is not specified in xml.
                        if (result.StandardArtifactTypeMapByName.TryGetValue(at.Name, out itemType)
                            && itemType != null)
                        {
                            at.Id = itemType.Id * -1;
                        }
                    }
                }));
            }

            var artifactTypesInProjects = projects.SelectMany(p => p.ArtifactTypes?.Where(at => at.Name != null).Select(at => at.Name)).ToList();

            artifactTypesInProjects.ForEach(at =>
            {
                ItemType itemType;
                if (!result.StandardArtifactTypeMapByName.TryGetValue(at, out itemType))
                {
                    result.Errors.Add(new WorkflowDataValidationError
                    {
                        Element = at,
                        ErrorCode = WorkflowDataValidationErrorCodes.StandardArtifactTypeNotFoundByName
                    });
                }
                else
                {
                    result.AssociatedArtifactTypeIds.Add(itemType.Id);
                }
            });
        }

        private async Task ValidateEventsDataAsync(WorkflowDataValidationResult result, IeWorkflow workflow,
            bool ignoreIds)
        {
            // For the workflow update Ids are already filled in.
            if (ignoreIds)
            {
                await FillInGroupProjectIdsAsync(result, workflow);
            }

            workflow.TransitionEvents?.ForEach(t => ValidateTransitionData(result, t, ignoreIds));
            workflow.PropertyChangeEvents?.ForEach(pce => ValidatePropertyChangeEventData(result, pce, ignoreIds));
            workflow.NewArtifactEvents?.ForEach(nae => ValidateNewArtifactEventData(result, nae, ignoreIds));
        }

        private async Task FillInGroupProjectIdsAsync(WorkflowDataValidationResult result, IeWorkflow workflow)
        {
            var groupsWithoutProjectId = new List<IeUserGroup>();
            workflow.TransitionEvents?.ForEach(t => CollectGroupsWithUnassignedProjectId(t, groupsWithoutProjectId));
            workflow.PropertyChangeEvents?.ForEach(
                pce => CollectGroupsWithUnassignedProjectId(pce, groupsWithoutProjectId));
            workflow.NewArtifactEvents?.ForEach(nae => CollectGroupsWithUnassignedProjectId(nae, groupsWithoutProjectId));

            if (!groupsWithoutProjectId.Any())
            {
                return;
            }

            var projectMap = (await
                _workflowRepository.GetProjectIdsByProjectPathsAsync(groupsWithoutProjectId.Select(g => g.GroupProjectPath)))
                .ToDictionary(p => p.ProjectPath, p => p.ProjectId);
            groupsWithoutProjectId.ForEach(g =>
            {
                int projectId;
                if (projectMap.TryGetValue(g.GroupProjectPath, out projectId))
                {
                    g.GroupProjectId = projectId;
                }
                else
                {
                    result.Errors.Add(new WorkflowDataValidationError
                    {
                        Element = g.GroupProjectPath,
                        ErrorCode = WorkflowDataValidationErrorCodes.ProjectByPathNotFound
                    });
                }
            });
        }

        private static void CollectGroupsWithUnassignedProjectId(IeEvent wEvent, ICollection<IeUserGroup> collection)
        {
            wEvent?.Triggers?.ForEach(t =>
            {
                if (t?.Action.ActionType == ActionTypes.PropertyChange)
                {
                    var pca = (IePropertyChangeAction)t.Action;
                    pca?.UsersGroups?.UsersGroups?.Where(IsGroupProjectIdUnassigned).ForEach(collection.Add);
                }
            });
        }

        private static bool IsGroupProjectIdUnassigned(IeUserGroup userGroup)
        {
            return userGroup != null && userGroup.IsGroup.GetValueOrDefault()
                   && !string.IsNullOrEmpty(userGroup.GroupProjectPath)
                   && !userGroup.GroupProjectId.HasValue;
        }

        private void ValidateTransitionData(WorkflowDataValidationResult result, IeTransitionEvent transition,
            bool ignoreIds)
        {
            if (transition == null)
            {
                return;
            }

            ValidatePermissionGroupsData(result, transition.PermissionGroups, ignoreIds);
            transition.Triggers?.ForEach(t => ValidateTriggerData(result, t, ignoreIds));
        }

        internal void ValidatePropertyChangeEventData(WorkflowDataValidationResult result, IePropertyChangeEvent pcEvent, bool ignoreIds)
        {
            if (pcEvent == null)
            {
                return;
            }

            PropertyType propertyType;

            // Update Name where Id is present (to null if Id is not found)
            if (!ignoreIds && pcEvent.PropertyId.HasValue)
            {
                if (!WorkflowHelper.TryGetNameOrDescriptionPropertyType(pcEvent.PropertyId.Value, out propertyType)
                    && !result.StandardPropertyTypeMapById.TryGetValue(pcEvent.PropertyId.Value, out propertyType))
                {
                    result.Errors.Add(new WorkflowDataValidationError
                    {
                        Element = pcEvent.PropertyId.Value,
                        ErrorCode = WorkflowDataValidationErrorCodes.PropertyNotFoundById
                    });
                }

                pcEvent.PropertyName = propertyType?.Name;
            }

            if (pcEvent.PropertyName != null)
            {
                if (!WorkflowHelper.TryGetNameOrDescriptionPropertyType(pcEvent.PropertyName, out propertyType)
                && !result.StandardPropertyTypeMapByName.TryGetValue(pcEvent.PropertyName, out propertyType))
                {
                    result.Errors.Add(new WorkflowDataValidationError
                    {
                        Element = pcEvent.PropertyName,
                        ErrorCode = WorkflowDataValidationErrorCodes.PropertyNotFoundByName
                    });

                    return;
                }

                if (!IsAssociatedWithWorkflowArtifactTypes(propertyType, result))
                {
                    result.Errors.Add(new WorkflowDataValidationError
                    {
                        Element = pcEvent.PropertyName,
                        ErrorCode = WorkflowDataValidationErrorCodes.PropertyNotAssociated
                    });

                    return;
                }
            }

            pcEvent.Triggers?.ForEach(t => ValidateTriggerData(result, t, ignoreIds));
        }

        private void ValidateNewArtifactEventData(WorkflowDataValidationResult result, IeNewArtifactEvent naEvent,
            bool ignoreIds)
        {
            naEvent?.Triggers?.ForEach(t => ValidateTriggerData(result, t, ignoreIds));
        }

        private static void ValidatePermissionGroupsData(WorkflowDataValidationResult result, List<IeGroup> groups,
            bool ignoreIds)
        {
            if (groups.IsEmpty())
            {
                return;
            }

            var instanceGroupNames = new HashSet<string>();
            var instanceGroupMapById = new Dictionary<int, string>();
            result.Groups.Where(g => g.ProjectId == null).ForEach(g =>
            {
                instanceGroupNames.Add(g.Name);
                instanceGroupMapById.Add(g.GroupId, g.Name);
            });

            groups.ForEach(g =>
            {
                // Update Name where Id is present (to null if Id is not found)
                if (!ignoreIds && g.Id.HasValue)
                {
                    string name;
                    if (!instanceGroupMapById.TryGetValue(g.Id.Value, out name))
                    {
                        result.Errors.Add(new WorkflowDataValidationError
                        {
                            Element = g.Id.Value,
                            ErrorCode = WorkflowDataValidationErrorCodes.InstanceGroupNotFoundById
                        });
                    }
                    g.Name = name;
                }

                if (g.Name != null && !instanceGroupNames.Contains(g.Name))
                {
                    result.Errors.Add(new WorkflowDataValidationError
                    {
                        Element = g.Name,
                        ErrorCode = WorkflowDataValidationErrorCodes.InstanceGroupNotFoundByName
                    });
                }
            });
        }

        private void ValidateTriggerData(WorkflowDataValidationResult result, IeTrigger trigger,
            bool ignoreIds)
        {
            if (trigger == null)
            {
                return;
            }

            ValidateConditionData(result, trigger.Condition, ignoreIds);
            ValidateActionData(result, trigger.Action, ignoreIds);
        }

        private static void ValidateConditionData(WorkflowDataValidationResult result, IeCondition condition,
            bool ignoreIds)
        {
            if (condition == null)
            {
                return;
            }

            // For now the only condition IeStateCondition does not require data validation.
        }

        public virtual void ValidateActionData(WorkflowDataValidationResult result, IeBaseAction action,
            bool ignoreIds)
        {
            if (action == null)
            {
                return;
            }

            switch (action.ActionType)
            {
                case ActionTypes.EmailNotification:
                    ValidateEmailNotificationActionData(result, (IeEmailNotificationAction)action, ignoreIds);
                    break;
                case ActionTypes.PropertyChange:
                    ValidatePropertyChangeActionData(result, (IePropertyChangeAction)action, ignoreIds);
                    break;
                case ActionTypes.Generate:
                    ValidateGenerateActionData(result, (IeGenerateAction)action, ignoreIds);
                    break;
                case ActionTypes.Webhook:
                    // No data to validate
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(action.ActionType));
            }
        }

        public virtual void ValidateEmailNotificationActionData(WorkflowDataValidationResult result,
            IeEmailNotificationAction action, bool ignoreIds)
        {
            if (action == null)
            {
                return;
            }

            PropertyType propertyType;

            // Update Name where Id is present (to null if Id is not found)
            if (!ignoreIds && action.PropertyId.HasValue)
            {
                if (!result.StandardPropertyTypeMapById.TryGetValue(action.PropertyId.Value, out propertyType))
                {
                    result.Errors.Add(new WorkflowDataValidationError
                    {
                        Element = action.PropertyId.Value,
                        ErrorCode = WorkflowDataValidationErrorCodes.EmailNotificationActionPropertyTypeNotFoundById
                    });
                }
                action.PropertyName = propertyType?.Name;
            }

            if (action.PropertyName == null)
            {
                return;
            }

            if (!result.StandardPropertyTypeMapByName.TryGetValue(action.PropertyName, out propertyType))
            {
                result.Errors.Add(new WorkflowDataValidationError
                {
                    Element = action.PropertyName,
                    ErrorCode = WorkflowDataValidationErrorCodes.EmailNotificationActionPropertyTypeNotFoundByName
                });
            }
            else
            {
                if (propertyType.PrimitiveType != PropertyPrimitiveType.Text && propertyType.PrimitiveType != PropertyPrimitiveType.User)
                {
                    result.Errors.Add(new WorkflowDataValidationError
                    {
                        Element = action.PropertyName,
                        ErrorCode = WorkflowDataValidationErrorCodes.EmailNotificationActionUnacceptablePropertyType
                    });
                }

                if (!IsAssociatedWithWorkflowArtifactTypes(propertyType, result))
                {
                    result.Errors.Add(new WorkflowDataValidationError
                    {
                        Element = action.PropertyName,
                        ErrorCode = WorkflowDataValidationErrorCodes.EmailNotificationActionPropertyTypeNotAssociated
                    });
                }
            }
        }

        public virtual void ValidatePropertyChangeActionData(WorkflowDataValidationResult result,
            IePropertyChangeAction action, bool ignoreIds)
        {
            if (action == null)
            {
                return;
            }

            PropertyType propertyType;

            // Update Name where Id is present (to null if Id is not found)
            if (!ignoreIds && action.PropertyId.HasValue)
            {
                if (!result.StandardPropertyTypeMapById.TryGetValue(action.PropertyId.Value, out propertyType)
                    && !WorkflowHelper.TryGetNameOrDescriptionPropertyType(action.PropertyId.Value, out propertyType))
                {
                    result.Errors.Add(new WorkflowDataValidationError
                    {
                        Element = action.PropertyId.Value,
                        ErrorCode = WorkflowDataValidationErrorCodes.PropertyChangeActionPropertyTypeNotFoundById
                    });
                }

                action.PropertyName = propertyType?.Name;
            }

            if (action.PropertyName == null)
            {
                return;
            }

            if (!result.StandardPropertyTypeMapByName.TryGetValue(action.PropertyName, out propertyType)
                && !WorkflowHelper.TryGetNameOrDescriptionPropertyType(action.PropertyName, out propertyType))
            {
                result.Errors.Add(new WorkflowDataValidationError
                {
                    Element = action.PropertyName,
                    ErrorCode = WorkflowDataValidationErrorCodes.PropertyChangeActionPropertyTypeNotFoundByName
                });

                return;
            }

            if (!IsAssociatedWithWorkflowArtifactTypes(propertyType, result))
            {
                result.Errors.Add(new WorkflowDataValidationError
                {
                    Element = action.PropertyName,
                    ErrorCode = WorkflowDataValidationErrorCodes.PropertyChangeActionPropertyTypeNotAssociated
                });

                return;
            }

            var propertyValueValidator = _propertyValueValidatorFactory.Create(propertyType, result.Users, result.Groups, ignoreIds);
            propertyValueValidator.Validate(action, propertyType, result);
        }

        public virtual void ValidateGenerateActionData(WorkflowDataValidationResult result, IeGenerateAction action, bool ignoreIds)
        {
            if (action == null)
            {
                return;
            }

            switch (action.GenerateActionType)
            {
                case GenerateActionTypes.Children:
                    ValidateGenerateChildArtifactsActionData(result, action, ignoreIds);
                    break;
                case GenerateActionTypes.UserStories:
                case GenerateActionTypes.TestCases:
                    // No data validation is required.
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(action.GenerateActionType));
            }
        }

        public virtual void ValidateGenerateChildArtifactsActionData(WorkflowDataValidationResult result,
            IeGenerateAction action, bool ignoreIds)
        {
            if (action == null)
            {
                return;
            }

            // Update Name where Id is present (to null if Id is not found)
            if (!ignoreIds && action.ArtifactTypeId.HasValue)
            {
                ItemType itemType;
                if (!result.StandardArtifactTypeMapById.TryGetValue(action.ArtifactTypeId.Value, out itemType))
                {
                    result.Errors.Add(new WorkflowDataValidationError
                    {
                        Element = action.ArtifactTypeId.Value,
                        ErrorCode = WorkflowDataValidationErrorCodes.GenerateChildArtifactsActionArtifactTypeNotFoundById
                    });
                }
                action.ArtifactType = itemType?.Name;
            }

            if (action.ArtifactType == null)
            {
                return;
            }

            if (!result.StandardArtifactTypeMapByName.ContainsKey(action.ArtifactType))
            {
                result.Errors.Add(new WorkflowDataValidationError
                {
                    Element = action.ArtifactType,
                    ErrorCode = WorkflowDataValidationErrorCodes.GenerateChildArtifactsActionArtifactTypeNotFoundByName
                });
            }
        }

        private static bool IsAssociatedWithWorkflowArtifactTypes(PropertyType propertyType, WorkflowDataValidationResult result)
        {
            if (WorkflowHelper.IsNameOrDescriptionProperty(propertyType.Id))
            {
                return result.AssociatedArtifactTypeIds.Any();
            }

            return result.StandardTypes.ArtifactTypes
                .Where(at => result.AssociatedArtifactTypeIds.Contains(at.Id))
                .SelectMany(at => at.CustomPropertyTypeIds)
                .Contains(propertyType.Id);
        }
    }

    #endregion
}
