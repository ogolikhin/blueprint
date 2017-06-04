using System.Threading.Tasks;

namespace ServiceLibrary.Models
{
    public interface IConstraint<in T>
    {
        Task<bool> IsFulfilled();
    }

    public class ArtifactPropertyConstraint : IConstraint<Artifact>
    {
        public async Task<bool> IsFulfilled()
        {
            return await Task.FromResult(true);
        }
    }
}
