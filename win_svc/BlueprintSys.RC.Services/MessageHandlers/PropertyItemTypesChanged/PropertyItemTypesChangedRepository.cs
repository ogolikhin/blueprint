using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using ServiceLibrary.Repositories;

namespace BlueprintSys.RC.Services.MessageHandlers.PropertyItemTypesChanged
{
    public interface IPropertyItemTypesChangedRepository : IBaseRepository
    {
        /// <summary>
        /// Calls the stored procedure [dbo].[GetSearchItemsItemTypesChangeArtifactIds]
        /// </summary>
        Task<List<int>> GetAffectedArtifactIdsForItemTypes(IEnumerable<int> itemTypeIds, bool isInstance, int revisionId);

        /// <summary>
        /// Calls the stored procedure [dbo].[GetSearchItemsPropertyTypesChangeArtifactIds]
        /// </summary>
        Task<List<int>> GetAffectedArtifactIdsForPropertyTypes(IEnumerable<int> propertyTypeIds, bool isInstance, int revisionId);
    }

    public class PropertyItemTypesChangedRepository : BaseRepository, IPropertyItemTypesChangedRepository
    {
        public PropertyItemTypesChangedRepository(string connectionString) : base(connectionString)
        {
        }

        public async Task<List<int>> GetAffectedArtifactIdsForItemTypes(IEnumerable<int> itemTypeIds, bool isInstance, int revisionId)
        {
            var param = new DynamicParameters();
            param.Add("@itemTypeIds", SqlConnectionWrapper.ToDataTable(itemTypeIds ?? new int[0]));
            param.Add("@instance", isInstance);
            param.Add("@changeRevisionId", revisionId);
            return (await ConnectionWrapper.QueryAsync<int>("[dbo].[GetSearchItemsItemTypesChangeArtifactIds]", param, commandType: CommandType.StoredProcedure)).ToList();
        }

        public async Task<List<int>> GetAffectedArtifactIdsForPropertyTypes(IEnumerable<int> propertyTypeIds, bool isInstance, int revisionId)
        {
            var param = new DynamicParameters();
            param.Add("@propertyTypeIds", SqlConnectionWrapper.ToDataTable(propertyTypeIds ?? new int[0]));
            param.Add("@instance", isInstance);
            param.Add("@changeRevisionId", revisionId);
            return (await ConnectionWrapper.QueryAsync<int>("[dbo].[GetSearchItemsPropertyTypesChangeArtifactIds]", param, commandType: CommandType.StoredProcedure)).ToList();
        }
    }
}
