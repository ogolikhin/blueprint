using Dapper;
using ServiceLibrary.Helpers;
using ServiceLibrary.Models;
using ServiceLibrary.Models.Workflow;
using ServiceLibrary.Repositories;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using ArtifactStore.Helpers;
using ServiceLibrary.Models.Enums;

namespace ArtifactStore.Repositories.Workflow
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

        public async Task<Dictionary<int, CustomProperties>> GetCustomPropertiesForInstancePropertyIdsAsync(
            int userId,
            int artifactId,
            int projectId, 
            IEnumerable<int> instancePropertyIds)
        {
            //Need to access code for artifact permissions for revision
            await CheckForArtifactPermissions(userId, artifactId, permissions: RolePermissions.Edit);

            return await GetCustomPropertiesForInstancePropertyIds(
                instancePropertyIds,
                projectId);
        }
        #endregion

        #region Private methods

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
                    return new GenerateChildrenAction
                    {
                        ArtifactTypeId = generateAction.ArtifactTypeId,
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

        private async Task<Dictionary<int, CustomProperties>> GetCustomPropertiesForInstancePropertyIds(IEnumerable<int> instancePropertyTypeIds, int projectId, IDbTransaction transaction = null)
        {
            var param = new DynamicParameters();
            var instancePropertyTypeIdsTable = SqlConnectionWrapper.ToDataTable(instancePropertyTypeIds);
            param.Add("@projectId", projectId);
            param.Add("@instancePropertyIds", instancePropertyTypeIdsTable);

            const string storedProcedure = "GetCustomPropertiesForInstancePropertyIds";
            if (transaction == null)
            {
                return (await ConnectionWrapper.QueryAsync<CustomProperties>(storedProcedure, param, commandType: CommandType.StoredProcedure)).ToDictionary(c => c.InstancePropertyTypeId);
            }
            return (await transaction.Connection.QueryAsync<CustomProperties>(storedProcedure, param, transaction, commandType: CommandType.StoredProcedure)).ToDictionary(c => c.InstancePropertyTypeId);

        }
        #endregion
    }
}
