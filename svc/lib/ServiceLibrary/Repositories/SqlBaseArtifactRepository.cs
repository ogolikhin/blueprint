using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using ServiceLibrary.Models;

namespace ServiceLibrary.Repositories
{
    public class SqlBaseArtifactRepository
    {
        protected ISqlConnectionWrapper ConnectionWrapper;
        protected IArtifactPermissionsRepository ArtifactPermissionsRepository;

        public SqlBaseArtifactRepository(ISqlConnectionWrapper connectionWrapper,
            IArtifactPermissionsRepository artifactPermissionsRepository)
        {
            ConnectionWrapper = connectionWrapper;
            ArtifactPermissionsRepository = artifactPermissionsRepository;
        }

        protected async Task<ArtifactBasicDetails> GetArtifactBasicDetails(ISqlConnectionWrapper connectionWrapper, int artifactId, int userId)
        {
            var prm = new DynamicParameters();
            prm.Add("@userId", userId);
            prm.Add("@itemId", artifactId);
            return (await connectionWrapper.QueryAsync<ArtifactBasicDetails>(
                "GetArtifactBasicDetails", prm, commandType: CommandType.StoredProcedure)).FirstOrDefault();
        }
    }
}
