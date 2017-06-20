using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using AdminStore.Models.Workflow;
using Dapper;
using ServiceLibrary.Exceptions;
using ServiceLibrary.Helpers;
using ServiceLibrary.Repositories;

namespace AdminStore.Repositories.Workflow
{
    public class WorkflowRepository : IWorkflowRepository
    {
        internal readonly ISqlConnectionWrapper ConnectionWrapper;
        private readonly ISqlUserRepository _userRepository;
        private readonly ISqlHelper _sqlHelper;
        private readonly IWorkflowValidator _workflowValidator;

        #region Constructors

        public WorkflowRepository()
            : this(new SqlConnectionWrapper(ServiceConstants.RaptorMain),
                  new SqlHelper(), 
                  new WorkflowValidator(),
                  new SqlUserRepository())
        {
        }

        internal WorkflowRepository(ISqlConnectionWrapper connectionWrapper,
            ISqlHelper sqlHelper,
            IWorkflowValidator workfloweValidator,
            ISqlUserRepository userRepository)
        {
            ConnectionWrapper = connectionWrapper;
            _sqlHelper = sqlHelper;
            _workflowValidator = workfloweValidator;
            _userRepository = userRepository;
        }

        #endregion


        #region Interface implementation

        public async Task<ImportWorkflowResult> ImportWorkflowAsync(IeWorkflow workflow, string fileName, int userId)
        {
            if (workflow == null)
            {
                throw new NullReferenceException(nameof(workflow));
            }

            await VerifyUserRole(userId);
            VerifyWorkflowFeature();

            var importResult = new ImportWorkflowResult();

            var validationResult = _workflowValidator.Validate(workflow);
            if (validationResult.HasErrors)
            {
                // TODO: Create a text file and save it to the file store.
                // TODO: The name convention for the error file "$workflow_import_errors$.txt".
                // TODO: The name convention should be checked when the errors are requested by the client. 
                // TODO: Return guid of the errors file.
                importResult.ErrorsGuid = "temp_guid";
                importResult.ResultCode = ImportWorkflowResultCodes.InvalidModel;
                return importResult;
            }

            DWorkflow newWorkflow = null;

            Func<IDbTransaction, Task> action = async transaction =>
            {
                var publishRevision = await _sqlHelper.CreateRevisionInTransactionAsync(transaction, userId, "Workflow import.");
                var duplicateNames = await CheckLiveWorkflowsForNameUniqueness(transaction, new[] { workflow.Name });
                if (duplicateNames.Any())
                {
                    // TODO: Create a text file and save it to the file store, see TODO above.
                    importResult.ErrorsGuid = "temp_guid";
                    return;
                }
                
                var importParams = new DWorkflow
                {
                    Name = workflow.Name,
                    Description = workflow.Description
                };
                newWorkflow = (await CreateWorkflowsAsync(new [] {importParams}, publishRevision, transaction)).FirstOrDefault();

                var importStateParams = new List<DState>();
                var importTriggersParams = new List<DTrigger>();
                IEnumerable<DState> newStates = null;
                IEnumerable<DTrigger> newTriggers = null;
                if (newWorkflow != null)
                {
                    float orderIndex = 0;
                    workflow.States.ForEach(state =>
                    {

                        importStateParams.Add(new DState
                        {
                            Name = state.Name,
                            Description = state.Description,
                            WorkflowId = newWorkflow.WorkflowId,
                            Default = state.IsInitial.HasValue && state.IsInitial.Value,
                            OrderIndex = orderIndex
                        });
                        orderIndex += 10;
                    });
                    newStates = await CreateWorkflowStatesAsync(importStateParams, publishRevision, transaction);

                    if (newStates != null)
                    {
                        var newStatesArray = newStates.ToArray();
                        HashSet<string> listOfAllGroups = new HashSet<string>();
                        workflow.Transitions.ForEach(transition =>
                        {
                            transition.PermissionGroups.ForEach(group =>
                            {
                                if (!listOfAllGroups.Contains(group.Name))
                                {
                                    listOfAllGroups.Add(group.Name);
                                }
                            });
                        });
                        var nonExistantGroupNames = await _userRepository.GetNonExistantGroupsByName(listOfAllGroups);
                        if (nonExistantGroupNames.Any())
                        {
                            throw new DuplicateNameException(nonExistantGroupNames.ToString());
                        }
                        
                        workflow.Transitions.ForEach(transition =>
                        {
                            importTriggersParams.Add(new DTrigger
                            {
                                Name = transition.Name,
                                Description = string.Empty,
                                WorkflowId = newWorkflow.WorkflowId,
                                Type = DTriggerType.Transition,
                                Permissions = SerializationHelper.ToXml(transition.PermissionGroups),
                                Validations = null,
                                Actions = null,
                                ProjectId = null,
                                WorkflowState1Id = newStatesArray.FirstOrDefault(s => s.Name.Equals(transition.FromState))?.WorkflowStateId,
                                WorkflowState2Id = newStatesArray.FirstOrDefault(s => s.Name.Equals(transition.ToState))?.WorkflowStateId,
                                PropertyTypeId = null
                            });
                        });
                        newTriggers = await CreateWorkflowTriggersAsync(importTriggersParams, publishRevision, transaction);
                    }
                    
                }
                
            };

            await _sqlHelper.RunInTransactionAsync(ServiceConstants.RaptorMain, action);
            importResult.WorkflowId = newWorkflow?.WorkflowId;
            importResult.ResultCode = ImportWorkflowResultCodes.Ok;

            return importResult;

        }

        public async Task<string> GetImportWorkflowErrorsAsync(string guid, int userId)
        {
            await VerifyUserRole(userId);
            VerifyWorkflowFeature();

            await Task.Delay(1); // TODO: temp, remove
            throw new NotImplementedException();
        }

        public async Task<IEnumerable<DWorkflow>> CreateWorkflowsAsync(IEnumerable<DWorkflow> workflows, int publishRevision, IDbTransaction transaction = null)
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

            IEnumerable<DWorkflow> result;
            if (transaction == null)
            {
                result = await ConnectionWrapper.QueryAsync<DWorkflow>("CreateWorkflows", prm,
                    commandType: CommandType.StoredProcedure);
            }
            else
            {
                result = await transaction.Connection.QueryAsync<DWorkflow>("CreateWorkflows", prm,
                    transaction, commandType: CommandType.StoredProcedure); ;
            }

            return result;
        }

        public async Task<IEnumerable<DState>> CreateWorkflowStatesAsync(IEnumerable<DState> workflowStates, int publishRevision, IDbTransaction transaction = null)
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

            IEnumerable<DState> result;
            if (transaction == null)
            {
                result = await ConnectionWrapper.QueryAsync<DState>("CreateWorkflowStates", prm,
                    commandType: CommandType.StoredProcedure);
            }
            else
            {
                result = await transaction.Connection.QueryAsync<DState>("CreateWorkflowStates", prm,
                    transaction, commandType: CommandType.StoredProcedure); ;
            }

            return result;
        }

        public async Task<IEnumerable<DTrigger>> CreateWorkflowTriggersAsync(IEnumerable<DTrigger> workflowTriggers, int publishRevision, IDbTransaction transaction = null)
        {
            if (workflowTriggers == null)
            {
                throw new ArgumentNullException(nameof(workflowTriggers));
            }

            var dWorkflowTriggers = workflowTriggers.ToList();
            if (!dWorkflowTriggers.Any())
            {
                throw new ArgumentException(I18NHelper.FormatInvariant("{0} is empty.", nameof(dWorkflowTriggers)));
            }

            if (publishRevision < 1)
            {
                throw new ArgumentException(I18NHelper.FormatInvariant("{0} is less than 1.", nameof(publishRevision)));
            }

            var prm = new DynamicParameters();
            prm.Add("@publishRevision", publishRevision);
            prm.Add("@workflowTriggers", ToWorkflowTriggersCollectionDataTable(dWorkflowTriggers));

            IEnumerable<DTrigger> result;
            if (transaction == null)
            {
                result = await ConnectionWrapper.QueryAsync<DTrigger>("CreateWorkflowTriggers", prm,
                    commandType: CommandType.StoredProcedure);
            }
            else
            {
                result = await transaction.Connection.QueryAsync<DTrigger>("CreateWorkflowTriggers", prm,
                    transaction, commandType: CommandType.StoredProcedure); ;
            }

            return result;
        }

        #endregion

        #region Private methods

        private async Task VerifyUserRole(int userId)
        {
            var user = await _userRepository.GetLoginUserByIdAsync(userId);
            // At least for now, all instance administrators can import workflows.
            if (user.InstanceAdminRoleId == null)
            {
                throw new AuthorizationException(
                    "The user is not an instance administrator and therefore does not have permissions to import workflows.",
                    ErrorCodes.UnauthorizedAccess);
            }
        }

        private void VerifyWorkflowFeature()
        {
            if (!IsWorkflowFeatureEnabled())
            {
                throw new ConflictException("The Workflow feature is disabled.", ErrorCodes.WorkflowIsDisabled);
            }
        }

        private bool IsWorkflowFeatureEnabled()
        {
            // TODO: after NW made information about Workflow feature available for the services.
            return true;
        }

        private static async Task<IEnumerable<string>> CheckLiveWorkflowsForNameUniqueness(IDbTransaction transaction, IEnumerable<string> names)
        {
            var prm = new DynamicParameters();
            prm.Add("@names", SqlConnectionWrapper.ToStringDataTable(names));
            var duplicateNames = await transaction.Connection.QueryAsync<string>("CheckLiveWorkflowsForNameUniqueness", prm, transaction, commandType: CommandType.StoredProcedure);
            return duplicateNames;
        }

        private static DataTable ToWorkflowsCollectionDataTable(IEnumerable<DWorkflow> workflows)
        {
            var table = new DataTable { Locale = CultureInfo.InvariantCulture };
            table.SetTypeName("WorkflowsCollection");
            table.Columns.Add("WorkflowId", typeof(int));
            table.Columns.Add("Name", typeof(string));
            table.Columns.Add("Description", typeof(string));
            foreach (var workflow in workflows)
            {
                table.Rows.Add(workflow.WorkflowId, workflow.Name, workflow.Description);
            }
            return table;
        }

        private static DataTable ToWorkflowStatesCollectionDataTable(IEnumerable<DState> workflowStates)
        {
            var table = new DataTable { Locale = CultureInfo.InvariantCulture };
            table.SetTypeName("WorkflowStatesCollection");
            table.Columns.Add("WorkflowStateId", typeof(int));
            table.Columns.Add("Name", typeof(string));
            table.Columns.Add("Description", typeof(string));
            table.Columns.Add("WorkflowId", typeof(int));
            table.Columns.Add("Default", typeof(bool));
            table.Columns.Add("OrderIndex", typeof(float));
            foreach (var workflowState in workflowStates)
            {
                table.Rows.Add(workflowState.WorkflowStateId, workflowState.Name, workflowState.Description,
                    workflowState.WorkflowId, workflowState.Default, workflowState.OrderIndex);
            }
            return table;
        }

        private static DataTable ToWorkflowTriggersCollectionDataTable(IEnumerable<DTrigger> workflowTriggers)
        {
            var table = new DataTable { Locale = CultureInfo.InvariantCulture };
            table.SetTypeName("WorkflowTriggersCollection");
            table.Columns.Add("TriggerId", typeof(int));
            table.Columns.Add("Name", typeof(string));
            table.Columns.Add("Description", typeof(string));
            table.Columns.Add("WorkflowId", typeof(int));
            table.Columns.Add("Type", typeof(int));
            table.Columns.Add("Permissions", typeof(string));
            table.Columns.Add("Validations", typeof(string));
            table.Columns.Add("Actions", typeof(string));
            table.Columns.Add("ProjectId", typeof(int));
            table.Columns.Add("WorkflowState1Id", typeof(int));
            table.Columns.Add("WorkflowState2Id", typeof(int));
            table.Columns.Add("PropertyTypeId", typeof(int));

            foreach (var workflowTrigger in workflowTriggers)
            {
                table.Rows.Add(workflowTrigger.TriggerId, workflowTrigger.Name, workflowTrigger.Description,
                    workflowTrigger.WorkflowId, workflowTrigger.Type, workflowTrigger.Permissions, 
                    workflowTrigger.Validations, workflowTrigger.Actions, workflowTrigger.ProjectId, 
                    workflowTrigger.WorkflowState1Id, workflowTrigger.WorkflowState2Id, workflowTrigger.PropertyTypeId);
            }
            return table;
        }



        #endregion

    }
}