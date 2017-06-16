using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using AdminStore.Models.Workflow;
using Dapper;
using ServiceLibrary.Helpers;
using ServiceLibrary.Repositories;

namespace AdminStore.Repositories.Workflow
{
    public class WorkflowRepository : IWorkflowRepository
    {
        internal readonly ISqlConnectionWrapper ConnectionWrapper;
        private ISqlHelper _sqlHelper;
        private IWorkflowValidator _workflowValidator;

        #region Constructors

        public WorkflowRepository()
            : this(new SqlConnectionWrapper(ServiceConstants.RaptorMain), new SqlHelper(), new WorkflowValidator())
        {
        }

        internal WorkflowRepository(ISqlConnectionWrapper connectionWrapper, ISqlHelper sqlHelper, IWorkflowValidator workfloweValidator)
        {
            ConnectionWrapper = connectionWrapper;
            _sqlHelper = sqlHelper;
            _workflowValidator = workfloweValidator;
        }

        #endregion


        #region Interface implementation

        public async Task<ImportWorkflowResult> ImportWorkflowAsync(IeWorkflow workflow, int userId)
        {
            if (workflow == null)
            {
                throw new NullReferenceException(nameof(workflow));
            }

            var importResult = new ImportWorkflowResult();

            if (!IsWorkflowFeatureEnabled())
            {
                // TODO: Create a text file and save it to the file store.
                // TODO: The name convention for the error file "$workflow_import_errors$.txt".
                // TODO: The name convention should be checked when the errors are requested by the client. 
                // TODO: Return guid of the errors file.
                importResult.ErrorsGuid = "temp_guid";
                return importResult;
            }

            var validationResult = _workflowValidator.Validate(workflow);
            if (validationResult.HasErrors)
            {
                // TODO: Create a text file and save it to the file store.
                // TODO: The name convention for the error file "$workflow_import_errors$.txt".
                // TODO: The name convention should be checked when the errors are requested by the client. 
                // TODO: Return guid of the errors file.
                importResult.ErrorsGuid = "temp_guid";
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
                IEnumerable<DState> newStates = null;
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
                }
                newStates = await CreateWorkflowStatesAsync(importStateParams, publishRevision, transaction);
            };

            await _sqlHelper.RunInTransactionAsync(ServiceConstants.RaptorMain, action);
            importResult.WorkflowId = newWorkflow?.WorkflowId;

            return importResult;

        }

        public async Task<string> GetImportWorkflowErrorsAsync(string guid, int userId)
        {
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

        #endregion

        #region Private methods

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



        #endregion

    }
}