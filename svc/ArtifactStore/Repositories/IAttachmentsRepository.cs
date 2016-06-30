﻿using ArtifactStore.Models;
using System.Threading.Tasks;

namespace ArtifactStore.Repositories
{
    public interface IAttachmentsRepository
    {
        Task<FilesInfo> GetAttachmentsAndDocumentReferences(int artifactId, int userId, int? subArtifactId = null,
            bool addDrafts = true);
    }
}