using System.Data;
using System.Threading.Tasks;
using ArtifactStore.Helpers;
using ArtifactStore.Services.VersionControl;

namespace ArtifactStore.Repositories.VersionControl
{
    public class SqlPublishCollectionAssignmentsRepository : SqlPublishRepository, IPublishRepository
    {
        public async Task Execute(int revisionId, PublishParameters parameters, PublishEnvironment environment, IDbTransaction transaction = null)
        {
            await Task.Run(() => { });
        }
    }
}