using ArtifactStore.Repositories;
using ServiceLibrary.Models.VersionControl;
using ServiceLibrary.Repositories;
using ServiceLibrary.Repositories.ConfigControl;
using ServiceLibrary.Repositories.ProjectMeta;
using ServiceLibrary.Repositories.Reuse;
using ServiceLibrary.Repositories.Webhooks;
using ServiceLibrary.Repositories.Workflow;

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
        IUsersRepository UsersRepository { get; }
        IWebhooksRepository WebhooksRepository { get; }
        IProjectMetaRepository ProjectMetaRepository { get; }
    }
}
