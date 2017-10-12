using AdminStore.Helpers;
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
        public async Task<int> AssignProjectsAndArtifactTypesToWorkflow(int workflowId, WorkflowAssignScope scope)
        {
            var parameters = new DynamicParameters();
            parameters.Add("@WorkflowId", workflowId);
            parameters.Add("@AllArtifactTypes", scope.AllArtifacts, dbType: DbType.Boolean);
            parameters.Add("@AllProjects", scope.AllProjects, dbType: DbType.Boolean);
            parameters.Add("@ArtifactTypesIds", SqlConnectionWrapper.ToDataTable(scope.ArtifactIds, "Int32Collection", "Int32Value"));
            parameters.Add("@ProjectIds", SqlConnectionWrapper.ToDataTable(scope.ProjectIds, "Int32Collection", "Int32Value"));
            parameters.Add("@ErrorCode", dbType: DbType.Int32, direction: ParameterDirection.Output);

            var result = await _connectionWrapper.ExecuteScalarAsync<int>("AssignProjectsAndArtifactTypesToWorkflow", parameters, commandType: CommandType.StoredProcedure);
            var errorCode = parameters.Get<int?>("ErrorCode");

            if (errorCode.HasValue)
            {
                switch (errorCode.Value)
                {
                    case (int)SqlErrorCodes.GeneralSqlError:
                        throw new Exception(ErrorMessages.GeneralErrorOfAssignProjectsAndArtifactTypesToWorkflow);

                    case (int)SqlErrorCodes.WorkflowWithCurrentIdNotExist:
                        throw new ResourceNotFoundException(ErrorMessages.WorkflowNotExist, ErrorCodes.ResourceNotFound);

                    case (int)SqlErrorCodes.WorkflowWithCurrentIdIsActive:
                        throw new ConflictException(ErrorMessages.WorkflowIsActive, ErrorCodes.WorkflowIsActive);                    
                }
            }

            return result;
        }


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

            var parameters = new DynamicParameters();
            parameters.Add("@publishRevision", publishRevision);
            parameters.Add("@workflows", ToWorkflowsCollectionDataTable(dWorkflows));

            IEnumerable<SqlWorkflow> result;

            if (transaction == null)
            {
                result = await _connectionWrapper.QueryAsync<SqlWorkflow>
                (
                    "CreateWorkflows", 
                    parameters,
                    commandType: CommandType.StoredProcedure
                );
            }
            else
            {
                result = await transaction.Connection.QueryAsync<SqlWorkflow>
                (
                    "CreateWorkflows", 
                    parameters,
                    transaction, 
                    commandType: CommandType.StoredProcedure
                );
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

            var parameters = new DynamicParameters();
            parameters.Add("@publishRevision", publishRevision);
            parameters.Add("@workflowStates", ToWorkflowStatesCollectionDataTable(dWorkflowStates));

            IEnumerable<SqlState> result;

            if (transaction == null)
            {
                result = await _connectionWrapper.QueryAsync<SqlState>
                (
                    "CreateWorkflowStates", 
                    parameters,
                    commandType: CommandType.StoredProcedure
                );
            }
            else
            {
                result = await transaction.Connection.QueryAsync<SqlState>
                (
                    "CreateWorkflowStates", 
                    parameters,
                    transaction, 
                    commandType: CommandType.StoredProcedure
                );
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

            var parameters = new DynamicParameters();
            parameters.Add("@publishRevision", publishRevision);
            parameters.Add("@workflowStates", ToWorkflowStatesCollectionDataTable(dWorkflowStates));

            IEnumerable<SqlState> result;

            if (transaction == null)
            {
                result = await _connectionWrapper.QueryAsync<SqlState>
                (
                    "UpdateWorkflowStates", 
                    parameters, 
                    commandType: CommandType.StoredProcedure
                );
            }
            else
            {
                result = await transaction.Connection.QueryAsync<SqlState>
                (
                    "UpdateWorkflowStates", 
                    parameters, 
                    transaction,
                    commandType: CommandType.StoredProcedure
                );
            }

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

            var parameters = new DynamicParameters();
            parameters.Add("@publishRevision", publishRevision);
            parameters.Add("@workflowStateIds", SqlConnectionWrapper.ToDataTable(listWorkflowStateIds));

            IEnumerable<SqlState> result;

            if (transaction == null)
            {
                result = await _connectionWrapper.QueryAsync<SqlState>
                (
                    "DeleteWorkflowStates", 
                    parameters,
                    commandType: CommandType.StoredProcedure
                );
            }
            else
            {
                result = await transaction.Connection.QueryAsync<SqlState>
                (
                    "DeleteWorkflowStates", 
                    parameters, 
                    transaction,
                    commandType: CommandType.StoredProcedure
                );
            }

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

            if (transaction == null)
            {
                await _connectionWrapper.ExecuteAsync
                (
                    "UpdateWorkflowsChangedWithRevisions", 
                    parameters,
                    commandType: CommandType.StoredProcedure
                );
            }
            else
            {
                await transaction.Connection.ExecuteAsync
                (
                    "UpdateWorkflowsChangedWithRevisions", 
                    parameters,
                    transaction, 
                    commandType: CommandType.StoredProcedure
                );
            }
        }

        public async Task<int> CreateWorkflow(SqlWorkflow workflow, int revision, IDbTransaction transaction)
        {
            var parameters = new DynamicParameters();
            parameters.Add("@Name", workflow.Name);
            parameters.Add("@Description", workflow.Description);
            parameters.Add("@RevisionId", revision);
            parameters.Add("@ErrorCode", dbType: DbType.Int32, direction: ParameterDirection.Output);

            int result;

            if (transaction == null)
            {
                result = await _connectionWrapper.ExecuteScalarAsync<int>
                (
                    "CreateWorkflow", 
                    parameters, 
                    commandType: CommandType.StoredProcedure
                );
            }
            else
            {
                result = await transaction.Connection.ExecuteScalarAsync<int>
                (
                    "CreateWorkflow", 
                    parameters, 
                    transaction, 
                    commandType: CommandType.StoredProcedure
                );
            }

            var errorCode = parameters.Get<int?>("ErrorCode");

            if (errorCode.HasValue)
            {
                switch (errorCode.Value)
                {
                    case (int)SqlErrorCodes.WorkflowWithSuchANameAlreadyExists:
                        throw new ConflictException(ErrorMessages.WorkflowAlreadyExists, ErrorCodes.WorkflowAlreadyExists);
                    case (int)SqlErrorCodes.GeneralSqlError:
                        throw new Exception(ErrorMessages.GeneralErrorOfCreatingWorkflow);
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

            var parameters = new DynamicParameters();
            parameters.Add("@publishRevision", publishRevision);
            parameters.Add("@workflowEvents", ToWorkflowEventsCollectionDataTable(dWorkflowEvents));

            IEnumerable<SqlWorkflowEvent> result;

            if (transaction == null)
            {
                result = await _connectionWrapper.QueryAsync<SqlWorkflowEvent>
                (
                    "CreateWorkflowEvents", 
                    parameters,
                    commandType: CommandType.StoredProcedure
                );
            }
            else
            {
                result = await transaction.Connection.QueryAsync<SqlWorkflowEvent>
                (
                    "CreateWorkflowEvents", 
                    parameters,
                    transaction, 
                    commandType: CommandType.StoredProcedure
                );
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

            var parameters = new DynamicParameters();
            parameters.Add("@publishRevision", publishRevision);
            parameters.Add("@workflowEvents", ToWorkflowEventsCollectionDataTable(dWorkflowEvents));

            IEnumerable<SqlWorkflowEvent> result;

            if (transaction == null)
            {
                result = await _connectionWrapper.QueryAsync<SqlWorkflowEvent>
                (
                    "UpdateWorkflowEvents", 
                    parameters,
                    commandType: CommandType.StoredProcedure
                );
            }
            else
            {
                result = await transaction.Connection.QueryAsync<SqlWorkflowEvent>
                (
                    "UpdateWorkflowEvents", 
                    parameters,
                    transaction, 
                    commandType: CommandType.StoredProcedure
                );
            }

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

            var parameters = new DynamicParameters();
            parameters.Add("@publishRevision", publishRevision);
            parameters.Add("@workflowEventIds", SqlConnectionWrapper.ToDataTable(listWorkflowStateIds));

            IEnumerable<SqlWorkflowEvent> result;

            if (transaction == null)
            {
                result = await _connectionWrapper.QueryAsync<SqlWorkflowEvent>
                (
                    "DeleteWorkflowEvents", 
                    parameters,
                    commandType: CommandType.StoredProcedure
                );
            }
            else
            {
                result = await transaction.Connection.QueryAsync<SqlWorkflowEvent>
                (
                    "DeleteWorkflowEvents", 
                    parameters,
                    transaction, 
                    commandType: CommandType.StoredProcedure
                );
            }

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

            var parameters = new DynamicParameters();
            parameters.Add("@projectArtifactTypePairs", SqlConnectionWrapper.ToIdStringMapDataTable(projectArtifactTypePairList));
            parameters.Add("@revisionId", publishRevision);
            parameters.Add("@workflowId", workflowId);

            if (transaction == null)
            {
                await _connectionWrapper.ExecuteAsync
                (
                    "UpdateItemTypeVersionsWithWorkflowId", 
                    parameters,
                    commandType: CommandType.StoredProcedure
                );
            }
            else
            {
                await transaction.Connection.ExecuteAsync
                (
                    "UpdateItemTypeVersionsWithWorkflowId", 
                    parameters,
                    transaction, 
                    commandType: CommandType.StoredProcedure
                );
            }
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

        public async Task<List<InstanceItem>> GetWorkflowAvailableProjectsAsync(int workflowId, int folderId)
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
            prm.Add("@workflowId", workflowId, dbType:DbType.Int32);
            prm.Add("@folderId", folderId, dbType: DbType.Int32);          
            prm.Add("@ErrorCode", dbType: DbType.Int32, direction: ParameterDirection.Output);

            var items = await _connectionWrapper.QueryAsync<InstanceItem>("GetWorkflowAvailableProjects", prm, commandType: CommandType.StoredProcedure);

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

            if (items != null && items.Count() > 0)
                return items.OrderBy(i => i.Type).ThenBy(i => i.Name).ToList();
            else return new List<InstanceItem>();           
        }

        public async Task<QueryResult<WorkflowProjectArtifactTypesDto>> GetProjectArtifactTypesAssignedtoWorkflowAsync(int workflowId, Pagination pagination, 
                                            string search = null)
        {
            if (workflowId < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(workflowId));
            }
            if (search != null)
            {
                search = UsersHelper.ReplaceWildcardCharacters(search);
            }

            var prm = new DynamicParameters();
            prm.Add("@Offset", pagination.Offset);
            prm.Add("@WorkflowId", workflowId, DbType.Int32);
            prm.Add("@Limit", pagination.Limit);
            prm.Add("@Search", search ?? string.Empty);
            prm.Add("@Total", dbType: DbType.Int32, direction: ParameterDirection.Output);
            prm.Add("@ErrorCode", dbType: DbType.Int32, direction: ParameterDirection.Output);

            var items = (await _connectionWrapper.QueryAsync<WorkflowProjectArtifactType>("GetWorkflowProjectsArtifactTypes", prm, commandType: CommandType.StoredProcedure));

            var workflowArtifactTypes = items as WorkflowProjectArtifactType[] ?? items.ToArray();
            var projectIds = workflowArtifactTypes.Select(x => x.ProjectId).Distinct().ToList();

            var groupedList = new List<WorkflowProjectArtifactTypesDto>();

            foreach (var projectId in projectIds)
            {
                var artifacts = workflowArtifactTypes.Where(x => x.ProjectId == projectId).ToList();
                
                var projectArtifacts = artifacts.Select(artifact => new WorkflowArtifactType()
                {
                    Id = artifact.ArtifactId, Name = artifact.ArtifactName
                }).ToList();
              
                string projectName = artifacts[0].ProjectName;

                var groupedProjectArtifacts = new WorkflowProjectArtifactTypesDto()
                {
                    ProjectId = projectId,
                    ProjectName = projectName,
                    Artifacts = projectArtifacts.OrderBy(a=>a.Name)
                };
                groupedList.Add(groupedProjectArtifacts);
            }

            var errorCode = prm.Get<int?>("ErrorCode");
            var total = prm.Get<int?>("Total");

            if (errorCode.HasValue)
            {
                switch (errorCode.Value)
                {
                    case (int)SqlErrorCodes.WorkflowWithCurrentIdNotExist:
                        throw new ResourceNotFoundException(ErrorMessages.WorkflowNotExist, ErrorCodes.ResourceNotFound);
                }
            }

            return new QueryResult<WorkflowProjectArtifactTypesDto>() { Items = groupedList, Total = total ?? 0 };
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

        public async Task<int> UnassignProjectsAndArtifactTypesFromWorkflowAsync(int workflowId, OperationScope scope, string search = null)
        {
            if (search != null)
            {
                search = UsersHelper.ReplaceWildcardCharacters(search);
            }
            
            var parameters = new DynamicParameters();
            parameters.Add("@WorkflowId", workflowId);
            parameters.Add("@AllProjects", scope.SelectAll);
            parameters.Add("@ProjectIds", SqlConnectionWrapper.ToDataTable(scope.Ids));
            parameters.Add("@Search", search);
            parameters.Add("@ErrorCode", dbType: DbType.Int32, direction: ParameterDirection.Output);
            
            var result = await _connectionWrapper.ExecuteScalarAsync<int>("UnassignProjectsAndArtifactTypesFromWorkflow", parameters, 
                commandType: CommandType.StoredProcedure);
            
            var errorCode = parameters.Get<int?>("ErrorCode");
            if (errorCode.HasValue)
            {
                switch (errorCode.Value)
                {
                    case (int)SqlErrorCodes.WorkflowWithCurrentIdNotExist:
                        throw new ResourceNotFoundException(ErrorMessages.WorkflowNotExist, ErrorCodes.ResourceNotFound);
                    case (int)SqlErrorCodes.WorkflowWithCurrentIdIsActive:
                        throw new ConflictException(ErrorMessages.WorkflowIsActive, ErrorCodes.WorkflowIsActive);
                    case (int)SqlErrorCodes.GeneralSqlError:
                        throw new Exception(ErrorMessages.GeneralErrorOfDeletingWorkflows);
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

        public async Task UpdateWorkflowAsync(SqlWorkflow workflow, int revision, IDbTransaction transaction = null)
        {
            var parameters = new DynamicParameters();
            parameters.Add("@WorkflowId", workflow.WorkflowId);
            parameters.Add("@Name", workflow.Name);
            parameters.Add("@Description", workflow.Description);
            parameters.Add("@Status", workflow.Active);
            parameters.Add("@RevisionId", revision);

            parameters.Add("@ErrorCode", dbType: DbType.Int32, direction: ParameterDirection.Output);

            if (transaction != null)
            {
                    await
                        transaction.Connection.ExecuteScalarAsync<int>("UpdateWorkflow", parameters, transaction,
                            commandType: CommandType.StoredProcedure);
            }
            else
            {
                    await
                        _connectionWrapper.ExecuteScalarAsync<int>("UpdateWorkflow", parameters,
                            commandType: CommandType.StoredProcedure);
            }

            var errorCode = parameters.Get<int?>("ErrorCode");

            if (errorCode.HasValue)
            {
                switch (errorCode.Value)
                {
                    case (int)SqlErrorCodes.GeneralSqlError:
                        throw new Exception(ErrorMessages.GeneralErrorOfUpdatingWorkflow);

                    case (int)SqlErrorCodes.WorkflowWithCurrentIdNotExist:
                        throw new ResourceNotFoundException(ErrorMessages.WorkflowNotExist, ErrorCodes.ResourceNotFound);

                    case (int)SqlErrorCodes.WorkflowWithCurrentIdIsActive:
                        throw new ConflictException(ErrorMessages.WorkflowIsActive, ErrorCodes.WorkflowIsActive);

                    case (int)SqlErrorCodes.WorkflowWithoutProjectArtifactTypeAssignmentsCannotBeActivated:
                        throw new ConflictException(ErrorMessages.WorkflowWithoutProjectArtifactTypeAssignmentsCannotBeActivated, ErrorCodes.Conflict);

                    case (int)SqlErrorCodes.WorkflowHasSameProjectArtifactTypeAssignedToAnotherActiveWorkflow:
                        throw new ConflictException(ErrorMessages.WorkflowHasSameProjectArtifactTypeAssignedToAnotherActiveWorkflow, ErrorCodes.Conflict);
                }
            }
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

        public async Task<SyncResult> AssignArtifactTypesToProjectInWorkflow(int workflowId, int projectId, IEnumerable<int> artifactTypeIds)
        {
            var parameters = new DynamicParameters();
            
            parameters.Add("@WorkflowId", workflowId);
            parameters.Add("@ProjectId", projectId);
            parameters.Add("@ArtifactTypeIds", SqlConnectionWrapper.ToDataTable(artifactTypeIds, "Int32Collection", "Int32Value"));           
            parameters.Add("@ErrorCode", dbType: DbType.Int32, direction: ParameterDirection.Output);
        
            var result = await _connectionWrapper.QueryAsync<SyncResult>("AssignArtifactTypesToProjectInWorkflow", parameters, commandType: CommandType.StoredProcedure);
            var errorCode = parameters.Get<int?>("ErrorCode");                      

            if (errorCode.HasValue)
            {
                switch (errorCode.Value)
                {
                    case (int)SqlErrorCodes.GeneralSqlError:
                        throw new Exception(ErrorMessages.GeneralErrorOfAssignProjectsAndArtifactTypesToWorkflow);

                    case (int)SqlErrorCodes.WorkflowWithCurrentIdNotExist:
                        throw new ResourceNotFoundException(ErrorMessages.WorkflowNotExist, ErrorCodes.ResourceNotFound);

                    case (int)SqlErrorCodes.ProjectWithCurrentIdNotExist:
                        throw new ResourceNotFoundException(ErrorMessages.ProjectNotExist, ErrorCodes.ResourceNotFound);

                    case (int)SqlErrorCodes.WorkflowWithCurrentIdIsActive:
                        throw new ConflictException(ErrorMessages.WorkflowIsActive, ErrorCodes.WorkflowIsActive);                   

                    case (int)SqlErrorCodes.WorkflowProjectHasNoArtifactTypes:
                        throw new ConflictException(ErrorMessages.WorkflowProjectHasNoArtifactTypes, ErrorCodes.WorkflowProjectHasNoArtifactTypes);
                }
            }

            return result.FirstOrDefault();
        }
        #endregion
    }
}
