using System.Collections.Generic;
using ServiceLibrary.Repositories;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;
using ServiceLibrary.Helpers;
using ServiceLibrary.Models;
using ServiceLibrary.Models.Enums;
using System.Net;
using ServiceLibrary.Exceptions;
using Dapper;
using System.Linq;
using System;

namespace SearchEngineLibrary.Repository
{
    public class SearchEngineRepository: ISearchEngineRepository
    {
        private readonly ISqlConnectionWrapper _connectionWrapper;        

        public SearchEngineRepository()
            : this(new SqlConnectionWrapper(ServiceConstants.RaptorMain))
        {
        }

        internal SearchEngineRepository(ISqlConnectionWrapper connectionWrapper)
        {
            _connectionWrapper = connectionWrapper;           
        }

        public async Task<IEnumerable<int>> GetChildrenArtifactIdsByCollectionId(int scopeId, Pagination pagination, ScopeType scopeType, bool includeDraft, int userId)
        {
            var dynamicParameters = new DynamicParameters();
            dynamicParameters.Add("@userId", userId);
            dynamicParameters.Add("@itemId", scopeId);
            ArtifactBasicDetails artifactBasicDetails = (await _connectionWrapper.QueryAsync<ArtifactBasicDetails>(
                "GetArtifactBasicDetails", dynamicParameters, commandType: CommandType.StoredProcedure)).FirstOrDefault();

            if (artifactBasicDetails == null)
            {
                string errorMessage = I18NHelper.FormatInvariant("Item (Id:{0}) is not found.", scopeId);
                throw new ResourceNotFoundException(errorMessage, ErrorCodes.ResourceNotFound);
            }                                    

            if (artifactBasicDetails.PrimitiveItemTypePredefined != (int)ItemTypePredefined.ArtifactCollection)
            {
                throw new NotImplementedException(ErrorMessages.NotImplementedForNotCollection);
            }

            if (scopeType == ScopeType.Descendants)
            {
                throw new NotImplementedException(ErrorMessages.NotImplementedForDescendantsScopeType);
            }

            var query = String.Format("DECLARE @Offset INT = {0} DECLARE   @Limit INT = {1} DECLARE @includeDraft BIT = {2} DECLARE @scopeId INT = {3} DECLARE @infinityRevision INT = 2147483647 ", pagination.Offset, pagination.Limit, includeDraft ? 1 : 0, scopeId);
            query += "CREATE TABLE #VersionArtifactId (id int identity(1,1), VersionArtifactId int) ";
            query += "INSERT INTO #VersionArtifactId SELECT [VersionArtifactId] FROM [dbo].[CollectionAssignmentVersions] WHERE [VersionCollectionId] = @scopeId AND EndRevision = @infinityRevision ";
            query += "DELETE FROM #VersionArtifactId WHERE VersionArtifactId IN (SELECT [VersionArtifactId] FROM #VersionArtifactId JOIN [dbo].[Items] as it ON VersionArtifactId = it.ItemId WHERE it.DraftVersion_VersionId IS NULL AND it.LatestVersion_VersionId IS NULL) ";
            query += "IF(@includeDraft = 0) BEGIN DELETE FROM #VersionArtifactId WHERE VersionArtifactId IN (SELECT [VersionArtifactId] FROM #VersionArtifactId JOIN [dbo].[Items] as it ON VersionArtifactId = it.ItemId WHERE it.DraftVersion_VersionId IS NOT NULL) END ";
            query += "SELECT[VersionArtifactId] FROM #VersionArtifactId GROUP BY [VersionArtifactId] ORDER BY [VersionArtifactId] OFFSET @Offset ROWS FETCH NEXT @Limit ROWS ONLY ";
            query += "DROP TABLE #VersionArtifactId "; 

            return await _connectionWrapper.QueryAsync<int>(
                @query, commandType:CommandType.Text);
        }
    }
}