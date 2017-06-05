using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ArtifactStore.Executors;
using ArtifactStore.Repositories;
using ArtifactStore.Repositories.Workflow;
using ServiceLibrary.Models;
using ServiceLibrary.Models.Workflow;

namespace ArtifactStore.Services.Workflow
{
    public interface IWorkflowService
    {
        Task<WorkflowTransitionResult> GetTransitions(int userId, int artifactId, int workflowId, int stateId);

        Task<QuerySingleResult<WorkflowState>> GetCurrentState(int userId, int artifactId, int revisionId = int.MaxValue,
            bool addDrafts = true);

        Task<QuerySingleResult<WorkflowState>> ChangeStateForArtifact(int userId,
            WorkflowStateChangeParameter stateChangeParameter);
    }

    public class WorkflowService : IWorkflowService
    {
        private readonly IWorkflowRepository _workflowRepository;
        private readonly IArtifactVersionsRepository _artifactVersionsRepository;

        public WorkflowService(IWorkflowRepository workflowRepository,
            IArtifactVersionsRepository artifactVersionsRepository)
        {
            _workflowRepository = workflowRepository;
            _artifactVersionsRepository = artifactVersionsRepository;
        }

        public async Task<WorkflowTransitionResult> GetTransitions(int userId, int artifactId, int workflowId, int stateId)
        {
            return await _workflowRepository.GetTransitions(userId, artifactId, workflowId, stateId);
        }

        public async Task<QuerySingleResult<WorkflowState>> GetCurrentState(int userId, int artifactId, int revisionId = Int32.MaxValue, bool addDrafts = true)
        {
            return await _workflowRepository.GetState(userId, artifactId, revisionId, addDrafts);
        }

        public async Task<QuerySingleResult<WorkflowState>> ChangeStateForArtifact(int userId, WorkflowStateChangeParameter stateChangeParameter)
        {
            //Get enhanced state information so that we can validate constraints
            //VALIDATE CONSTRAINTS. USER PERMISSIONS IS ONE SUCH VALID CONSTRAINT
            //PROPERTY CONSTRAINTS WILL BE APPLIED
            var propertyConstraints = new List<IConstraint>
            {
                new PropertyRequiredConstraint(),
                new PropertyRequiredConstraint(),
                new PropertyRequiredConstraint()
            };

            var postOpActions = new List<IAction>
                {
                    new EmailAction(),
                    new EmailAction(),
                    new EmailAction(),
                };

            var stateChangeExecutor = new StateChangeExecutor(propertyConstraints, 
                postOpActions, 
                stateChangeParameter, 
                userId, 
                _artifactVersionsRepository, 
                _workflowRepository);

            return await stateChangeExecutor.Execute();
        }

       
    }
}