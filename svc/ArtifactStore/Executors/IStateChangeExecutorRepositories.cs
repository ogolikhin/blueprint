using ArtifactStore.Repositories;
using ArtifactStore.Repositories.Reuse;
using ArtifactStore.Repositories.Workflow;
using ArtifactStore.Services.VersionControl;
using ServiceLibrary.Models.VersionControl;
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
}
