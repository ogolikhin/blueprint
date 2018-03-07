using ServiceLibrary.Helpers;
using ServiceLibrary.Models;

namespace SearchEngineLibrary.Helpers
{
    public static class QueryBuilder
    {
        /// <param name="id">Collection Id.</param>
        /// <param name="pagination">Used to specify pagination settings. If pagintaion is not needed, than null value should be passed.</param>
        /// <param name="includeDraft">Specifies whether draft versions should be included into search results.</param>
        /// <param name="userId">userId is required for permissions check and getting of draft.</param>
        public static string GetCollectionContentSearchArtifactResults(int id, Pagination pagination, bool includeDraft, int userId)
        {
            var partWhereInQuery = includeDraft ?
                @"AND (col.[EndRevision] = @infinityRevision OR (col.[StartRevision] = 1 AND (col.[EndRevision] = 1 OR col.[EndRevision] = -1) AND col.[VersionUserId] = @userId))
                    AND (iv.[EndRevision] = @infinityRevision OR (iv.[StartRevision] = 1 AND (iv.[EndRevision] = 1 OR iv.[EndRevision] = -1) AND iv.[VersionUserId] = @userId))
                    GROUP BY col.[VersionArtifactId] HAVING MIN(col.[EndRevision]) > 0 AND MIN(iv.[EndRevision]) > 0 "
                :
                @"AND col.[EndRevision] = @infinityRevision AND iv.[EndRevision] = @infinityRevision ";

            var paginationParams = pagination != null ?
                I18NHelper.FormatInvariant(@"
                    DECLARE @Offset INT = {0}
                    DECLARE @Limit INT = {1}",
                    pagination.Offset, pagination.Limit)
                : string.Empty;

            var paginationOffset = pagination != null ? "OFFSET @Offset ROWS FETCH NEXT @Limit ROWS ONLY" : string.Empty;

            var query = I18NHelper.FormatInvariant(@"
                DECLARE @scopeId INT = {0}
                DECLARE @infinityRevision INT = 2147483647
                DECLARE @userId INT = {1}
                {2}

                CREATE TABLE #VersionArtifactId (id int identity(1,1), VersionArtifactId int)

                INSERT INTO #VersionArtifactId
                SELECT 
                    col.[VersionArtifactId] 
                FROM    [dbo].[CollectionAssignmentVersions] AS col 
                JOIN    [dbo].[ItemVersions] AS iv 
                    ON  iv.[HolderId] = col.[VersionArtifactId]
                CROSS APPLY [dbo].[Getartifactpermission](@userId, iv.[VersionProjectId], col.[VersionArtifactId]) AS p
                WHERE [VersionCollectionId] = @scopeId AND p.[Perm] = 1 {3}

                SELECT COUNT(VersionArtifactId) AS Total 
                FROM #VersionArtifactId

                SELECT DISTINCT(VersionArtifactId)
                FROM #VersionArtifactId 
                ORDER BY [VersionArtifactId]
                {4}

                DROP TABLE #VersionArtifactId",
                id, userId, paginationParams, partWhereInQuery, paginationOffset);

            return query;
        }
    }
}
