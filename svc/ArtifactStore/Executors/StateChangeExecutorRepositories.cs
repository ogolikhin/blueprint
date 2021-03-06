﻿using ArtifactStore.Repositories;
using ServiceLibrary.Models.VersionControl;
using ServiceLibrary.Repositories;
using ServiceLibrary.Repositories.ConfigControl;
using ServiceLibrary.Repositories.ProjectMeta;
using ServiceLibrary.Repositories.Reuse;
using ServiceLibrary.Repositories.Webhooks;
using ServiceLibrary.Repositories.Workflow;

namespace ArtifactStore.Executors
{
    public class StateChangeExecutorRepositories : IStateChangeExecutorRepositories
    {
        public IArtifactVersionsRepository ArtifactVersionsRepository { get; }
        public IWorkflowRepository WorkflowRepository { get; }
        public IVersionControlService VersionControlService { get; }
        public IReuseRepository ReuseRepository { get; }
        public ISaveArtifactRepository SaveArtifactRepository { get; }
        public IApplicationSettingsRepository ApplicationSettingsRepository { get; }
        public IServiceLogRepository ServiceLogRepository { get; }
        public IUsersRepository UsersRepository { get; }
        public IWebhooksRepository WebhooksRepository { get; }
        public IProjectMetaRepository ProjectMetaRepository { get; }


        public StateChangeExecutorRepositories(IArtifactVersionsRepository artifactVersionsRepository,
            IWorkflowRepository workflowRepository,
            IVersionControlService versionControlService,
            IReuseRepository reuseRepository,
            ISaveArtifactRepository saveArtifactRepository,
            IApplicationSettingsRepository applicationSettingsRepository,
            IServiceLogRepository serviceLogRepository,
            IUsersRepository usersRepository,
            IWebhooksRepository webhooksRepository,
            IProjectMetaRepository projectMetaRepository)
        {
            ArtifactVersionsRepository = artifactVersionsRepository;
            WorkflowRepository = workflowRepository;
            VersionControlService = versionControlService;
            ReuseRepository = reuseRepository;
            SaveArtifactRepository = saveArtifactRepository;
            ApplicationSettingsRepository = applicationSettingsRepository;
            ServiceLogRepository = serviceLogRepository;
            UsersRepository = usersRepository;
            WebhooksRepository = webhooksRepository;
            ProjectMetaRepository = projectMetaRepository;
        }
    }
}