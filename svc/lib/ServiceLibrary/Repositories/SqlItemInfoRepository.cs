using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using ServiceLibrary.Helpers;
using ServiceLibrary.Models;
using ServiceLibrary.Exceptions;

namespace ServiceLibrary.Repositories
{
    public class SqlItemInfoRepository : ISqlItemInfoRepository
    {
        private readonly ISqlConnectionWrapper _connectionWrapper;
        private readonly IArtifactPermissionsRepository _artifactPermissionsRepository;

        public SqlItemInfoRepository()
            : this(new SqlConnectionWrapper(ServiceConstants.RaptorMain))
        {
        }
        public SqlItemInfoRepository(ISqlConnectionWrapper connectionWrapper)
        {
            _connectionWrapper = connectionWrapper;
            _artifactPermissionsRepository = new SqlArtifactPermissionsRepository(connectionWrapper);
        }
        public async Task<IEnumerable<ItemLabel>> GetItemsLabels(int userId, IEnumerable<int> itemIds, bool addDrafts = true, int revisionId = int.MaxValue)
        {
            var parameters = new DynamicParameters();
            parameters.Add("@itemIds", SqlConnectionWrapper.ToDataTable(itemIds, "Int32Collection", "Int32Value"));
            parameters.Add("@userId", userId);
            parameters.Add("@addDrafts", addDrafts);
            parameters.Add("@revisionId", revisionId);
            return (await _connectionWrapper.QueryAsync<ItemLabel>("GetItemsLabels", parameters, commandType: CommandType.StoredProcedure));
        }

        public async Task<IEnumerable<ItemDetails>> GetItemsDetails(int userId, IEnumerable<int> itemIds, bool addDrafts = true, int revisionId = int.MaxValue)
        {
            var parameters = new DynamicParameters();
            parameters.Add("@userId", userId);
            parameters.Add("@itemIds", SqlConnectionWrapper.ToDataTable(itemIds, "Int32Collection", "Int32Value"));
            parameters.Add("@addDrafts", addDrafts);
            parameters.Add("@revisionId", revisionId);
            return await _connectionWrapper.QueryAsync<ItemDetails>("GetItemsDetails", parameters, commandType: CommandType.StoredProcedure);
        }

        public async Task<IEnumerable<ItemRawDataCreatedDate>> GetItemsRawDataCreatedDate(int userId, IEnumerable<int> itemIds, bool addDrafts = true, int revisionId = int.MaxValue)
        {
            var parameters = new DynamicParameters();
            parameters.Add("@itemIds", SqlConnectionWrapper.ToDataTable(itemIds, "Int32Collection", "Int32Value"));
            parameters.Add("@userId", userId);
            parameters.Add("@addDrafts", addDrafts);
            parameters.Add("@revisionId", revisionId);
            return (await _connectionWrapper.QueryAsync<ItemRawDataCreatedDate>("GetItemsRawDataCreatedDate", parameters, commandType: CommandType.StoredProcedure));
        }

        private async Task<int> GetRevisionIdByVersionIndex(int artifactId, int versionIndex)
        {
            var parameters = new DynamicParameters();
            parameters.Add("@artifactId", artifactId);
            parameters.Add("@versionIndex", versionIndex);
            return (await _connectionWrapper.QueryAsync<int>("GetRevisionIdByVersionIndex", parameters, commandType: CommandType.StoredProcedure)).SingleOrDefault();
        }

        public async Task<int> GetRevisionId(int artifactId, int userId, int? versionId = null, int? baselineId = null)
        {
            var revisionId = int.MaxValue;
            if (versionId != null)
            {
                revisionId = await GetRevisionIdByVersionIndex(artifactId, versionId.Value);
            }
            else if (baselineId != null)
            {
                revisionId = await _artifactPermissionsRepository.GetRevisionIdFromBaselineId(baselineId.Value, userId);
            }
            if (revisionId <= 0)
            {
                throw new ResourceNotFoundException($"Version Index or Baseline Timestamp is not found.", ErrorCodes.ResourceNotFound);
            }
            return revisionId;
        }

    }
}