using System.Collections.Generic;
using System.Threading.Tasks;

namespace BlueprintSys.RC.Services.MessageHandlers.ProjectsChanged
{
    public interface IProjectsChangedRepository : IBaseRepository
    {
        /// <summary>
        /// TODO
        /// </summary>
        Task<List<int>> GetAffectedArtifactIds();
    }

    public class ProjectsChangedRepository : BaseRepository, IProjectsChangedRepository
    {
        public ProjectsChangedRepository(string connectionString) : base(connectionString)
        {
        }

        public async Task<List<int>> GetAffectedArtifactIds()
        {
            // TODO
            return await Task.FromResult(new List<int>());
        }
    }
}
