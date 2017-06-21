using System.Threading.Tasks;
using ArtifactStore.Helpers;
using ArtifactStore.Services.VersionControl;
using ServiceLibrary.Helpers;

namespace ArtifactStore.Repositories.VersionControl
{
    public interface IPublishRepository
    {
        Task Execute(ISqlHelper sqlHelper, int revisionId, PublishParameters parameters, PublishEnvironment environment);
    }
}
