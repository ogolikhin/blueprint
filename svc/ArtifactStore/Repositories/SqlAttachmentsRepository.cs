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

        private async Task<IEnumerable<int>> GetDocumentReferenceArtifacts(int itemId, int userId, bool addDrafts = true)
        {
            var parameters = new DynamicParameters();
            parameters.Add("@itemId", itemId);
            parameters.Add("@userId", userId);
            parameters.Add("@addDrafts", addDrafts);
            return await ConnectionWrapper.QueryAsync<int>("GetDocumentReferenceArtifacts", parameters, commandType: CommandType.StoredProcedure);
        }

        private async Task<IEnumerable<DocumentReference>> GetOnlyDocumentArtifacts(IEnumerable<int> artifactIds, int userId, bool addDrafts = true)
        {
            var parameters = new DynamicParameters();            
            var artifactIdsTable = DapperHelper.GetIntCollectionTableValueParameter(artifactIds);
            parameters.Add("@artifactIds", artifactIdsTable);
            parameters.Add("@userId", userId);
            parameters.Add("@addDrafts", addDrafts);
            return await ConnectionWrapper.QueryAsync<DocumentReference>("GetOnlyDocumentArtifacts", parameters, commandType: CommandType.StoredProcedure);
        }

        public async Task<FilesInfo> GetAttachmentsAndDocumentReferences(int artifactId, int userId, int? subArtifactId = null, bool addDrafts = true)
        {
            var itemId = artifactId;
            if (subArtifactId.HasValue)
            {
                itemId = subArtifactId.Value;
            }
            var attachments = (await GetAttachments(itemId, userId, addDrafts)).ToList();            

            var referenceArtifacts = (await GetDocumentReferenceArtifacts(itemId, userId, addDrafts)).ToList();

            var documentReferenceArtifacts = (await GetOnlyDocumentArtifacts(referenceArtifacts, userId, addDrafts)).ToList();

            var result = new FilesInfo(attachments, documentReferenceArtifacts)
            {
                ArtifactId = artifactId,
                SubartifactId = subArtifactId
            };

            return result;
        }

    }
}