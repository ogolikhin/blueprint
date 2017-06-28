﻿using System.Data;
using System.Threading.Tasks;
using ArtifactStore.Helpers;
using ArtifactStore.Services.VersionControl;
using ServiceLibrary.Models.VersionControl;

namespace ArtifactStore.Repositories.VersionControl
{
    public class SqlPublishRepositoryComposer : IPublishRepository
    {
        private readonly IPublishRepositoriesContainer _repositoriesContainer;

        public SqlPublishRepositoryComposer() :
            this(new PublishRepositoriesContainer())
        { }

        public SqlPublishRepositoryComposer(IPublishRepositoriesContainer repositoriesContainer)
        {
            _repositoriesContainer = repositoriesContainer ?? new PublishRepositoriesContainer();
        }

        public async Task Execute(int revisionId, PublishParameters parameters, PublishEnvironment environment, IDbTransaction transaction = null)
        {
            await _repositoriesContainer.PublishItemsRepo.Execute(revisionId, parameters, environment, transaction);

            await _repositoriesContainer.PublishRelationshipsRepo.Execute(revisionId, parameters, environment, transaction);

            await _repositoriesContainer.PublishPropertiesRepo.Execute(revisionId, parameters, environment, transaction);

            await _repositoriesContainer.PublishAttachmentsRepo.Execute(revisionId, parameters, environment, transaction);

            await _repositoriesContainer.PublishReuseProcessingRepo.Execute(revisionId, parameters, environment, transaction);

            //TODO: DISCUSSIONS IS NOT IMPLEMENTED

            await _repositoriesContainer.PublishCollectionAssignmentsRepo.Execute(revisionId, parameters, environment, transaction);
        }
    }
}