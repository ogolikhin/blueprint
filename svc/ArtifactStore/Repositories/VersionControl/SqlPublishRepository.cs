using ServiceLibrary.Helpers;
using ServiceLibrary.Repositories;

namespace ArtifactStore.Repositories.VersionControl
{
    public class SqlPublishRepository : SqlBaseArtifactRepository
    {
        protected abstract class BaseVersionData
        {
            public int DraftVersionId { get; set; }
            public int? LatestVersionId { get; set; }

            public bool DraftDeleted { get; set; }
        }
        public SqlPublishRepository()
            : this(new SqlConnectionWrapper(ServiceConstants.RaptorMain))
        {
        }

        public SqlPublishRepository(ISqlConnectionWrapper connectionWrapper)
            : this(connectionWrapper, new SqlArtifactPermissionsRepository(connectionWrapper))
        {
        }

        public SqlPublishRepository(ISqlConnectionWrapper connectionWrapper,
            IArtifactPermissionsRepository artifactPermissionsRepository)
            : base(connectionWrapper, artifactPermissionsRepository)
        {
        }
    }
}