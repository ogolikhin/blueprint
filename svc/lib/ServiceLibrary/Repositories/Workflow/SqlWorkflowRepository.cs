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

        public async Task<Dictionary<int, List<DPropertyType>>> GetCustomItemTypeToPropertiesMap(
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
            IEnumerable<int> artifactIds, 
            int revisionId)
        {
            return await GetWorkflowEventTriggersForNewArtifactEventInternal(userId, artifactIds, revisionId);
        }



        #endregion

        #region Private methods

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
                eventTriggers.AddRange(ToWorkflowTriggers(SerializationHelper.FromXml<XmlWorkflowEventTriggers>(n.Triggers)));
            });
            return GetWorkflowTriggersContainer(eventTriggers);
        }

        private static WorkflowTriggersContainer GetWorkflowTriggersContainer(WorkflowEventTriggers eventTriggers)
        {
            var preOpTriggers = new PreopWorkflowEventTriggers();
            var postOpTriggers = new PostopWorkflowEventTriggers();
            foreach (var workflowEventTrigger in eventTriggers.Where(t => t?.Action != null))
            {
                if (workflowEventTrigger.Action is IWorkflowEventSynchronousAction)
                {
                    preOpTriggers.Add(workflowEventTrigger);
                }
                else if (workflowEventTrigger.Action is IWorkflowEventASynchronousAction)
                {
                    postOpTriggers.Add(workflowEventTrigger);
                }
            }
            return new WorkflowTriggersContainer
            {
                SynchronousTriggers = preOpTriggers,
                AsynchronousTriggers = postOpTriggers
            };
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
                            commandType: CommandType.StoredProcedure));
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
                            commandType: CommandType.StoredProcedure)).FirstOrDefault();
        }
        
        private IList<WorkflowTransition> ToWorkflowTransitions(IEnumerable<SqlWorkflowTransition> sqlWorkflowTransitions)
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
                transition.Triggers.AddRange(ToWorkflowTriggers(SerializationHelper.FromXml<XmlWorkflowEventTriggers>(wt.Triggers)));
                return transition;
            }).ToList();
        }

        private WorkflowEventTriggers ToWorkflowTriggers(XmlWorkflowEventTriggers xmlWorkflowEventTriggers)
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
                Action = GenerateAction(xmlWorkflowEventTrigger.Action)
            }));
            return triggers;
        }

        private WorkflowEventAction GenerateAction(XmlAction action)
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
                return ToPropertyChangeAction(propertyChangeAction);
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

        private PropertyChangeAction ToPropertyChangeAction(XmlPropertyChangeAction propertyChangeAction)
        {
            if (propertyChangeAction.UsersGroups.Any())
            {
                var action = new PropertyChangeUserGroupsAction
                {
                    InstancePropertyTypeId = propertyChangeAction.PropertyTypeId,
                    PropertyValue = propertyChangeAction.PropertyValue
                };
                action.UserGroups.AddRange(propertyChangeAction.UsersGroups.Select(
                        u => new ActionUserGroups
                        {
                            Id = u.Id,
                            IsGroup = u.IsGroup
                        }).ToList());
            }
            return new PropertyChangeAction
            {
                InstancePropertyTypeId = propertyChangeAction.PropertyTypeId,
                PropertyValue = propertyChangeAction.PropertyValue
            };
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

        private async Task<Dictionary<int, List<DPropertyType>>> GetCustomPropertyTypesFromStandardIds(
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

        private Dictionary<int, List<DPropertyType>> ToItemTypePropertyTypesDictionary(IEnumerable<SqlPropertyType> sqlPropertyTypes)
        {
            var dictionary = new Dictionary<int, List<DPropertyType>>();
            foreach (var sqlPropertyType in sqlPropertyTypes)
            {
                DPropertyType dProperty;
                switch (sqlPropertyType.PrimitiveType)
                {
                    case PropertyPrimitiveType.Number:
                    {
                        dProperty = new DNumberPropertyType
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
                        dProperty = new DDatePropertyType
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
                    //TODO: add other DPropertyTypes
                    default:
                        {
                            dProperty = new DPropertyType
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
                    dictionary[sqlPropertyType.ItemTypeId].Add(dProperty);
                }
                else
                {
                    dictionary.Add(sqlPropertyType.ItemTypeId, new List<DPropertyType> { dProperty});
                }
            }
            return dictionary;
        }
        #endregion
    }
}
