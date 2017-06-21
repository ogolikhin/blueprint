using System.Collections.Generic;
using System.Threading.Tasks;
using ArtifactStore.Helpers;
using ArtifactStore.Services.VersionControl;
using ServiceLibrary.Helpers;

namespace ArtifactStore.Repositories.VersionControl
{
    public class SqlPublishRepositoryComposer : IPublishRepository
    {
        private readonly ICollection<IPublishRepository> _repositories;

        public SqlPublishRepositoryComposer() :
            this(new List<IPublishRepository>
            {
                new SqlPublishItemsRepository(),
                new SqlPublishRelationshipsRepository(),
                new SqlPublishPropertiesRepository(),
                new SqlPublishAttachmentsRepository(),
                new SqlPublishReuseProcessingRepository(),
                new SqlPublishCollectionAssignmentsRepository(),
                new SqlJournalPublishRepository()
            })
        { }

        public SqlPublishRepositoryComposer(ICollection<IPublishRepository> repositories)
        {
            _repositories = repositories ?? new List<IPublishRepository>();
        }

        public async Task Execute(ISqlHelper sqlHelper, int revisionId, PublishParameters parameters, PublishEnvironment environment)
        {
            await Task.Run(() =>
            {
                
            });
            foreach (var repo in _repositories)
            {
                await repo.Execute(sqlHelper, revisionId, parameters, environment);
            }
        }
    }
}