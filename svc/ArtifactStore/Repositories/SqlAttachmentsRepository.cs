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

    public class SqlAttachmentsRepository : IAttachmentsRepository
    {
        internal readonly ISqlConnectionWrapper ConnectionWrapper;
        private readonly IUsersRepository UserRepository;
        public SqlAttachmentsRepository()
            : this(new SqlConnectionWrapper(ServiceConstants.RaptorMain), new SqlUsersRepository())
        {
        }

        internal SqlAttachmentsRepository(ISqlConnectionWrapper connectionWrapper, IUsersRepository userRepository)
        {
            ConnectionWrapper = connectionWrapper;
            UserRepository = userRepository;
        }

        private async Task<IEnumerable<Attachment>> GetAttachments(int itemId, int userId, bool addDrafts = true)
        {
            var artifactVersionsPrm = new DynamicParameters();
            artifactVersionsPrm.Add("@itemId", itemId);
            artifactVersionsPrm.Add("@userId", userId);
            artifactVersionsPrm.Add("@addDrafts", addDrafts);           
            return await ConnectionWrapper.QueryAsync<Attachment>("GetItemAttachments", artifactVersionsPrm, commandType: CommandType.StoredProcedure);
        }

        private async Task<IEnumerable<DocumentReference>> GetDocumentReferenceArtifacts(int itemId, int userId, bool addDrafts = true)
        {
            var parameters = new DynamicParameters();
            parameters.Add("@itemId", itemId);
            parameters.Add("@userId", userId);
            parameters.Add("@addDrafts", addDrafts);
            return await ConnectionWrapper.QueryAsync<DocumentReference>("GetDocumentReferenceArtifacts", parameters, commandType: CommandType.StoredProcedure);
        }

        private async Task<IEnumerable<LinkedArtifactInfo>> GetDocumentArtifactInfos(IEnumerable<int> artifactIds, int userId, bool addDrafts = true)
        {
            var parameters = new DynamicParameters();            
            var artifactIdsTable = DapperHelper.GetIntCollectionTableValueParameter(artifactIds);
            parameters.Add("@artifactIds", artifactIdsTable);
            parameters.Add("@userId", userId);
            parameters.Add("@addDrafts", addDrafts);
            return await ConnectionWrapper.QueryAsync<LinkedArtifactInfo>("GetDocumentArtifactInfos", parameters, commandType: CommandType.StoredProcedure);
        }

        public async Task<FilesInfo> GetAttachmentsAndDocumentReferences(int artifactId, int userId, int? subArtifactId = null, bool addDrafts = true)
        {
            var itemId = artifactId;
            if (subArtifactId.HasValue)
            {
                itemId = subArtifactId.Value;
            }
            var attachments = (await GetAttachments(itemId, userId, addDrafts)).ToList();
            var referencedArtifacts = (await GetDocumentReferenceArtifacts(itemId, userId, addDrafts)).ToList();
            var referencedArtifactIds = referencedArtifacts.Select(a => a.ArtifactId).Distinct().ToList();

            var documentReferenceArtifactInfos = (await GetDocumentArtifactInfos(referencedArtifactIds, userId, addDrafts)).ToList();
            var documentReferenceArtifactInfoDictionary = documentReferenceArtifactInfos.ToDictionary(a => a.ArtifactId);

            var distinctUsers = attachments.Select(a => a.UserId).Union(referencedArtifacts.Select(b=>b.UserId)).Distinct().ToList();
            var userInfoDictionary = (await UserRepository.GetUserInfos(distinctUsers)).ToDictionary(a => a.UserId);            

            foreach (var attachment in attachments)
            {
                UserInfo userInfo;
                userInfoDictionary.TryGetValue(attachment.UserId, out userInfo);
                attachment.UserName = userInfo.DisplayName;
                attachment.UploadedDate = DateTime.SpecifyKind(attachment.UploadedDate, DateTimeKind.Utc);
            }

            foreach (var referencedArtifact in referencedArtifacts)
            {
                UserInfo userInfo;
                userInfoDictionary.TryGetValue(referencedArtifact.UserId, out userInfo);
                LinkedArtifactInfo linkedArtifactInfo;
                documentReferenceArtifactInfoDictionary.TryGetValue(referencedArtifact.ArtifactId, out linkedArtifactInfo);
                referencedArtifact.UserName = userInfo.DisplayName;
                referencedArtifact.ArtifactName = linkedArtifactInfo.ArtifactName;
                referencedArtifact.ItemTypePrefix = linkedArtifactInfo.ItemTypePrefix;
                referencedArtifact.ReferencedDate = DateTime.SpecifyKind(referencedArtifact.ReferencedDate, DateTimeKind.Utc);
            }

            var result = new FilesInfo(attachments, referencedArtifacts)
            {
                ArtifactId = artifactId,
                SubartifactId = subArtifactId
            };
            return result;
        }

    }
}