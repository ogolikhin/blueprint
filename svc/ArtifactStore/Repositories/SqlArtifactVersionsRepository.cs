using ArtifactStore.Models;
using ServiceLibrary.Helpers;
using ServiceLibrary.Repositories;

namespace ArtifactStore.Repositories
{
    public class SqlArtifactVersionsRepository : ISqlArtifactVersionsRepository
    {
        internal readonly ISqlConnectionWrapper ConnectionWrapper;

        public SqlArtifactVersionsRepository()
            : this(new SqlConnectionWrapper(ServiceConstants.RaptorMain))
        {
        }

        internal SqlArtifactVersionsRepository(ISqlConnectionWrapper connectionWrapper)
        {
            ConnectionWrapper = connectionWrapper;
        }
        public ArtifactHistoryResultSet GetArtifactVersions(int artifactId, int limit, int offset, int userId, bool asc)
        {
            return new ArtifactHistoryResultSet();
        }
    }
}