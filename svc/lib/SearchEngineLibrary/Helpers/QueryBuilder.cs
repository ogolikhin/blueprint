using ServiceLibrary.Helpers;
using ServiceLibrary.Models;
using System;
using System.Globalization;

namespace SearchEngineLibrary.Helpers
{
    public class QueryBuilder
    {
        public static string GetCollectionArtifactIds(int scopeId, Pagination pagination, bool includeDraft, int userId)
        {
            var partWhereInQuery = string.Empty;
            if (!includeDraft)
            {
                partWhereInQuery = @"AND col.[EndRevision] = @infinityRevision AND iv.[EndRevision] = @infinityRevision ";
            }
            else
            {
                partWhereInQuery = @"AND (col.[EndRevision] = @infinityRevision OR (col.[StartRevision] = 1 AND (col.[EndRevision] = 1 OR col.[EndRevision] = -1) AND col.[VersionUserId] = @userId))
                                     AND (iv.[EndRevision] = @infinityRevision OR (iv.[StartRevision] = 1 AND (iv.[EndRevision] = 1 OR iv.[EndRevision] = -1) AND iv.[VersionUserId] = @userId)) 
                                     GROUP BY col.[VersionArtifactId] HAVING MIN(col.[EndRevision]) > 0 AND MIN(iv.[EndRevision]) > 0 ";
            }

            var query = I18NHelper.FormatInvariant(@"
                            DECLARE @Offset INT = {0} 
                            DECLARE @Limit INT = {1}                             
                            DECLARE @scopeId INT = {2} 
                            DECLARE @infinityRevision INT = 2147483647 
                            DECLARE @userId INT = {3}
                            CREATE TABLE #VersionArtifactId (id int identity(1,1), VersionArtifactId int)

                            INSERT INTO #VersionArtifactId 
                            SELECT col.[VersionArtifactId] FROM [dbo].[CollectionAssignmentVersions] as col JOIN [dbo].[ItemVersions] as iv on iv.[HolderId] = col.[VersionArtifactId] 
                            CROSS APPLY [dbo].[Getartifactpermission](@userId, iv.VersionProjectId, iv.VersionArtifactId) as p
                            WHERE [VersionCollectionId] = @scopeId AND p.Perm = 1 {4}

                            SELECT COUNT(VersionArtifactId) as Total FROM #VersionArtifactId 
                            SELECT DISTINCT(VersionArtifactId) FROM #VersionArtifactId ORDER BY [VersionArtifactId] OFFSET @Offset ROWS FETCH NEXT @Limit ROWS ONLY
                            DROP TABLE #VersionArtifactId", pagination.Offset, pagination.Limit, scopeId, userId, partWhereInQuery);

            return query;
        }
    }
}
