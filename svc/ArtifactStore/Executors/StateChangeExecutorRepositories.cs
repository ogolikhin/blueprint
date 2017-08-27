using ArtifactStore.Repositories;
using ArtifactStore.Repositories.Reuse;
using ArtifactStore.Repositories.Workflow;
using ArtifactStore.Services.VersionControl;
using ServiceLibrary.Repositories;
using ServiceLibrary.Repositories.ConfigControl;

namespace ArtifactStore.Executors
{
    public interface IStateChangeExecutorRepositories
    {
        IArtifactVersionsRepository ArtifactVersionsRepository { get; }
        IWorkflowRepository WorkflowRepository { get; }
        IVersionControlService VersionControlService { get; }
        IReuseRepository ReuseRepository { get; }
        ISaveArtifactRepository SaveArtifactRepository { get; }
        IApplicationSettingsRepository ApplicationSettingsRepository { get; }
        IServiceLogRepository ServiceLogRepository { get; }
    }

    public class StateChangeExecutorRepositories : IStateChangeExecutorRepositories
    {
        public IArtifactVersionsRepository ArtifactVersionsRepository { get; }
        public IWorkflowRepository WorkflowRepository { get; }
        public IVersionControlService VersionControlService { get; }
        public IReuseRepository ReuseRepository { get; }
        public ISaveArtifactRepository SaveArtifactRepository { get; }
        public IApplicationSettingsRepository ApplicationSettingsRepository { get; }
        public IServiceLogRepository ServiceLogRepository { get; }


        public StateChangeExecutorRepositories(IArtifactVersionsRepository artifactVersionsRepository,
            IWorkflowRepository workflowRepository,
            IVersionControlService versionControlService,
            IReuseRepository reuseRepository,
            ISaveArtifactRepository saveArtifactRepository,
            IApplicationSettingsRepository applicationSettingsRepository,
            IServiceLogRepository serviceLogRepository)
        {
            ArtifactVersionsRepository = artifactVersionsRepository;
            WorkflowRepository = workflowRepository;
            VersionControlService = versionControlService;
            ReuseRepository = reuseRepository;
            SaveArtifactRepository = saveArtifactRepository;
            ApplicationSettingsRepository = applicationSettingsRepository;
            ServiceLogRepository = serviceLogRepository;
        }
    }
}