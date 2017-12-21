using System.Collections.Generic;
using System.Threading.Tasks;

namespace BlueprintSys.RC.Services.MessageHandlers.PropertyItemTypesChanged
{
    public interface IPropertyItemTypesChangedRepository : IBaseRepository
    {
        /// <summary>
        /// TODO
        /// </summary>
        Task<List<int>> GetAffectedArtifactIds();
    }

    public class PropertyItemTypesChangedRepository : BaseRepository, IPropertyItemTypesChangedRepository
    {
        public PropertyItemTypesChangedRepository(string connectionString) : base(connectionString)
        {
        }

        public async Task<List<int>> GetAffectedArtifactIds()
        {
            //TODO
            return await Task.FromResult(new List<int>());
        }
    }
}
