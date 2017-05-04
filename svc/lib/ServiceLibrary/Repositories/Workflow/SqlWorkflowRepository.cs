using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Dapper;
using ServiceLibrary.Helpers;
using ServiceLibrary.Models;
using ServiceLibrary.Models.Workflow;

namespace ServiceLibrary.Repositories.Workflow
{
    public class SqlWorkflowRepository : SqlBaseArtifactRepository, ISqlWorkflowRepository
    {
        public SqlWorkflowRepository()
            : this(new SqlConnectionWrapper(ServiceConstants.RaptorMain))
        {
        }

        public SqlWorkflowRepository(ISqlConnectionWrapper connectionWrapper)
            : this(connectionWrapper, new SqlArtifactPermissionsRepository(connectionWrapper))
        {
        }

        public SqlWorkflowRepository(ISqlConnectionWrapper connectionWrapper,
            IArtifactPermissionsRepository artifactPermissionsRepository) 
            : base(connectionWrapper,artifactPermissionsRepository)
        {
        }

        #region artifact workflow
        public async Task<IEnumerable<Transitions>> GetTransitions(int artifactId, int userId)
        {
            var artifactBasicDetails = await GetArtifactBasicDetails(ConnectionWrapper, artifactId, userId);
            if (artifactBasicDetails == null)
            {
                ExceptionHelper.ThrowArtifactNotFoundException(artifactId);
            }

            var artifactsPermissions =
                await ArtifactPermissionsRepository.GetArtifactPermissions(new List<int> { artifactId }, userId);

            if (!artifactsPermissions.ContainsKey(artifactId) || !artifactsPermissions[artifactId].HasFlag(RolePermissions.Read))
            {
                ExceptionHelper.ThrowArtifactForbiddenException(artifactId);
            }
            return await GetTransitionsInternal(artifactId, userId);
        }

        private async Task<IEnumerable<Transitions>> GetTransitionsInternal(int artifactId, int userId)
        {
            var param = new DynamicParameters();
            param.Add("@artifactId", artifactId);
            param.Add("@userId", userId);

            return await ConnectionWrapper.QueryAsync<Transitions>("GetAvailableTransitions", param, commandType: CommandType.StoredProcedure);
        }
        #endregion
    }
}
