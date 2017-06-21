using System.Threading.Tasks;
using ArtifactStore.Helpers;
using ArtifactStore.Services.VersionControl;
using ServiceLibrary.Helpers;

namespace ArtifactStore.Repositories.VersionControl
{
    public class SqlPublishAttachmentsRepository : SqlPublishRepository, IPublishRepository
    {
        public async Task Execute(ISqlHelper sqlHelper, int revisionId, PublishParameters parameters, PublishEnvironment environment)
        {
            await Task.Run(() => { });
        }
    }
}