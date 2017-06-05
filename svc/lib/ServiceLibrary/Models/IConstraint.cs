using System.Threading.Tasks;

namespace ServiceLibrary.Models
{
    public interface IConstraint
    {
        Task<bool> IsFulfilled();
    }

    public class PropertyRequiredConstraint : IConstraint
    {
        public async Task<bool> IsFulfilled()
        {
            return await Task.FromResult(true);
        }
    }
}
