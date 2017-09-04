using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using ServiceLibrary.Exceptions;
using ServiceLibrary.Helpers;
using ServiceLibrary.Models;
using ServiceLibrary.Models.Enums;
using ServiceLibrary.Models.ProjectMeta;
using ServiceLibrary.Models.PropertyType;
using ServiceLibrary.Models.Workflow;
using ServiceLibrary.Models.Workflow.Actions;
using ServiceLibrary.Repositories.ProjectMeta.PropertyXml;

namespace ServiceLibrary.Repositories.Workflow
{
    public class SqlWorkflowRepository : SqlBaseArtifactRepository, IWorkflowRepository
    {
        public SqlWorkflowRepository()
            : this(new SqlConnectionWrapper(ServiceConstants.RaptorMain))
        {
        }

        public SqlWorkflowRepository(ISqlConnectionWrapper connectionWrapper)
            : this(connectionWrapper, new SqlArtifactPermissionsRepository(connectionWrapper))
        {
        }

        public SqlWorkflowRepository(ISqlConnectionWrapper connectionWrapper,
            IArtifactPermissionsRepository artifactPermissionsRepository) 
            : base(connectionWrapper,artifactPermissionsRepository)
        {
        }

        #region artifact workflow

        public async Task<IList<WorkflowTransition>> GetTransitionsAsync(int userId, int artifactId, int workflowId, int stateId)
        {
            //Do not return transitions if the user does not have edit permissions
            await CheckForArtifactPermissions(userId, artifactId, permissions: RolePermissions.Edit);
            
            return await GetTransitionsForStateInternalAsync(userId, workflowId, stateId);
        }

        public async Task<WorkflowTransition> GetTransitionForAssociatedStatesAsync(int userId, int artifactId, int workflowId, int fromStateId, int toStateId)
        {
            //Do not return transitions if the user does not have edit permissions
            await CheckForArtifactPermissions(userId, artifactId, permissions: RolePermissions.Edit);

            return await GetTransitionForAssociatedStatesInternalAsync(userId, workflowId, fromStateId, toStateId);
        }

        public async Task<WorkflowTriggersContainer> GetWorkflowEventTriggersForTransition(int userId, int artifactId, int workflowId,
            int fromStateId, int toStateId)
        {
            var desiredTransition = await GetTransitionForAssociatedStatesAsync(userId, artifactId, workflowId, fromStateId, toStateId);

            if (desiredTransition == null)
            {
                throw new ConflictException(I18NHelper.FormatInvariant("No transitions available. Workflow could have been updated. Please refresh your view."));
            }
            return GetWorkflowTriggersContainer(desiredTransition.Triggers);
        }

        public async Task<WorkflowState> GetStateForArtifactAsync(int userId, int artifactId, int revisionId, bool addDrafts)
        {
            //Need to access code for artifact permissions for revision
            await CheckForArtifactPermissions(userId, artifactId, revisionId);
            
            return await GetCurrentStateInternal(userId, artifactId, revisionId, addDrafts);
        }

        public async Task<WorkflowState> ChangeStateForArtifactAsync(
            int userId, 
            int artifactId, 
            WorkflowStateChangeParameterEx stateChangeParameter, 
            IDbTransaction transaction = null)
        {
            //Need to access code for artifact permissions for revision
            await CheckForArtifactPermissions(userId, artifactId, permissions: RolePermissions.Edit);

            return await ChangeStateForArtifactInternal(
                userId, 
                artifactId, 
                stateChangeParameter.ToStateId,
                transaction);
        }

        public async Task<Dictionary<int, List<WorkflowPropertyType>>> GetCustomItemTypeToPropertiesMap(
            int userId,
            int artifactId,
            int projectId,
            IEnumerable<int> instanceItemTypeIds,
            IEnumerable<int> instancePropertyIds)
        {
            //Need to access code for artifact permissions for revision
            await CheckForArtifactPermissions(userId, artifactId, permissions: RolePermissions.Edit);

            return await GetCustomPropertyTypesFromStandardIds(instanceItemTypeIds, instancePropertyIds, projectId);
        }

        public async Task<WorkflowTriggersContainer> GetWorkflowEventTriggersForNewArtifactEvent(int userId,
            int artifactId, int revisionId)
        {
            //Need to access code for artifact permissions for revision
            await CheckForArtifactPermissions(userId, artifactId, permissions: RolePermissions.Read);

            return await GetWorkflowEventTriggersForNewArtifactEventInternal(userId, artifactId, revisionId);
        }

        public async Task<WorkflowTriggersContainer> GetWorkflowEventTriggersForNewArtifactEvent(int userId,
            IEnumerable<int> artifactIds, 
            int revisionId)
        {
            foreach (var artifactId in artifactIds)
            {
                await CheckForArtifactPermissions(userId, artifactId, permissions: RolePermissions.Read);
            }
            return await GetWorkflowEventTriggersForNewArtifactEventInternal(userId, artifactIds, revisionId);
        }

        public async Task<IEnumerable<WorkflowMessageArtifactInfo>> GetWorkflowMessageArtifactInfoAsync(int userId,
            IEnumerable<int> artifactIds, int revisionId, IDbTransaction transaction = null)
        {
            return await GetWorkflowMessageArtifactInfoAsyncInternal(userId, artifactIds, revisionId, transaction);
        }

        #endregion

        #region Private methods

        private async Task<WorkflowTriggersContainer> GetWorkflowEventTriggersForNewArtifactEventInternal(int userId, int artifactId, int revisionId)
        {
            var param = new DynamicParameters();
            param.Add("@userId", userId);
            param.Add("@artifactId", artifactId);
            param.Add("@revisionId", revisionId);
            var newArtifactEvents = (await
                ConnectionWrapper.QueryAsync<SqlWorkflowNewArtifactEvent>("GetWorkflowEventTriggersForNewArtifact",
                    param,
                    commandType: CommandType.StoredProcedure)).ToList();
            var eventTriggers = new WorkflowEventTriggers();
            newArtifactEvents.Where(n => n != null).ForEach(n =>
            {
                eventTriggers.AddRange(ToWorkflowTriggers(SerializationHelper.FromXml<XmlWorkflowEventTriggers>(n.Triggers), userId));
            });
            return GetWorkflowTriggersContainer(eventTriggers);
        }

        
        private static WorkflowTriggersContainer GetWorkflowTriggersContainer(WorkflowEventTriggers eventTriggers)
        {
            var workflowTriggersContainer = new WorkflowTriggersContainer();
            foreach (var workflowEventTrigger in eventTriggers.Where(t => t?.Action != null))
            {
                if (workflowEventTrigger.Action is IWorkflowEventSynchronousAction)
                {
                    workflowTriggersContainer.SynchronousTriggers.Add(workflowEventTrigger);
                }
                else if (workflowEventTrigger.Action is IWorkflowEventASynchronousAction)
                {
                    workflowTriggersContainer.AsynchronousTriggers.Add(workflowEventTrigger);
                }
            }
            return workflowTriggersContainer;
        }

        private async Task<WorkflowTriggersContainer> GetWorkflowEventTriggersForNewArtifactEventInternal(int userId, 
            IEnumerable<int> artifactIds, 
            int revisionId)
        {
            var param = new DynamicParameters();
            param.Add("@userId", userId);
            var artifactIdsTable = SqlConnectionWrapper.ToDataTable(artifactIds);
            param.Add("@artifactIds", artifactIdsTable);
            param.Add("@revisionId", revisionId);
            var newArtifactEvents = (await
                ConnectionWrapper.QueryAsync<SqlWorkflowNewArtifactEvent>("GetWorkflowEventTriggersForNewArtifact",
                    param,
                    commandType: CommandType.StoredProcedure)).ToList();
            var eventTriggers = new WorkflowEventTriggers();
            newArtifactEvents.Where(n => n != null).ForEach(n =>
            {
                eventTriggers.AddRange(ToWorkflowTriggers(SerializationHelper.FromXml<XmlWorkflowEventTriggers>(n.Triggers), userId));
            });
            return GetWorkflowTriggersContainer(eventTriggers);
        }

        

        private async Task<WorkflowState> GetCurrentStateInternal(int userId, int artifactId, int revisionId, bool addDrafts)
        {
            return (await GetCurrentStatesInternal(userId, 
                new [] { artifactId }, revisionId, addDrafts)).FirstOrDefault();
        }

        private async Task<IList<WorkflowState>> GetCurrentStatesInternal(int userId, IEnumerable<int>  artifactIds, int revisionId, bool addDrafts)
        {
            var param = new DynamicParameters();
            param.Add("@userId", userId);
            var artifactIdsTable = SqlConnectionWrapper.ToDataTable(artifactIds);
            param.Add("@artifactIds", artifactIdsTable);
            param.Add("@revisionId", revisionId);
            param.Add("@addDrafts", addDrafts);

            return ToWorkflowStates(
                await 
                    ConnectionWrapper.QueryAsync<SqlWorkFlowStateInformation>("GetWorkflowStatesForArtifacts", param, 
                        commandType: CommandType.StoredProcedure));
        }

        private async Task<IList<WorkflowTransition>> GetTransitionsForStateInternalAsync(int userId, int workflowId, int stateId)
        {
            var param = new DynamicParameters();
            param.Add("@workflowId", workflowId);
            param.Add("@stateId", stateId);
            param.Add("@userId", userId);

            return ToWorkflowTransitions(
                    await
                        ConnectionWrapper.QueryAsync<SqlWorkflowTransition>("GetTransitionsForState", param,
                            commandType: CommandType.StoredProcedure), userId);
        }

        private async Task<WorkflowTransition> GetTransitionForAssociatedStatesInternalAsync(int userId, int workflowId, int fromStateId, int toStateId)
        {
            var param = new DynamicParameters();
            param.Add("@workflowId", workflowId);
            param.Add("@fromStateId", fromStateId);
            param.Add("@toStateId", toStateId);
            param.Add("@userId", userId);

            return ToWorkflowTransitions(
                    await
                        ConnectionWrapper.QueryAsync<SqlWorkflowTransition>("GetTransitionAssociatedWithStates", param,
                            commandType: CommandType.StoredProcedure), userId).FirstOrDefault();
        }
        
        private IList<WorkflowTransition> ToWorkflowTransitions(IEnumerable<SqlWorkflowTransition> sqlWorkflowTransitions, int currentUserId)
        {
            return sqlWorkflowTransitions.Select(wt =>
            {
                var transition = new WorkflowTransition
                {
                    Id = wt.WorkflowEventId,
                    ToState = new WorkflowState
                    {
                        WorkflowId = wt.WorkflowId,
                        Id = wt.ToStateId,
                        Name = wt.ToStateName
                    },
                    FromState = new WorkflowState
                    {
                        WorkflowId = wt.WorkflowId,
                        Id = wt.FromStateId,
                        Name = wt.FromStateName
                    },
                    Name = wt.WorkflowEventName,
                    WorkflowId = wt.WorkflowId
                };
                transition.Triggers.AddRange(ToWorkflowTriggers(SerializationHelper.FromXml<XmlWorkflowEventTriggers>(wt.Triggers), currentUserId));
                return transition;
            }).ToList();
        }

        private WorkflowEventTriggers ToWorkflowTriggers(XmlWorkflowEventTriggers xmlWorkflowEventTriggers, int currentUserId)
        {
            WorkflowEventTriggers triggers = new WorkflowEventTriggers();
            if (xmlWorkflowEventTriggers == null || xmlWorkflowEventTriggers.Triggers == null)
            {
                return triggers;
            }
            triggers.AddRange(xmlWorkflowEventTriggers.Triggers.Select(xmlWorkflowEventTrigger => new WorkflowEventTrigger
            {
                Name = xmlWorkflowEventTrigger.Name,
                Condition = new WorkflowEventCondition(),
                Action = GenerateAction(xmlWorkflowEventTrigger.Action, currentUserId)
            }));
            return triggers;
        }

        private WorkflowEventAction GenerateAction(XmlAction action, int currentUserId)
        {
            if (action == null)
            {
                return null;
            }
            var emailNotification = action as XmlEmailNotificationAction;
            if (emailNotification != null)
            {
                return ToEmailNotificationAction(emailNotification);
            }
            var propertyChangeAction = action as XmlPropertyChangeAction;
            if (propertyChangeAction != null)
            {
                return ToPropertyChangeAction(propertyChangeAction, currentUserId);
            }
            var generateAction = action as XmlGenerateAction;
            //TODO: Should we throw an exception if the action is not a known action? Import ahead of handling situation
            return generateAction != null ? ToGenerateAction(generateAction) : null;
        }

        private EmailNotificationAction ToEmailNotificationAction(XmlEmailNotificationAction emailNotification)
        {
            var action = new EmailNotificationAction
            {
                PropertyTypeId = emailNotification.PropertyTypeId,
                Message = emailNotification.Message
            };
            action.Emails.AddRange(emailNotification.Emails);
            return action;
        }

        private PropertyChangeAction ToPropertyChangeAction(XmlPropertyChangeAction propertyChangeAction, int currentUserId)
        {
            if (propertyChangeAction.UsersGroups!= null)
            {
                return ToPropertyChangeUserGroupAction(propertyChangeAction, currentUserId);
            }
            else
            {
                var action = new PropertyChangeAction
                {
                    InstancePropertyTypeId = propertyChangeAction.PropertyTypeId,
                    PropertyValue = propertyChangeAction.PropertyValue,
                };
                if (propertyChangeAction.ValidValues.Any())
                {
                    action.ValidValues.AddRange(propertyChangeAction.ValidValues);
                }

                return action;
            }
        }

        private PropertyChangeAction ToPropertyChangeUserGroupAction(
            XmlPropertyChangeAction propertyChangeAction, 
            int currentUserId
            )
        {
            var action = new PropertyChangeUserGroupsAction
            {
                InstancePropertyTypeId = propertyChangeAction.PropertyTypeId,
                PropertyValue = propertyChangeAction.PropertyValue
            };
            if (propertyChangeAction.UsersGroups.UsersGroups?.Any() ?? false)
            {
                action.UserGroups.AddRange(propertyChangeAction.UsersGroups.UsersGroups.Select(
                    u => new UserGroup
                    {
                        Id = u.Id,
                        IsGroup = u.IsGroup
                    }).ToList());
            }
            var includeCurrentUser = propertyChangeAction.UsersGroups.IncludeCurrentUser.GetValueOrDefault(false);
            if (!includeCurrentUser)
            {
                return action;
            }
            var isUserAlreadyIncluded =
                action.UserGroups.Exists(
                    u => !u.IsGroup.GetValueOrDefault(false) && u.Id.GetValueOrDefault(0) == currentUserId);
            if (!isUserAlreadyIncluded)
            {
                action.UserGroups.Add(new UserGroup()
                {
                    Id = currentUserId,
                    IsGroup = false
                });
            }
            return action;
        }

        private WorkflowEventAction ToGenerateAction(XmlGenerateAction generateAction)
        {
            switch (generateAction.GenerateActionType)
            {
                case GenerateActionTypes.Children:
                    if (!generateAction.ArtifactTypeId.HasValue)
                    {
                        return null;
                    }
                    return new GenerateChildrenAction
                    {
                        ArtifactTypeId = generateAction.ArtifactTypeId.Value,
                        ChildCount = generateAction.ChildCount
                    };
                case GenerateActionTypes.UserStories:
                    return new GenerateUserStoriesAction();
                case GenerateActionTypes.TestCases:
                    return new GenerateTestCasesAction();
            }
            return null;
        }

        private async Task<WorkflowState> ChangeStateForArtifactInternal(int userId, int artifactId, int desiredStateId, IDbTransaction transaction = null)
        {
            var param = new DynamicParameters();
            param.Add("@userId", userId);
            param.Add("@artifactId", artifactId);
            param.Add("@desiredStateId", desiredStateId);
            param.Add("@result");

            if (transaction == null)
            {
                return ToWorkflowStates(await ConnectionWrapper.QueryAsync<SqlWorkFlowState>("ChangeStateForArtifact", param, commandType: CommandType.StoredProcedure)).FirstOrDefault();
            }
            return ToWorkflowStates(await transaction.Connection.QueryAsync<SqlWorkFlowState>("ChangeStateForArtifact", param, transaction, commandType: CommandType.StoredProcedure)).FirstOrDefault();
        }

        private IList<WorkflowState> ToWorkflowStates(IEnumerable<SqlWorkFlowState> sqlWorkFlowStates)
        {
            return sqlWorkFlowStates.Select(workflowState => new WorkflowState
            {
                Id = workflowState.WorkflowStateId, Name = workflowState.WorkflowStateName, WorkflowId = workflowState.WorkflowId
            }).ToList();
        }

        private async Task<Dictionary<int, List<WorkflowPropertyType>>> GetCustomPropertyTypesFromStandardIds(
            IEnumerable<int> itemTypeIds, 
            IEnumerable<int> instancePropertyTypeIds, 
            int projectId,
            IDbTransaction transaction = null)
        {
            var param = new DynamicParameters();
            param.Add("@instanceItemTypeIds", SqlConnectionWrapper.ToDataTable(itemTypeIds));
            param.Add("@instancePropertyIds", SqlConnectionWrapper.ToDataTable(instancePropertyTypeIds));
            param.Add("@projectId", projectId);

            const string storedProcedure = "GetCustomPropertyTypesFromStandardIds";
            if (transaction == null)
            {
                return ToItemTypePropertyTypesDictionary(await ConnectionWrapper.QueryAsync<SqlPropertyType>(storedProcedure, param, commandType: CommandType.StoredProcedure));
            }
            return ToItemTypePropertyTypesDictionary(await transaction.Connection.QueryAsync<SqlPropertyType>(storedProcedure, param, transaction, commandType: CommandType.StoredProcedure));
        }

        private Dictionary<int, List<WorkflowPropertyType>> ToItemTypePropertyTypesDictionary(IEnumerable<SqlPropertyType> sqlPropertyTypes)
        {
            var dictionary = new Dictionary<int, List<WorkflowPropertyType>>();
            foreach (var sqlPropertyType in sqlPropertyTypes)
            {
                WorkflowPropertyType workflowProperty;
                switch (sqlPropertyType.PrimitiveType)
                {
                    case PropertyPrimitiveType.Text:
                    {
                        workflowProperty = new TextPropertyType
                        {
                            AllowMultiple = sqlPropertyType.AllowMultiple,
                            DefaultValidValueId = sqlPropertyType.DefaultValidValueId,
                            InstancePropertyTypeId = sqlPropertyType.InstancePropertyTypeId,
                            Name = sqlPropertyType.Name,
                            PropertyTypeId = sqlPropertyType.PropertyTypeId,
                            PrimitiveType = sqlPropertyType.PrimitiveType,
                            IsRequired = sqlPropertyType.Required != null && sqlPropertyType.Required.Value,
                            StringDefaultValue = sqlPropertyType.StringDefaultValue,
                            VersionId = sqlPropertyType.VersionId,
                            Predefined = sqlPropertyType.Predefined,

                            DefaultValue = sqlPropertyType.StringDefaultValue,
                            IsValidate = sqlPropertyType.Validate.GetValueOrDefault(false),
                            IsRichText = sqlPropertyType.IsRichText
                        };
                        break;
                    }
                    case PropertyPrimitiveType.Number:
                    {
                        workflowProperty = new NumberPropertyType
                        {
                            AllowMultiple = sqlPropertyType.AllowMultiple,
                            DefaultValue = PropertyHelper.ToDecimal((byte[])sqlPropertyType.DecimalDefaultValue),
                            DecimalPlaces = sqlPropertyType.DecimalPlaces.GetValueOrDefault(0),
                            DefaultValidValueId = sqlPropertyType.DefaultValidValueId,
                            InstancePropertyTypeId = sqlPropertyType.InstancePropertyTypeId,
                            Name = sqlPropertyType.Name,
                            PropertyTypeId = sqlPropertyType.PropertyTypeId,
                            Range = new Range<decimal>
                            {
                                End = PropertyHelper.ToDecimal((byte[])sqlPropertyType.NumberRange_End).GetValueOrDefault(0),
                                Start = PropertyHelper.ToDecimal((byte[])sqlPropertyType.NumberRange_Start).GetValueOrDefault(0)
                            },
                            PrimitiveType = sqlPropertyType.PrimitiveType,
                            IsRequired = sqlPropertyType.Required != null && sqlPropertyType.Required.Value,
                            IsValidate = sqlPropertyType.Validate.GetValueOrDefault(false),
                            VersionId = sqlPropertyType.VersionId,
                            Predefined = sqlPropertyType.Predefined
                        };
                        break;
                    }
                    case PropertyPrimitiveType.Date:
                    {
                        workflowProperty = new DatePropertyType
                        {
                            AllowMultiple = sqlPropertyType.AllowMultiple,
                            DefaultValue = sqlPropertyType.DateDefaultValue,
                            DefaultValidValueId = sqlPropertyType.DefaultValidValueId,
                            InstancePropertyTypeId = sqlPropertyType.InstancePropertyTypeId,
                            Name = sqlPropertyType.Name,
                            PropertyTypeId = sqlPropertyType.PropertyTypeId,
                            Range = new Range<DateTime?>
                            {
                                End = sqlPropertyType.DateRange_End,
                                Start = sqlPropertyType.DateRange_Start
                            },
                            PrimitiveType = sqlPropertyType.PrimitiveType,
                            IsRequired = sqlPropertyType.Required != null && sqlPropertyType.Required.Value,
                            IsValidate = sqlPropertyType.Validate.GetValueOrDefault(false),
                            VersionId = sqlPropertyType.VersionId,
                            Predefined = sqlPropertyType.Predefined
                        };
                        break;
                    }
                    case PropertyPrimitiveType.User:
                        workflowProperty = new UserPropertyType()
                        {
                            DefaultLabels = sqlPropertyType.UserDefaultLabel,
                            DefaultValues = sqlPropertyType.UserDefaultValue,
                            InstancePropertyTypeId = sqlPropertyType.InstancePropertyTypeId,
                            Name = sqlPropertyType.Name,
                            PropertyTypeId = sqlPropertyType.PropertyTypeId,
                            PrimitiveType = sqlPropertyType.PrimitiveType,
                            IsRequired = sqlPropertyType.Required != null && sqlPropertyType.Required.Value,
                            VersionId = sqlPropertyType.VersionId,
                            Predefined = sqlPropertyType.Predefined
                        };
                        break;
                    case PropertyPrimitiveType.Choice:
                    {
                        workflowProperty = new ChoicePropertyType
                        {
                            AllowMultiple = sqlPropertyType.AllowMultiple,
                            //DefaultValue = PropertyHelper.ToDecimal((byte[])sqlPropertyType.DecimalDefaultValue),
                            ValidValues = XmlModelSerializer.DeserializeCustomProperties(sqlPropertyType.CustomProperty).CustomProperties[0]?.ValidValues
                                    .OrderBy(v => I18NHelper.Int32ParseInvariant(v.OrderIndex))
                                    .Select(v =>
                                    {
                                        int? vvId = null;
                                        if (!string.IsNullOrWhiteSpace(v.LookupListItemId))
                                        {
                                            int intValue;
                                            if (int.TryParse(v.LookupListItemId, out intValue))
                                                vvId = intValue;
                                        }
                                        int? vvSid = null;
                                        if (!string.IsNullOrWhiteSpace(v.StandardLookupListItemId))
                                        {
                                            int intValue;
                                            if (int.TryParse(v.StandardLookupListItemId, out intValue))
                                                vvSid = intValue;
                                        }
                                        return new ValidValue { Id = vvId, Value = v.Value, Sid = vvSid };
                                    }).ToList(),
                            DefaultValidValueId = sqlPropertyType.DefaultValidValueId,
                            InstancePropertyTypeId = sqlPropertyType.InstancePropertyTypeId,
                            Name = sqlPropertyType.Name,
                            PropertyTypeId = sqlPropertyType.PropertyTypeId,
                            PrimitiveType = sqlPropertyType.PrimitiveType,
                            IsRequired = sqlPropertyType.Required != null && sqlPropertyType.Required.Value,
                            IsValidate = sqlPropertyType.Validate.GetValueOrDefault(false),
                            VersionId = sqlPropertyType.VersionId,
                            Predefined = sqlPropertyType.Predefined
                        };
                        break;
                    }                    
                    //TODO: add other DPropertyTypes
                    default:
                        {
                            workflowProperty = new WorkflowPropertyType
                            {
                                AllowMultiple = sqlPropertyType.AllowMultiple,
                                DefaultValidValueId = sqlPropertyType.DefaultValidValueId,
                                InstancePropertyTypeId = sqlPropertyType.InstancePropertyTypeId,
                                IsRichText = sqlPropertyType.IsRichText,
                                Name = sqlPropertyType.Name,
                                PropertyTypeId = sqlPropertyType.PropertyTypeId,
                                PrimitiveType = sqlPropertyType.PrimitiveType,
                                IsRequired = sqlPropertyType.Required != null && sqlPropertyType.Required.Value,
                                StringDefaultValue = sqlPropertyType.StringDefaultValue,
                                VersionId = sqlPropertyType.VersionId,
                                Predefined = sqlPropertyType.Predefined
                            };
                            break;
                    }
                }
               
                if (dictionary.ContainsKey(sqlPropertyType.ItemTypeId))
                {
                    dictionary[sqlPropertyType.ItemTypeId].Add(workflowProperty);
                }
                else
                {
                    dictionary.Add(sqlPropertyType.ItemTypeId, new List<WorkflowPropertyType> { workflowProperty});
                }
            }
            return dictionary;
        }

        private async Task<IEnumerable<WorkflowMessageArtifactInfo>> GetWorkflowMessageArtifactInfoAsyncInternal(int userId,
            IEnumerable<int> artifactIds,
            int revisionId,
            IDbTransaction transaction = null)
        {
            var param = new DynamicParameters();
            param.Add("@userId", userId);
            var artifactIdsTable = SqlConnectionWrapper.ToDataTable(artifactIds);
            param.Add("@artifactIds", artifactIdsTable);
            param.Add("@revisionId", revisionId);
            if (transaction != null)
            {
                return (await transaction.Connection.QueryAsync<WorkflowMessageArtifactInfo>("GetWorkflowMessageArtifactInfo",
                    param,
                    transaction,
                    commandType: CommandType.StoredProcedure)).ToList();
            }
            return (await
                ConnectionWrapper.QueryAsync<WorkflowMessageArtifactInfo>("GetWorkflowMessageArtifactInfo",
                    param,
                    commandType: CommandType.StoredProcedure)).ToList();
        }

        #endregion
    }
}
