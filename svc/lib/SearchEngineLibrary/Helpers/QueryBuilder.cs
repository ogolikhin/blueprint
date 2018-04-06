using ServiceLibrary.Helpers;
using ServiceLibrary.Models;

namespace SearchEngineLibrary.Helpers
{
    public static class QueryBuilder
    {
        /// <param name="id">Collection Id.</param>
        /// <param name="projectId"></param>
        /// <param name="pagination">Used to specify pagination settings. If pagintaion is not needed, than null value should be passed.</param>
        /// <param name="includeDraft">Specifies whether draft versions should be included into search results.</param>
        /// <param name="userId">userId is required for permissions check and getting of draft.</param>
        public static string GetCollectionContentSearchArtifactResults(int id, int projectId, Pagination pagination, bool includeDraft, int userId)
        {
            const int maxRevision = int.MaxValue;

            var partWhereInQuery = includeDraft ?
                I18NHelper.FormatInvariant(
                    @"AND (col.[EndRevision] = {0} OR (col.[StartRevision] = 1 AND col.[VersionUserId] = {1}))
                      AND (iv.[EndRevision] = {0} OR (iv.[StartRevision] = 1 AND iv.[VersionUserId] = {1}))
                      GROUP BY col.[VersionArtifactId] HAVING MIN(col.[EndRevision]) > 0 AND MIN(iv.[EndRevision]) > 0 ",
                    maxRevision, userId) :
                I18NHelper.FormatInvariant(@"AND col.[EndRevision] = {0} AND iv.[EndRevision] = {0} ", maxRevision);

            var paginationOffset = pagination != null ?
                I18NHelper.FormatInvariant("OFFSET {0} ROWS FETCH NEXT {1} ROWS ONLY", pagination.Offset, pagination.Limit) :
                string.Empty;

            var query = I18NHelper.FormatInvariant(@"
                CREATE TABLE #VersionArtifactId ([Id] INT IDENTITY(1, 1), [VersionArtifactId] INT)

                INSERT INTO #VersionArtifactId
                SELECT 
                    col.[VersionArtifactId]
                FROM    [dbo].[CollectionAssignmentVersions] AS col
                JOIN    [dbo].[ItemVersions] AS iv
                    ON  iv.[HolderId] = col.[VersionArtifactId]
                    AND iv.[VersionProjectId] = {0}
                CROSS APPLY [dbo].[Getartifactpermission]({1}, iv.[VersionProjectId], col.[VersionArtifactId]) AS p
                WHERE [VersionCollectionId] = {2} AND p.[Perm] = 1 {3}

                SELECT COUNT([VersionArtifactId]) AS Total
                FROM #VersionArtifactId

                SELECT DISTINCT([VersionArtifactId])
                FROM #VersionArtifactId
                ORDER BY [VersionArtifactId]
                {4}

                DROP TABLE #VersionArtifactId",
                projectId, userId, id, partWhereInQuery, paginationOffset);

            return query;
        }
    }
}
