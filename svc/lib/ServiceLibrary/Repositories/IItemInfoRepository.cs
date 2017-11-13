﻿using System.Collections.Generic;
using System.Threading.Tasks;
using ServiceLibrary.Models;

namespace ServiceLibrary.Repositories
{
    public interface IItemInfoRepository
    {
        Task<IEnumerable<ItemLabel>> GetItemsLabels(int userId, IEnumerable<int> itemIds, bool addDrafts = true, int revisionId = int.MaxValue);

        Task<IEnumerable<ItemDetails>> GetItemsDetails(int userId, IEnumerable<int> itemIds, bool addDrafts = true, int revisionId = int.MaxValue);

        Task<IEnumerable<ItemRawDataCreatedDate>> GetItemsRawDataCreatedDate(int userId, IEnumerable<int> itemIds, bool addDrafts = true, int revisionId = int.MaxValue);

        Task<int> GetRevisionId(int artifactId, int userId, int? versionId = null, int? baselineId = null);

        Task<int> GetRevisionIdFromBaselineId(int baselineId, int userId, bool addDrafts = true, int revisionId = int.MaxValue);

        Task<int> GetTopRevisionId(int userId);

        Task<string> GetItemDescription(int itemId, int userId, bool? addDrafts = true, int? revisionId = int.MaxValue);

        Task<ISet<int>> GetBaselineArtifacts(int baselineId, int userId, bool addDrafts = true, int revisionId = int.MaxValue);
    }
}