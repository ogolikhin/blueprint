using AdminStore.Helpers;
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

        public async Task<IEnumerable<SqlState>> GetWorkflowStatesByWorkflowId(int workflowId)
        {
            var parameters = new DynamicParameters();
            parameters.Add("WorkflowId", workflowId);
            var result = await _connectionWrapper.QueryAsync<SqlState>("GetWorkflowStatesById", parameters, commandType: CommandType.StoredProcedure);

            return result;
        }

        public async Task<IEnumerable<SqlWorkflowTransitionsAndPropertyChanges>> GetWorkflowTransitionsAndPropertyChangesByWorkflowId(int workflowId)
        {
            var parameters = new DynamicParameters();
            parameters.Add("WorkflowId", workflowId);
            var result = await _connectionWrapper.QueryAsync<SqlWorkflowTransitionsAndPropertyChanges>("GetWorkflowTransitionsAndPropertyChangesById", parameters, commandType: CommandType.StoredProcedure);

            return result;
        }

        public async Task UpdateWorkflowsChangedWithRevisions(int workflowId, int revisionId, IDbTransaction transaction = null)
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

            var connection = transaction == null ? (IDbConnection) _connectionWrapper: transaction.Connection;
            await connection.ExecuteAsync("UpdateWorkflowsChangedWithRevisions", parameters,
                transaction, commandType: CommandType.StoredProcedure);
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

        public async Task CreateWorkflowArtifactAssociationsAsync(IEnumerable<string> artifactTypeNames,
            IEnumerable<int> projectIds, int workflowId, int publishRevision, IDbTransaction transaction = null)
        {

            var dArtifactTypeNames = artifactTypeNames.ToList();
            if (!dArtifactTypeNames.Any())
            {
                throw new ArgumentException(I18NHelper.FormatInvariant("{0} is empty.", nameof(dArtifactTypeNames)));
            }

            var dProjectIds = projectIds.ToList();
            if (!dProjectIds.Any())
            {
                throw new ArgumentException(I18NHelper.FormatInvariant("{0} is empty.", nameof(dProjectIds)));
            }

            if (publishRevision < 1)
            {
                throw new ArgumentException(I18NHelper.FormatInvariant("{0} is less than 1.", nameof(publishRevision)));
            }

            var prm = new DynamicParameters();
            prm.Add("@names", SqlConnectionWrapper.ToStringDataTable(dArtifactTypeNames));
            prm.Add("@revisionId", publishRevision);
            prm.Add("@workflowId", workflowId);
            prm.Add("@projectIds", SqlConnectionWrapper.ToDataTable(dProjectIds));

            if (transaction == null)
            {
                await _connectionWrapper.ExecuteAsync("UpdateItemTypeVersionsWithWorkflowId", prm,
                    commandType: CommandType.StoredProcedure);
            }
            else
            {
                await transaction.Connection.ExecuteAsync("UpdateItemTypeVersionsWithWorkflowId", prm,
                    transaction, commandType: CommandType.StoredProcedure);
            }
        }

        public async Task<IEnumerable<SqlProjectPathPair>> GetProjectIdsByProjectPaths(IEnumerable<string> projectPaths)
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

        public async Task<IEnumerable<SqlArtifactTypesWorkflowDetails>> GetExistingStandardArtifactTypesForWorkflows(IEnumerable<string> artifactTypes, IEnumerable<int> projectIds)
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

        public async Task<IEnumerable<int>> GetExistingProjectsByIds(IEnumerable<int> projectIds)
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

        public async Task<IEnumerable<string>> CheckLiveWorkflowsForNameUniqueness(IEnumerable<string> names)
        {
            var prm = new DynamicParameters();
            prm.Add("@names", SqlConnectionWrapper.ToStringDataTable(names));
            var duplicateNames = await _connectionWrapper.QueryAsync<string>("CheckLiveWorkflowsForNameUniqueness", prm, commandType: CommandType.StoredProcedure);
            return duplicateNames;
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

            return new QueryResult<WorkflowDto>() {Items = workflows, Total = total};
        }

        public async Task<SqlWorkflow> GetWorkflowDetailsAsync(int workflowId)
        {
            var parameters = new DynamicParameters();
            parameters.Add("WorkflowId", workflowId);

            var result = (await _connectionWrapper.QueryAsync<SqlWorkflow>("GetWorkflowDetails", parameters, commandType: CommandType.StoredProcedure)).FirstOrDefault();

            return result;           
        }

        public async Task<IEnumerable<SqlWorkflowArtifactTypesAndProjects>> GetWorkflowArtifactTypesAndProjectsAsync(int workflowId)
        {
            var parameters = new DynamicParameters();
            parameters.Add("WorkflowId", workflowId);

            var result = await _connectionWrapper.QueryAsync<SqlWorkflowArtifactTypesAndProjects>("GetWorkflowProjectsAndArtifactTypes", parameters, commandType: CommandType.StoredProcedure);

            return result;
        }

        public async Task<int> DeleteWorkflows(OperationScope body, string search, int revision, IDbTransaction transaction = null)
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

        public async Task<IEnumerable<SqlWorkflow>> UpdateWorkflows(IEnumerable<SqlWorkflow> workflows, int revision, IDbTransaction transaction = null)
        {
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
            }
            else
            {
                updatedWorkflows =
                    await
                        _connectionWrapper.QueryAsync<SqlWorkflow>("UpdateWorkflows", prm, 
                            commandType: CommandType.StoredProcedure);
            }

            return updatedWorkflows;
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
                table.Rows.Add(workfloEvent.TriggerId, workfloEvent.Name,
                    workfloEvent.WorkflowId, workfloEvent.Type, workfloEvent.Permissions, 
                    workfloEvent.Validations, workfloEvent.Triggers, workfloEvent.WorkflowState1Id, 
                    workfloEvent.WorkflowState2Id, workfloEvent.PropertyTypeId);
            }

            return table;
        }

        #endregion
    }
}
