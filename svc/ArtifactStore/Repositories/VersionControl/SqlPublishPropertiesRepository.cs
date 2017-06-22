using System.Data;
using System.Threading.Tasks;
using ArtifactStore.Helpers;
using ArtifactStore.Services.VersionControl;
using ServiceLibrary.Models.VersionControl;

namespace ArtifactStore.Repositories.VersionControl
{
    public class SqlPublishPropertiesRepository : SqlPublishRepository, IPublishRepository
    {
        public async Task Execute(int revisionId, PublishParameters parameters, PublishEnvironment environment, IDbTransaction transaction = null)
        {
            await Task.Run(() => { });
        }
    }
}