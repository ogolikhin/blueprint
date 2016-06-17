using ArtifactStore.Helpers;
using ArtifactStore.Models;
using Dapper;
using ServiceLibrary.Helpers;
using ServiceLibrary.Models;
using ServiceLibrary.Repositories;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace ArtifactStore.Repositories
{
    public class SqlAttachmentsRepository : ISqlAttachmentsRepository
    {
        internal readonly ISqlConnectionWrapper ConnectionWrapper;
        public SqlAttachmentsRepository()
            : this(new SqlConnectionWrapper(ServiceConstants.RaptorMain))
        {
        }

        internal SqlAttachmentsRepository(ISqlConnectionWrapper connectionWrapper)
        {
            ConnectionWrapper = connectionWrapper;
        }

        private async Task<IEnumerable<Attachment>> GetAttachments(int itemId, int userId, bool addDrafts = true)
        {
            var artifactVersionsPrm = new DynamicParameters();
            artifactVersionsPrm.Add("@itemId", itemId);
            artifactVersionsPrm.Add("@userId", userId);
            artifactVersionsPrm.Add("@addDrafts", addDrafts);           
            return await ConnectionWrapper.QueryAsync<Attachment>("GetItemAttachments", artifactVersionsPrm, commandType: CommandType.StoredProcedure);
        }

        public async Task<FilesInfo> GetAttachmentsAndDocumentReferences(int artifactId, int userId, int? subArtifactId = null, bool addDrafts = true)
        {
            var itemId = artifactId;
            if (subArtifactId.HasValue)
            {
                itemId = subArtifactId.Value;
            }
            var attachments = (await GetAttachments(itemId, userId, addDrafts)).ToList();

            var result = new FilesInfo(attachments, new List<DocumentReference>())
            {
                ArtifactId = artifactId,
                SubartifactId = subArtifactId                
            };

            return result;
        }

    }
}