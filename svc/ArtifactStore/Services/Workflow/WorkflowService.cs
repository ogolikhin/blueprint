using System.Threading.Tasks;
using ArtifactStore.Executors;
using ArtifactStore.Models.Workflow;
using ArtifactStore.Repositories;
using ArtifactStore.Repositories.Reuse;
using ArtifactStore.Repositories.Workflow;
using ArtifactStore.Services.VersionControl;
using ServiceLibrary.Helpers;
using ServiceLibrary.Models;
using ServiceLibrary.Models.Enums;
using ServiceLibrary.Models.Workflow;
using ServiceLibrary.Repositories;

namespace ArtifactStore.Services.Workflow
{
    public interface IWorkflowService
    {
        Task<WorkflowTransitionResult> GetTransitionsAsync(int userId, int artifactId, int workflowId, int stateId);

        Task<QuerySingleResult<WorkflowState>> GetStateForArtifactAsync(int userId, int artifactId, int? versionId = null,
            bool addDrafts = true);

        Task<QuerySingleResult<WorkflowState>> ChangeStateForArtifactAsync(int userId, int artifactId,
            WorkflowStateChangeParameter stateChangeParameter);
    }

    public class WorkflowService : IWorkflowService
    {
        private readonly IWorkflowRepository _workflowRepository;
        private readonly IArtifactVersionsRepository _artifactVersionsRepository;
        private readonly ISqlItemInfoRepository _itemInfoRepository;
        private readonly ISqlHelper _sqlHelper;
        private readonly IVersionControlService _versionControlService;
        private readonly IReuseRepository _reuseRepository;
        private readonly ISaveArtifactRepository _saveArtifactRepository;
        private readonly IApplicationSettingsRepository _applicationSettingsRepository;

        public WorkflowService(IWorkflowRepository workflowRepository,
            IArtifactVersionsRepository artifactVersionsRepository,
            ISqlItemInfoRepository itemInfoRepository,
            ISqlHelper sqlHelper,
            IVersionControlService versionControlService,
            IReuseRepository reuseRepository,
            ISaveArtifactRepository saveArtifactRepository,
            IApplicationSettingsRepository applicationSettingsRepository)
        {
            _workflowRepository = workflowRepository;
            _artifactVersionsRepository = artifactVersionsRepository;
            _itemInfoRepository = itemInfoRepository;
            _sqlHelper = sqlHelper;
            _versionControlService = versionControlService;
            _reuseRepository = reuseRepository;
            _saveArtifactRepository = saveArtifactRepository;
            _applicationSettingsRepository = applicationSettingsRepository;
        }

        public async Task<WorkflowTransitionResult> GetTransitionsAsync(int userId, int artifactId, int workflowId, int stateId)
        {
            var transitions = await _workflowRepository.GetTransitionsAsync(userId, artifactId, workflowId, stateId);

            return new WorkflowTransitionResult
            {
                ResultCode = QueryResultCode.Success,
                Total = transitions.Count,
                Count = transitions.Count,
                Items = transitions
            };
        }

        public async Task<QuerySingleResult<WorkflowState>> GetStateForArtifactAsync(int userId, int artifactId, int? versionId = null, bool addDrafts = true)
        {
            var revisionId = await _itemInfoRepository.GetRevisionId(artifactId, userId, versionId);
            var state = await _workflowRepository.GetStateForArtifactAsync(userId, artifactId, revisionId, addDrafts);
            if (state == null || state.WorkflowId <= 0 || state.Id <= 0)
            {
                return new QuerySingleResult<WorkflowState>
                {
                    ResultCode = QueryResultCode.Failure,
                    Message = I18NHelper.FormatInvariant("State information could not be retrieved for Artifact (Id:{0}).", artifactId)
                };
            }
            return new QuerySingleResult<WorkflowState>
            {
                ResultCode = QueryResultCode.Success,
                Item = state
            };
        }

        public async Task<QuerySingleResult<WorkflowState>> ChangeStateForArtifactAsync(int userId, int artifactId, WorkflowStateChangeParameter stateChangeParameter)
        {
            //We will be getting state information and then will construct the property constraints and post operation actions over here
            var stateChangeExecutor = new StateChangeExecutor(
                new WorkflowStateChangeParameterEx(stateChangeParameter)
                {
                    ArtifactId = artifactId
                },
                userId,
                _artifactVersionsRepository,
                _workflowRepository,
                _sqlHelper,
                _versionControlService,
                _reuseRepository,
                _saveArtifactRepository,
                _applicationSettingsRepository
                );

            return await stateChangeExecutor.Execute();
        }


    }
}