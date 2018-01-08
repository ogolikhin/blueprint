using ServiceLibrary.Helpers;
using ServiceLibrary.Models;
using ServiceLibrary.Models.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SearchEngineLibrary.Helpers
{
    public class QueryBuilder
    {
        public static string GetArtifactIds(int scopeId, Pagination pagination, bool includeDraft, int userId)
        {
            var query = new StringBuilder(I18NHelper.FormatInvariant("DECLARE @Offset INT = {0} DECLARE @Limit INT = {1} DECLARE @includeDraft BIT = {2} DECLARE @scopeId INT = {3} DECLARE @infinityRevision INT = 2147483647 DECLARE @userId INT = {4} ", pagination.Offset, pagination.Limit, includeDraft ? 1 : 0, scopeId, userId));
            query.Append("CREATE TABLE #VersionArtifactIds (id int identity(1,1), VersionArtifactId int) ");
            query.Append("IF(@includeDraft = 0) BEGIN INSERT INTO #VersionArtifactIds SELECT col.[VersionArtifactId] FROM [dbo].[CollectionAssignmentVersions] as col JOIN [dbo].[ItemVersions] as iv on iv.[HolderId] = col.[VersionArtifactId] CROSS APPLY [dbo].[Getartifactpermission](@userId, iv.VersionProjectId, iv.VersionArtifactId) as p ");
            query.Append("WHERE [VersionCollectionId] = @scopeId AND col.[EndRevision] = @infinityRevision AND iv.[EndRevision] = @infinityRevision AND p.Perm = 1 ");
            query.Append("END ELSE BEGIN INSERT INTO #VersionArtifactIds SELECT col.[VersionArtifactId] FROM [dbo].[CollectionAssignmentVersions] as col JOIN [dbo].[ItemVersions] as iv ON iv.HolderId = col.VersionArtifactId CROSS APPLY [dbo].[Getartifactpermission](@userId, iv.VersionProjectId, iv.VersionArtifactId) as p ");
            query.Append("WHERE col.[VersionCollectionId] = @scopeId AND (col.EndRevision = @infinityRevision OR (col.StartRevision = 1 AND (col.[EndRevision] = 1 OR col.[EndRevision] = -1) AND col.VersionUserId = @userId)) ");
            query.Append("AND (iv.[EndRevision] = @infinityRevision OR (iv.[StartRevision] = 1 AND (iv.[EndRevision] = 1 OR iv.[EndRevision] = -1) AND iv.[VersionUserId] = @userId)) AND p.Perm = 1 ");
            query.Append("GROUP BY col.[VersionArtifactId] HAVING MIN(col.[EndRevision]) > 0 AND MIN(iv.[EndRevision]) > 0 END ");
            query.Append("SELECT COUNT(VersionArtifactId) as Total FROM #VersionArtifactIds SELECT DISTINCT(VersionArtifactId) FROM #VersionArtifactIds ORDER BY [VersionArtifactId] OFFSET @Offset ROWS FETCH NEXT @Limit ROWS ONLY ");
            query.Append("DROP TABLE #VersionArtifactIds ");

            return query.ToString();
        }
    }
}
