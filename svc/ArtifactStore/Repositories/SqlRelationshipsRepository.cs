using ArtifactStore.Models;
using ServiceLibrary.Helpers;
using ServiceLibrary.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace ArtifactStore.Repositories
{
    public class SqlRelationshipsRepository: IRelationshipsRepository
    {
        internal readonly ISqlConnectionWrapper ConnectionWrapper;
        public SqlRelationshipsRepository()
            : this(new SqlConnectionWrapper(ServiceConstants.RaptorMain))
        {
        }
        internal SqlRelationshipsRepository(ISqlConnectionWrapper connectionWrapper)
        {
            ConnectionWrapper = connectionWrapper;
        }

        public async Task<RelationshipResultSet> GetRelationships(int artifactId, int? subArtifactId = null, bool addDrafts = true)
        {



            return (await Task.FromResult(new RelationshipResultSet()));
        }
    }
}