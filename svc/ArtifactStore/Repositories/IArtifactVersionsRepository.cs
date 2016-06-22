﻿using ArtifactStore.Models;
using System.Threading.Tasks;

namespace ArtifactStore.Repositories
{
    public interface IArtifactVersionsRepository
    {
        Task<ArtifactHistoryResultSet> GetArtifactVersions(int artifactId, int limit, int offset, int? userId, bool asc, int sessionUserId);

        Task<bool> IsItemDeleted(int itemId);

        Task<DeletedItemInfo> GetDeletedItemInfo(int itemId);

    }
}