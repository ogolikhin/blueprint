﻿using ArtifactStore.Helpers;
using ArtifactStore.Models;
using Dapper;
using ServiceLibrary.Exceptions;
using ServiceLibrary.Helpers;
using ServiceLibrary.Models;
using ServiceLibrary.Repositories;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;

namespace ArtifactStore.Repositories
{

    public class SqlAttachmentsRepository : IAttachmentsRepository
    {
        internal readonly ISqlConnectionWrapper ConnectionWrapper;
        private readonly IUsersRepository UserRepository;
        private readonly ISqlItemInfoRepository ItemInfoRepository;

        public SqlAttachmentsRepository()
            : this(new SqlConnectionWrapper(ServiceConstants.RaptorMain), new SqlUsersRepository())
        {
        }

        internal SqlAttachmentsRepository(ISqlConnectionWrapper connectionWrapper, IUsersRepository userRepository)
        {
            ConnectionWrapper = connectionWrapper;
            UserRepository = userRepository;
            ItemInfoRepository = new SqlItemInfoRepository(connectionWrapper);
        }

        private async Task<IEnumerable<Attachment>> GetAttachments(int itemId, int userId, int revisionId = int.MaxValue, bool addDrafts = true)
        {
            var artifactVersionsPrm = new DynamicParameters();
            artifactVersionsPrm.Add("@itemId", itemId);
            artifactVersionsPrm.Add("@userId", userId);
            artifactVersionsPrm.Add("@revisionId", revisionId);
            artifactVersionsPrm.Add("@addDrafts", addDrafts);           
            return await ConnectionWrapper.QueryAsync<Attachment>("GetItemAttachments", artifactVersionsPrm, commandType: CommandType.StoredProcedure);
        }

        private async Task<IEnumerable<DocumentReference>> GetDocumentReferenceArtifacts(int itemId, int userId, int revisionId = int.MaxValue, bool addDrafts = true)
        {
            var parameters = new DynamicParameters();
            parameters.Add("@itemId", itemId);
            parameters.Add("@userId", userId);
            parameters.Add("@revisionId", revisionId);
            parameters.Add("@addDrafts", addDrafts);
            return await ConnectionWrapper.QueryAsync<DocumentReference>("GetDocumentReferenceArtifacts", parameters, commandType: CommandType.StoredProcedure);
        }

        private async Task<IEnumerable<LinkedArtifactInfo>> GetDocumentArtifactInfos(IEnumerable<int> artifactIds, int userId, int revisionId = int.MaxValue, bool addDrafts = true)
        {
            var parameters = new DynamicParameters();            
            var artifactIdsTable = SqlConnectionWrapper.ToDataTable(artifactIds, "Int32Collection", "Int32Value");
            parameters.Add("@artifactIds", artifactIdsTable);
            parameters.Add("@userId", userId);
            parameters.Add("@revisionId", revisionId);
            parameters.Add("@addDrafts", addDrafts);
            return await ConnectionWrapper.QueryAsync<LinkedArtifactInfo>("GetDocumentArtifactInfos", parameters, commandType: CommandType.StoredProcedure);
        }

        public async Task<FilesInfo> GetAttachmentsAndDocumentReferences(
            int artifactId, 
            int userId, 
            int? versionId = null, 
            int? subArtifactId = null, 
            bool addDrafts = true, 
            int? baselineId = null)
        {
            var itemId = artifactId;
            if (subArtifactId.HasValue)
            {
                itemId = subArtifactId.Value;
            }
            var revisionId = await ItemInfoRepository.GetRevisionId(artifactId, userId, versionId, baselineId);
            if (baselineId != null)
            {
                addDrafts = false;
            }

            var attachments = (await GetAttachments(itemId, userId, revisionId, addDrafts)).ToList();
            var referencedArtifacts = (await GetDocumentReferenceArtifacts(itemId, userId, revisionId, addDrafts)).ToList();
            var referencedArtifactIds = referencedArtifacts.Select(a => a.ArtifactId).Distinct().ToList();

            var documentReferenceArtifactInfos = (await GetDocumentArtifactInfos(referencedArtifactIds, userId, revisionId, addDrafts)).ToList();
            var documentReferenceArtifactInfoDictionary = documentReferenceArtifactInfos.ToDictionary(a => a.ArtifactId);

            var distinctUsers = attachments.Select(a => a.UserId).Union(referencedArtifacts.Select(b=>b.UserId)).Distinct().ToList();
            var userInfoDictionary = (await UserRepository.GetUserInfos(distinctUsers)).ToDictionary(a => a.UserId);

            var referenceArtifactsToBeRemoved = new List<DocumentReference>();     

            foreach (var attachment in attachments)
            {
                UserInfo userInfo;
                userInfoDictionary.TryGetValue(attachment.UserId, out userInfo);
                attachment.UserName = userInfo.DisplayName;
                attachment.UploadedDate = attachment.UploadedDate == null ? (DateTime?)null : DateTime.SpecifyKind(attachment.UploadedDate.Value, DateTimeKind.Utc);
            }

            foreach (var referencedArtifact in referencedArtifacts)
            {
                UserInfo userInfo;
                userInfoDictionary.TryGetValue(referencedArtifact.UserId, out userInfo);
                LinkedArtifactInfo linkedArtifactInfo;
                documentReferenceArtifactInfoDictionary.TryGetValue(referencedArtifact.ArtifactId, out linkedArtifactInfo);
                if(linkedArtifactInfo != null)
                {
                    referencedArtifact.UserName = userInfo.DisplayName;
                    referencedArtifact.ArtifactName = linkedArtifactInfo.ArtifactName;
                    referencedArtifact.ItemTypePrefix = linkedArtifactInfo.ItemTypePrefix;
                    if (referencedArtifact.ReferencedDate != null)
                    {
                        referencedArtifact.ReferencedDate = DateTime.SpecifyKind(referencedArtifact.ReferencedDate.Value, DateTimeKind.Utc);
                    }
                }
                else
                {
                    // if the linked artifactInfo is not available, do not return this artifact references
                    referenceArtifactsToBeRemoved.Add(referencedArtifact);
                }
            }

            referencedArtifacts = referencedArtifacts.Except(referenceArtifactsToBeRemoved).ToList();

            var result = new FilesInfo(attachments, referencedArtifacts)
            {
                ArtifactId = artifactId,
                SubartifactId = subArtifactId
            };
            return result;
        }
    }
}