using System.Threading.Tasks;

namespace ServiceLibrary.Repositories
{
    public interface ILockArtifactsRepository
    {
        Task<bool> LockArtifactAsync(int artifactId, int userId);
    }
}
