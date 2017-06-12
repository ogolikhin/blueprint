using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using ServiceLibrary.Helpers;
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

        /// <summary>
        /// Checks whether the user has permission for this artifact. 
        /// if a revision Id is provided, the artifact's revision has to be less than the current revision.
        /// If the artifact is not a regular artifact type then we throw a non-supported exception.
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="artifactId"></param>
        /// <param name="revisionId"></param>
        /// <param name="permissions"></param>
        /// <returns></returns>
        protected async Task CheckForArtifactPermissions(int userId, int artifactId, int revisionId = int.MaxValue, RolePermissions permissions = RolePermissions.Read)
        {
            var artifactBasicDetails = await GetArtifactBasicDetails(ConnectionWrapper, artifactId, userId);
            if (artifactBasicDetails == null)
            {
                ExceptionHelper.ThrowArtifactNotFoundException(artifactId);
            }

            if (!((ItemTypePredefined)artifactBasicDetails.PrimitiveItemTypePredefined).IsRegularArtifactType())
            {
                ExceptionHelper.ThrowArtifactDoesNotSupportOperation(artifactId);
            }

            var artifactsPermissions =
                await ArtifactPermissionsRepository.GetArtifactPermissions(new List<int> { artifactId }, userId);

            if (!artifactsPermissions.ContainsKey(artifactId) ||
                !artifactsPermissions[artifactId].HasFlag(permissions))
            {
                ExceptionHelper.ThrowArtifactForbiddenException(artifactId);
            }
        }
    }
}
