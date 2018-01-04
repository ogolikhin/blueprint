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

        public async Task<IEnumerable<int>> GetArtifactIds(int scopeId, Pagination pagination, ScopeType scopeType, bool includeDraft, int userId)
        {
            if (scopeType == ScopeType.Descendants)
            {
                throw new NotImplementedException(ErrorMessages.NotImplementedForDescendantsScopeType);
            }

            var query = new StringBuilder(I18NHelper.FormatInvariant("DECLARE @Offset INT = {0} DECLARE   @Limit INT = {1} DECLARE @includeDraft BIT = {2} DECLARE @scopeId INT = {3} DECLARE @infinityRevision INT = 2147483647 DECLARE @userId INT = {4} ", pagination.Offset, pagination.Limit, includeDraft ? 1 : 0, scopeId, userId));
            query.Insert(query.Length - 1, "CREATE TABLE #VersionArtifactId (id int identity(1,1), VersionArtifactId int) ");
            query.Insert(query.Length - 1, "iF(@includeDraft = 0) BEGIN INSERT INTO #VersionArtifactId SELECT [VersionArtifactId] FROM [dbo].[CollectionAssignmentVersions] as col WHERE [VersionCollectionId] = @scopeId AND EndRevision = @infinityRevision AND col.VersionArtifactId IN (SELECT ArtifactId FROM [dbo].[SearchItems]) END ");
            query.Insert(query.Length - 1, "ELSE BEGIN INSERT INTO #VersionArtifactId SELECT [VersionArtifactId] FROM [dbo].[CollectionAssignmentVersions] as col ");
            query.Insert(query.Length - 1, "WHERE col.[VersionCollectionId] = @scopeId AND (col.EndRevision = @infinityRevision OR (col.StartRevision = 1 AND col.EndRevision = 1 AND col.VersionUserId = @userId)) ");
            query.Insert(query.Length - 1, "AND (SELECT COUNT(col2.VersionArtifactId) FROM [dbo].[CollectionAssignmentVersions] as col2 WHERE col2.VersionArtifactId = col.VersionArtifactId AND col2.StartRevision = 1 AND col2.EndRevision = -1 AND col2.VersionUserId = @userId) = 0 ");
            query.Insert(query.Length - 1, "AND col.VersionArtifactId IN (SELECT ArtifactId FROM [dbo].[SearchItems]) END ");
            query.Insert(query.Length - 1, "iF(@includeDraft = 0) BEGIN DELETE FROM #VersionArtifactId WHERE VersionArtifactId NOT IN (SELECT iv.HolderId FROM [dbo].[ItemVersions] as iv WHERE (iv.EndRevision = @infinityRevision)) END ");
            query.Insert(query.Length - 1, "ELSE BEGIN DELETE FROM #VersionArtifactId WHERE VersionArtifactId IN (SELECT iv.HolderId FROM [dbo].[ItemVersions] as iv WHERE (iv.StartRevision = 1 AND iv.EndRevision = -1 AND iv.VersionUserId = @userId)) END ");          
            query.Insert(query.Length - 1, "SELECT [ArtifactId] FROM [dbo].[SearchItems] WHERE [ArtifactId] IN (SELECT [VersionArtifactId] FROM #VersionArtifactId) GROUP BY [ArtifactId] ORDER BY [ArtifactId] OFFSET @Offset ROWS FETCH NEXT @Limit ROWS ONLY ");
            query.Insert(query.Length - 1, "DROP TABLE #VersionArtifactId "); 

            return await _connectionWrapper.QueryAsync<int>(
                @query.ToString(), commandType:CommandType.Text);
        }
    }
}
