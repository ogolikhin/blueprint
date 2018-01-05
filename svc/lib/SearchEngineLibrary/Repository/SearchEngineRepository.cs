using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using ServiceLibrary.Helpers;
using ServiceLibrary.Models;
using ServiceLibrary.Models.Enums;
using System.Net;
using ServiceLibrary.Exceptions;
using Dapper;
using System.Linq;
using System;
using ServiceLibrary.Repositories;
using System.Text;
using SearchEngineLibrary.Model;
using System.Data.SqlClient;

namespace SearchEngineLibrary.Repository
{
    public class SearchEngineRepository : ISearchEngineRepository
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

        public async Task<SearchArtifactsResult> GetArtifactIds(int scopeId, Pagination pagination, ScopeType scopeType, bool includeDraft, int userId)
        {
            var searchArtifactsResult = new SearchArtifactsResult() { ArtifactIds = new List<int>() };

            var query = new StringBuilder(I18NHelper.FormatInvariant("DECLARE @Offset INT = {0} DECLARE @Limit INT = {1} DECLARE @includeDraft BIT = {2} DECLARE @scopeId INT = {3} DECLARE @infinityRevision INT = 2147483647 DECLARE @userId INT = {4} ", pagination.Offset, pagination.Limit, includeDraft ? 1 : 0, scopeId, userId));
            query.Append("CREATE TABLE #VersionArtifactId (id int identity(1,1), VersionArtifactId int) ");
            query.Append("IF(@includeDraft = 0) BEGIN INSERT INTO #VersionArtifactId SELECT col.[VersionArtifactId] FROM [dbo].[CollectionAssignmentVersions] as col JOIN [dbo].[ItemVersions] as iv on iv.[HolderId] = col.[VersionArtifactId] ");
            query.Append("WHERE [VersionCollectionId] = @scopeId AND col.VersionArtifactId IN (SELECT ArtifactId FROM [dbo].[SearchItems]) AND col.[EndRevision] = @infinityRevision AND iv.[EndRevision] = @infinityRevision ");
            query.Append("END ELSE BEGIN INSERT INTO #VersionArtifactId SELECT col.[VersionArtifactId] FROM [dbo].[CollectionAssignmentVersions] as col JOIN [dbo].[ItemVersions] as iv ON iv.HolderId = col.VersionArtifactId ");
            query.Append("WHERE col.[VersionCollectionId] = @scopeId AND (col.EndRevision = @infinityRevision OR (col.StartRevision = 1 AND col.EndRevision = 1 AND col.VersionUserId = @userId)) ");
            query.Append("AND (iv.[EndRevision] = @infinityRevision OR (iv.[StartRevision] = 1 AND (iv.[EndRevision] = 1 OR iv.[EndRevision] = -1) AND iv.[VersionUserId] = @userId)) ");
            query.Append("AND col.[VersionArtifactId] IN (SELECT [ArtifactId] FROM [dbo].[SearchItems]) GROUP BY col.[VersionArtifactId] HAVING MIN(col.[EndRevision]) > 0 AND MIN(iv.[EndRevision]) > 0 END ");
            query.Append("SELECT COUNT(VersionArtifactId) as Total FROM #VersionArtifactId SELECT DISTINCT(VersionArtifactId) FROM #VersionArtifactId ORDER BY [VersionArtifactId] OFFSET @Offset ROWS FETCH NEXT @Limit ROWS ONLY ");
            query.Append("DROP TABLE #VersionArtifactId "); 
            
            var dbConnection = _connectionWrapper.CreateConnection();
            var reader = await dbConnection.ExecuteReaderAsync(query.ToString(), commandType: CommandType.Text);

            while (reader.Read())
            {
                searchArtifactsResult.Total = (int)reader["Total"];
            }

            reader.NextResult();

            while (reader.Read())
            {
                searchArtifactsResult.ArtifactIds = searchArtifactsResult.ArtifactIds.Union(Enumerable.Repeat((int)reader["VersionArtifactId"], 1));
            }

            return searchArtifactsResult;
        }
    }
}
