using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ArtifactStore.Models;
using ServiceLibrary.Repositories;
using ServiceLibrary.Helpers;

namespace ArtifactStore.Repositories
{
    public class SqlArtifactRepository : ISqlArtifactRepository
    {
        internal readonly ISqlConnectionWrapper _connectionWrapper;

        public SqlArtifactRepository()
            : this(new SqlConnectionWrapper(ServiceConstants.RaptorMain))
        {
        }

        internal SqlArtifactRepository(ISqlConnectionWrapper connectionWrapper)
        {
            _connectionWrapper = connectionWrapper;
        }

        public Task<List<Artifact>> GetArtifactChildrenAsync(int projectId, int artifactId)
        {
            throw new NotImplementedException();
        }

        public Task<List<Artifact>> GetProjectChildrenAsync(int projectId)
        {
            throw new NotImplementedException();
        }
    }
}