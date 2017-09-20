﻿using AdminStore.Helpers;
using AdminStore.Models;
using AdminStore.Models.Workflow;
using Dapper;
using ServiceLibrary.Exceptions;
using ServiceLibrary.Helpers;
using ServiceLibrary.Models;
using ServiceLibrary.Repositories;
using ServiceLibrary.Repositories.Files;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace AdminStore.Repositories.Workflow
{
    public class WorkflowRepository : IWorkflowRepository
    {
        private readonly ISqlConnectionWrapper _connectionWrapper;
        private readonly ISqlHelper _sqlHelper;

        public IFileRepository FileRepository { get; set; }

        #region Constructors

        public WorkflowRepository()
            : this(new SqlConnectionWrapper(ServiceConstants.RaptorMain),
                  new SqlHelper())
        {
        }

        internal WorkflowRepository(ISqlConnectionWrapper connectionWrapper,
            ISqlHelper sqlHelper)
        {
            _connectionWrapper = connectionWrapper;
            _sqlHelper = sqlHelper;
        }

        #endregion

        #region Interface implementation

        public async Task<IEnumerable<SqlWorkflow>> CreateWorkflowsAsync(IEnumerable<SqlWorkflow> workflows, int publishRevision, IDbTransaction transaction = null)
        {
            if (workflows == null)
            {
                throw new ArgumentNullException(nameof(workflows));
            }

            var dWorkflows = workflows.ToList();
            if (!dWorkflows.Any())
            {
                throw new ArgumentException(I18NHelper.FormatInvariant("{0} is empty.", nameof(workflows)));
            }

            if (publishRevision < 1)
            {
                throw new ArgumentException(I18NHelper.FormatInvariant("{0} is less than 1.", nameof(publishRevision)));
            }

            var prm = new DynamicParameters();
            prm.Add("@publishRevision", publishRevision);
            prm.Add("@workflows", ToWorkflowsCollectionDataTable(dWorkflows));

            IEnumerable<SqlWorkflow> result;
            if (transaction == null)
            {
                result = await _connectionWrapper.QueryAsync<SqlWorkflow>("CreateWorkflows", prm,
                    commandType: CommandType.StoredProcedure);
            }
            else
            {
                result = await transaction.Connection.QueryAsync<SqlWorkflow>("CreateWorkflows", prm,
                    transaction, commandType: CommandType.StoredProcedure);
            }

            return result;
        }

        public async Task<IEnumerable<SqlState>> CreateWorkflowStatesAsync(IEnumerable<SqlState> workflowStates, int publishRevision, IDbTransaction transaction = null)
        {
            if (workflowStates == null)
            {
                throw new ArgumentNullException(nameof(workflowStates));
            }

            var dWorkflowStates = workflowStates.ToList();
            if (!dWorkflowStates.Any())
            {
                throw new ArgumentException(I18NHelper.FormatInvariant("{0} is empty.", nameof(workflowStates)));
            }

            if (publishRevision < 1)
            {
                throw new ArgumentException(I18NHelper.FormatInvariant("{0} is less than 1.", nameof(publishRevision)));
            }

            var prm = new DynamicParameters();
            prm.Add("@publishRevision", publishRevision);
            prm.Add("@workflowStates", ToWorkflowStatesCollectionDataTable(dWorkflowStates));

            IEnumerable<SqlState> result;
            if (transaction == null)
            {
                result = await _connectionWrapper.QueryAsync<SqlState>("CreateWorkflowStates", prm,
                    commandType: CommandType.StoredProcedure);
            }
            else
            {
                result = await transaction.Connection.QueryAsync<SqlState>("CreateWorkflowStates", prm,
                    transaction, commandType: CommandType.StoredProcedure);
            }

            return result;
        }

        public async Task<IEnumerable<SqlState>> UpdateWorkflowStatesAsync(IEnumerable<SqlState> workflowStates, int publishRevision, IDbTransaction transaction = null)
        {
            if (workflowStates == null)
            {
                throw new ArgumentNullException(nameof(workflowStates));
            }

            var dWorkflowStates = workflowStates.ToList();
            if (!dWorkflowStates.Any())
            {
                throw new ArgumentException(I18NHelper.FormatInvariant("{0} is empty.", nameof(workflowStates)));
            }

            if (publishRevision < 1)
            {
                throw new ArgumentException(I18NHelper.FormatInvariant("{0} is less than 1.", nameof(publishRevision)));
            }

            var prm = new DynamicParameters();
            prm.Add("@publishRevision", publishRevision);
            prm.Add("@workflowStates", ToWorkflowStatesCollectionDataTable(dWorkflowStates));

            var connection = transaction == null ? (IDbConnection)_connectionWrapper : transaction.Connection;
            var result = await connection.QueryAsync<SqlState>("UpdateWorkflowStates", prm, transaction,
                    commandType: CommandType.StoredProcedure);

            return result;
        }

        public async Task<IEnumerable<int>> DeleteWorkflowStatesAsync(IEnumerable<int> workflowStateIds, int publishRevision, IDbTransaction transaction = null)
        {
            if (workflowStateIds == null)
            {
                throw new ArgumentNullException(nameof(workflowStateIds));
            }

            var listWorkflowStateIds = workflowStateIds.ToList();
            if (!listWorkflowStateIds.Any())
            {
                throw new ArgumentException(I18NHelper.FormatInvariant("{0} is empty.", nameof(workflowStateIds)));
            }

            if (publishRevision < 1)
            {
                throw new ArgumentException(I18NHelper.FormatInvariant("{0} is less than 1.", nameof(publishRevision)));
            }

            var prm = new DynamicParameters();
            prm.Add("@publishRevision", publishRevision);
            prm.Add("@workflowStateIds", SqlConnectionWrapper.ToDataTable(listWorkflowStateIds));

            var connection = transaction == null ? (IDbConnection)_connectionWrapper : transaction.Connection;
            var result = await connection.QueryAsync<SqlState>("DeleteWorkflowStates", prm, transaction,
                    commandType: CommandType.StoredProcedure);

            return result.Select(s => s.WorkflowStateId);
        }

        public async Task<IEnumerable<SqlState>> GetWorkflowStatesAsync(int workflowId)
        {
            var parameters = new DynamicParameters();
            parameters.Add("WorkflowId", workflowId);
            var result = await _connectionWrapper.QueryAsync<SqlState>("GetWorkflowStatesById", parameters, commandType: CommandType.StoredProcedure);

            return result;
        }

        public async Task<IEnumerable<SqlWorkflowEventData>> GetWorkflowEventsAsync(int workflowId)
        {
            var parameters = new DynamicParameters();
            parameters.Add("WorkflowId", workflowId);
            var result = await _connectionWrapper.QueryAsync<SqlWorkflowEventData>("GetWorkflowEventsById", parameters, commandType: CommandType.StoredProcedure);

            return result;
        }
        public async Task UpdateWorkflowsChangedWithRevisionsAsync(int workflowId, int revisionId, IDbTransaction transaction = null)
        {
            if (workflowId < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(workflowId));
            }

            if (revisionId <= 1)
            {
                throw new ArgumentOutOfRangeException(nameof(revisionId));
            }

            var parameters = new DynamicParameters();
            parameters.Add("@workflowId", workflowId);
            parameters.Add("@revisionId", revisionId);

            var connection = transaction == null ? (IDbConnection)_connectionWrapper : transaction.Connection;
            await connection.ExecuteAsync("UpdateWorkflowsChangedWithRevisions", parameters,
                transaction, commandType: CommandType.StoredProcedure);
        }

        public async Task<int> CreateWorkflow(SqlWorkflow workflow, int revision, IDbTransaction transaction)
        {
            var parameters = new DynamicParameters();
            parameters.Add("@Name", workflow.Name);
            parameters.Add("@Description", workflow.Description);
            parameters.Add("@RevisionId", revision);
            parameters.Add("@ErrorCode", dbType: DbType.Int32, direction: ParameterDirection.Output);

            var connection = transaction.Connection;
            var result = await connection.ExecuteScalarAsync<int>("CreateWorkflow", parameters, transaction, commandType: CommandType.StoredProcedure);
            var errorCode = parameters.Get<int?>("ErrorCode");

            if (errorCode.HasValue)
            {
                switch (errorCode.Value)
                {
                    case (int)SqlErrorCodes.WorkflowWithSuchANameAlreadyExists:
                        throw new ConflictException(ErrorMessages.WorkflowAlreadyExists, ErrorCodes.WorkflowAlreadyExists);
                    case (int)SqlErrorCodes.GeneralSqlError:
                        throw new Exception(ErrorMessages.GeneralErrorOfCreatingUser);
                }
            }
            return result;
        }

        public async Task<IEnumerable<SqlWorkflowEvent>> CreateWorkflowEventsAsync(IEnumerable<SqlWorkflowEvent> workflowEvents, int publishRevision, IDbTransaction transaction = null)
        {
            if (workflowEvents == null)
            {
                throw new ArgumentNullException(nameof(workflowEvents));
            }

            var dWorkflowEvents = workflowEvents.ToList();
            if (!dWorkflowEvents.Any())
            {
                throw new ArgumentException(I18NHelper.FormatInvariant("{0} is empty.", nameof(dWorkflowEvents)));
            }

            if (publishRevision < 1)
            {
                throw new ArgumentException(I18NHelper.FormatInvariant("{0} is less than 1.", nameof(publishRevision)));
            }

            var prm = new DynamicParameters();
            prm.Add("@publishRevision", publishRevision);
            prm.Add("@workflowEvents", ToWorkflowEventsCollectionDataTable(dWorkflowEvents));

            IEnumerable<SqlWorkflowEvent> result;
            if (transaction == null)
            {
                result = await _connectionWrapper.QueryAsync<SqlWorkflowEvent>("CreateWorkflowEvents", prm,
                    commandType: CommandType.StoredProcedure);
            }
            else
            {
                result = await transaction.Connection.QueryAsync<SqlWorkflowEvent>("CreateWorkflowEvents", prm,
                    transaction, commandType: CommandType.StoredProcedure);
            }

            return result;
        }

        public async Task<IEnumerable<SqlWorkflowEvent>> UpdateWorkflowEventsAsync(IEnumerable<SqlWorkflowEvent> workflowEvents, int publishRevision, IDbTransaction transaction = null)
        {
            if (workflowEvents == null)
            {
                throw new ArgumentNullException(nameof(workflowEvents));
            }

            var dWorkflowEvents = workflowEvents.ToList();
            if (!dWorkflowEvents.Any())
            {
                throw new ArgumentException(I18NHelper.FormatInvariant("{0} is empty.", nameof(dWorkflowEvents)));
            }

            if (publishRevision < 1)
            {
                throw new ArgumentException(I18NHelper.FormatInvariant("{0} is less than 1.", nameof(publishRevision)));
            }

            var prm = new DynamicParameters();
            prm.Add("@publishRevision", publishRevision);
            prm.Add("@workflowEvents", ToWorkflowEventsCollectionDataTable(dWorkflowEvents));

            var connection = transaction == null ? (IDbConnection)_connectionWrapper : transaction.Connection;
            var result = await connection.QueryAsync<SqlWorkflowEvent>("UpdateWorkflowEvents", prm,
                    transaction, commandType: CommandType.StoredProcedure);

            return result;
        }

        public async Task<IEnumerable<int>> DeleteWorkflowEventsAsync(IEnumerable<int> workflowEventIds, int publishRevision, IDbTransaction transaction = null)
        {
            if (workflowEventIds == null)
            {
                throw new ArgumentNullException(nameof(workflowEventIds));
            }

            var listWorkflowStateIds = workflowEventIds.ToList();
            if (!listWorkflowStateIds.Any())
            {
                throw new ArgumentException(I18NHelper.FormatInvariant("{0} is empty.", nameof(workflowEventIds)));
            }

            if (publishRevision < 1)
            {
                throw new ArgumentException(I18NHelper.FormatInvariant("{0} is less than 1.", nameof(publishRevision)));
            }

            var prm = new DynamicParameters();
            prm.Add("@publishRevision", publishRevision);
            prm.Add("@workflowEventIds", SqlConnectionWrapper.ToDataTable(listWorkflowStateIds));

            var connection = transaction == null ? (IDbConnection)_connectionWrapper : transaction.Connection;
            var result = await connection.QueryAsync<SqlWorkflowEvent>("DeleteWorkflowEvents", prm,
                    transaction, commandType: CommandType.StoredProcedure);

            return result.Select(s => s.WorkflowEventId);
        }

        public async Task CreateWorkflowArtifactAssociationsAsync(IEnumerable<KeyValuePair<int, string>> projectArtifactTypePair,
            int workflowId, int publishRevision, IDbTransaction transaction = null)
        {
            await UpdateWorkflowArtifactAssociationsAsync(projectArtifactTypePair, workflowId, publishRevision, transaction);
        }

        public async Task DeleteWorkflowArtifactAssociationsAsync(IEnumerable<KeyValuePair<int, string>> projectArtifactTypePair,
            int publishRevision, IDbTransaction transaction = null)
        {
            await UpdateWorkflowArtifactAssociationsAsync(projectArtifactTypePair, null, publishRevision, transaction);
        }

        private async Task UpdateWorkflowArtifactAssociationsAsync(IEnumerable<KeyValuePair<int, string>> projectArtifactTypePair,
            int? workflowId, int publishRevision, IDbTransaction transaction = null)
        {

            var projectArtifactTypePairList = projectArtifactTypePair.ToList();
            if (!projectArtifactTypePairList.Any())
            {
                throw new ArgumentException(I18NHelper.FormatInvariant("{0} is empty.", nameof(projectArtifactTypePair)));
            }

            if (publishRevision < 1)
            {
                throw new ArgumentException(I18NHelper.FormatInvariant("{0} is less than 1.", nameof(publishRevision)));
            }

            var prm = new DynamicParameters();
            prm.Add("@projectArtifactTypePairs", SqlConnectionWrapper.ToIdStringMapDataTable(projectArtifactTypePairList));
            prm.Add("@revisionId", publishRevision);
            prm.Add("@workflowId", workflowId);

            var connection = transaction == null ? (IDbConnection)_connectionWrapper : transaction.Connection;
            await connection.ExecuteAsync("UpdateItemTypeVersionsWithWorkflowId", prm,
                transaction, commandType: CommandType.StoredProcedure);
        }

        public async Task<IEnumerable<SqlProjectPathPair>> GetProjectIdsByProjectPathsAsync(IEnumerable<string> projectPaths)
        {
            var dProjectPaths = projectPaths.ToList();
            if (!dProjectPaths.Any())
            {
                throw new ArgumentException(I18NHelper.FormatInvariant("{0} is empty.", nameof(dProjectPaths)));
            }

            var prm = new DynamicParameters();
            prm.Add("@projectPaths", SqlConnectionWrapper.ToStringDataTable(dProjectPaths));

            return await _connectionWrapper.QueryAsync<SqlProjectPathPair>("GetProjectIdsByProjectPaths", prm,
                commandType: CommandType.StoredProcedure);
        }

        public async Task<IEnumerable<SqlArtifactTypesWorkflowDetails>> GetExistingStandardArtifactTypesForWorkflowsAsync(IEnumerable<string> artifactTypes, IEnumerable<int> projectIds)
        {
            var dArtifactTypes = artifactTypes.ToList();
            if (!dArtifactTypes.Any())
            {
                throw new ArgumentException(I18NHelper.FormatInvariant("{0} is empty.", nameof(dArtifactTypes)));
            }

            var dProjectIds = projectIds.ToList();
            if (!dProjectIds.Any())
            {
                throw new ArgumentException(I18NHelper.FormatInvariant("{0} is empty.", nameof(dProjectIds)));
            }

            var prm = new DynamicParameters();
            prm.Add("@artifactTypes", SqlConnectionWrapper.ToStringDataTable(dArtifactTypes));
            prm.Add("@projectIds", SqlConnectionWrapper.ToDataTable(dProjectIds));

            return await _connectionWrapper.QueryAsync<SqlArtifactTypesWorkflowDetails>("GetExistingStandardArtifactTypesForWorkflows", prm,
                commandType: CommandType.StoredProcedure);
        }

        public async Task<IEnumerable<string>> GetExistingPropertyTypesByName(IEnumerable<string> propertyTypeNames)
        {
            var dPropertyTypeNames = propertyTypeNames.ToList();
            if (!dPropertyTypeNames.Any())
            {
                throw new ArgumentException(I18NHelper.FormatInvariant("{0} is empty.", nameof(dPropertyTypeNames)));
            }

            var prm = new DynamicParameters();
            prm.Add("@propertyTypeNames", SqlConnectionWrapper.ToStringDataTable(dPropertyTypeNames));

            return await _connectionWrapper.QueryAsync<string>("GetExistingPropertyTypesByName", prm,
                commandType: CommandType.StoredProcedure);
        }

        public async Task<IEnumerable<int>> GetExistingProjectsByIdsAsync(IEnumerable<int> projectIds)
        {
            var dProjectIds = projectIds.ToList();
            if (!dProjectIds.Any())
            {
                throw new ArgumentException(I18NHelper.FormatInvariant("{0} is empty.", nameof(dProjectIds)));
            }

            var prm = new DynamicParameters();
            prm.Add("@projectIds", SqlConnectionWrapper.ToDataTable(dProjectIds));

            return await _connectionWrapper.QueryAsync<int>("GetExistingProjectsByIds", prm,
                commandType: CommandType.StoredProcedure);
        }

        public Task<int> CreateRevisionInTransactionAsync(IDbTransaction transaction, int userId, string description)
        {
            return _sqlHelper.CreateRevisionInTransactionAsync(transaction, userId, description);
        }

        public async Task<IEnumerable<string>> CheckLiveWorkflowsForNameUniquenessAsync(IEnumerable<string> names, int? exceptWorkflowId = null)
        {
            var prm = new DynamicParameters();
            prm.Add("@names", SqlConnectionWrapper.ToStringDataTable(names));
            prm.Add("@exceptWorkflowId", exceptWorkflowId);
            var duplicateNames = await _connectionWrapper.QueryAsync<string>("CheckLiveWorkflowsForNameUniqueness", prm, commandType: CommandType.StoredProcedure);
            return duplicateNames;
        }

        public async Task<QueryResult<InstanceItem>> GetWorkflowAvailableProjectsAsync(int workflowId, int folderId, string search)
        {
            if (workflowId < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(workflowId));
            }

            if (folderId < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(folderId));
            }

            var prm = new DynamicParameters();
            prm.Add("@workflowId", workflowId);
            prm.Add("@folderId", folderId);
            prm.Add("@search", search);
            prm.Add("@ErrorCode", dbType: DbType.Int32, direction: ParameterDirection.Output);

            var items = ((await _connectionWrapper.QueryAsync<InstanceItem>("GetWorkflowAvailableProjects", prm, commandType: CommandType.StoredProcedure))
                    ?? null).OrderBy(i => i.Type).ThenBy(i => i.Name);

            var errorCode = prm.Get<int?>("ErrorCode");

            if (errorCode.HasValue)
            {
                switch (errorCode.Value)
                {
                    case (int)SqlErrorCodes.FolderWithCurrentIdNotExist:
                        throw new ResourceNotFoundException(ErrorMessages.FolderNotExist, ErrorCodes.ResourceNotFound);
                    case (int)SqlErrorCodes.WorkflowWithCurrentIdNotExist:
                        throw new ResourceNotFoundException(ErrorMessages.WorkflowNotExist, ErrorCodes.ResourceNotFound);
                }
            }

            return new QueryResult<InstanceItem>() { Items = items };
        }

        public async Task RunInTransactionAsync(Func<IDbTransaction, Task> action)
        {
            await _sqlHelper.RunInTransactionAsync(ServiceConstants.RaptorMain, action);
        }

        public async Task<QueryResult<WorkflowDto>> GetWorkflows(Pagination pagination, Sorting sorting = null,
            string search = null, Func<Sorting, string> sort = null)
        {
            var orderField = string.Empty;
            if (sort != null && sorting != null)
            {
                orderField = sort(sorting);
            }
            if (search != null)
            {
                search = UsersHelper.ReplaceWildcardCharacters(search);
            }
            var parameters = new DynamicParameters();
            parameters.Add("@Offset", pagination.Offset);
            parameters.Add("@Limit", pagination.Limit);
            parameters.Add("@Search", search ?? string.Empty);
            parameters.Add("@OrderField", string.IsNullOrEmpty(orderField) ? "Name" : orderField);
            parameters.Add("@Total", dbType: DbType.Int32, direction: ParameterDirection.Output);
            var workflows =
                (await
                    _connectionWrapper.QueryAsync<WorkflowDto>("GetAllWorkflows", parameters,
                        commandType: CommandType.StoredProcedure)).ToList();
            var total = parameters.Get<int>("Total");

            return new QueryResult<WorkflowDto>() { Items = workflows, Total = total };
        }

        public async Task<SqlWorkflow> GetWorkflowDetailsAsync(int workflowId)
        {
            var parameters = new DynamicParameters();
            parameters.Add("WorkflowId", workflowId);

            var result = (await _connectionWrapper.QueryAsync<SqlWorkflow>("GetWorkflowDetails", parameters, commandType: CommandType.StoredProcedure)).FirstOrDefault();

            return result;
        }

        public async Task<IEnumerable<SqlWorkflowArtifactTypes>> GetWorkflowArtifactTypesAsync(int workflowId)
        {
            var parameters = new DynamicParameters();
            parameters.Add("WorkflowId", workflowId);

            var result = await _connectionWrapper.QueryAsync<SqlWorkflowArtifactTypes>("GetWorkflowArtifactTypes", parameters, commandType: CommandType.StoredProcedure);

            return result;
        }

        public async Task<int> DeleteWorkflowsAsync(OperationScope body, string search, int revision, IDbTransaction transaction = null)
        {
            if (search != null)
            {
                search = UsersHelper.ReplaceWildcardCharacters(search);
            }
            var result = 0;

            var parameters = new DynamicParameters();
            parameters.Add("@PublishRevision", revision);
            parameters.Add("@WorkflowIds", SqlConnectionWrapper.ToDataTable(body.Ids));
            parameters.Add("@Search", search);
            parameters.Add("@SelectAll", body.SelectAll);
            parameters.Add("@ErrorCode", dbType: DbType.Int32, direction: ParameterDirection.Output);
            if (transaction != null)
            {
                result =
                    await
                        _connectionWrapper.ExecuteScalarAsync<int>("DeleteWorkflows", parameters, transaction,
                            commandType: CommandType.StoredProcedure);
            }
            else
            {
                result =
                    await
                        _connectionWrapper.ExecuteScalarAsync<int>("DeleteWorkflows", parameters,
                            commandType: CommandType.StoredProcedure);
            }

            var errorCode = parameters.Get<int?>("ErrorCode");
            if (errorCode.HasValue)
            {
                switch (errorCode.Value)
                {
                    case (int)SqlErrorCodes.GeneralSqlError:
                        throw new BadRequestException(ErrorMessages.GeneralErrorOfDeletingWorkflows);
                }
            }

            return result;
        }

        public async Task<int> UpdateWorkflowsAsync(IEnumerable<SqlWorkflow> workflows, int revision, IDbTransaction transaction = null)
        {
            var versionId = 0;

            var prm = new DynamicParameters();
            prm.Add("@publishRevision", revision);
            prm.Add("@workflows", ToWorkflowsCollectionDataTable(workflows));

            IEnumerable<SqlWorkflow> updatedWorkflows;

            if (transaction != null)
            {
                updatedWorkflows =
                    await
                        transaction.Connection.QueryAsync<SqlWorkflow>("UpdateWorkflows", prm, transaction,
                            commandType: CommandType.StoredProcedure);
                var sqlWorkflows = updatedWorkflows as IList<SqlWorkflow> ?? updatedWorkflows.ToList();
                if (sqlWorkflows.Any())
                {
                    versionId = sqlWorkflows.First().VersionId;
                }
            }
            else
            {
                updatedWorkflows =
                    await
                        _connectionWrapper.QueryAsync<SqlWorkflow>("UpdateWorkflows", prm,
                            commandType: CommandType.StoredProcedure);
                var sqlWorkflows = updatedWorkflows as IList<SqlWorkflow> ?? updatedWorkflows.ToList();
                if (sqlWorkflows.Any())
                {
                    versionId = sqlWorkflows.First().VersionId;
                }
            }

            return versionId;
        }

        #endregion

        #region Private methods

        private static DataTable ToWorkflowsCollectionDataTable(IEnumerable<SqlWorkflow> workflows)
        {
            var table = new DataTable { Locale = CultureInfo.InvariantCulture };
            table.SetTypeName("WorkflowsCollection");
            table.Columns.Add("WorkflowId", typeof(int));
            table.Columns.Add("Name", typeof(string));
            table.Columns.Add("Description", typeof(string));
            table.Columns.Add("Active", typeof(bool));

            foreach (var workflow in workflows)
            {
                table.Rows.Add(workflow.WorkflowId, workflow.Name, workflow.Description, workflow.Active);
            }

            return table;
        }

        private static DataTable ToWorkflowStatesCollectionDataTable(IEnumerable<SqlState> workflowStates)
        {
            var table = new DataTable { Locale = CultureInfo.InvariantCulture };
            table.SetTypeName("WorkflowStatesCollection");
            table.Columns.Add("WorkflowStateId", typeof(int));
            table.Columns.Add("Name", typeof(string));
            table.Columns.Add("WorkflowId", typeof(int));
            table.Columns.Add("Default", typeof(bool));
            table.Columns.Add("OrderIndex", typeof(float));

            foreach (var workflowState in workflowStates)
            {
                table.Rows.Add(workflowState.WorkflowStateId, workflowState.Name,
                    workflowState.WorkflowId, workflowState.Default, workflowState.OrderIndex);
            }

            return table;
        }

        private static DataTable ToWorkflowEventsCollectionDataTable(IEnumerable<SqlWorkflowEvent> workflowEvents)
        {
            var table = new DataTable { Locale = CultureInfo.InvariantCulture };
            table.SetTypeName("WorkflowEventsCollection");
            table.Columns.Add("WorkflowEventId", typeof(int));
            table.Columns.Add("Name", typeof(string));
            table.Columns.Add("WorkflowId", typeof(int));
            table.Columns.Add("Type", typeof(int));
            table.Columns.Add("Permissions", typeof(string));
            table.Columns.Add("Validations", typeof(string));
            table.Columns.Add("Triggers", typeof(string));
            table.Columns.Add("WorkflowState1Id", typeof(int));
            table.Columns.Add("WorkflowState2Id", typeof(int));
            table.Columns.Add("PropertyTypeId", typeof(int));

            foreach (var workfloEvent in workflowEvents)
            {
                table.Rows.Add(workfloEvent.WorkflowEventId, workfloEvent.Name,
                    workfloEvent.WorkflowId, workfloEvent.Type, workfloEvent.Permissions,
                    workfloEvent.Validations, workfloEvent.Triggers, workfloEvent.WorkflowState1Id,
                    workfloEvent.WorkflowState2Id, workfloEvent.PropertyTypeId);
            }

            return table;
        }

        #endregion
    }
}
