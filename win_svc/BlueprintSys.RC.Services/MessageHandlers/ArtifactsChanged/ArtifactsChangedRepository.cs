using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Dapper;
using ServiceLibrary.Repositories;

namespace BlueprintSys.RC.Services.MessageHandlers.ArtifactsChanged
{
    public interface IArtifactsChangedRepository : IBaseRepository
    {
        /// <summary>
        /// Calls the stored procedure [dbo].[RepopulateSearchItems]
        /// </summary>
        Task<int> RepopulateSearchItems(IEnumerable<int> artifactIds);
    }

    public class ArtifactsChangedRepository : BaseRepository, IArtifactsChangedRepository
    {
        public ArtifactsChangedRepository(string connectionString) : base(connectionString)
        {
        }

        public async Task<int> RepopulateSearchItems(IEnumerable<int> artifactIds)
        {
            var param = new DynamicParameters();
            param.Add("@artifactIds", SqlConnectionWrapper.ToDataTable(artifactIds));
            return await ConnectionWrapper.ExecuteAsync("[dbo].[RepopulateSearchItems]", param, commandType: CommandType.StoredProcedure);
        }
    }
}
