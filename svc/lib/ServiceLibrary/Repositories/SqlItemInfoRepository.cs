﻿using BluePrintSys.RC.Service.Business.Baselines.Impl;
using Dapper;
using ServiceLibrary.Exceptions;
using ServiceLibrary.Helpers;
using ServiceLibrary.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlTypes;
using System.Linq;
using System.Threading.Tasks;

namespace ServiceLibrary.Repositories
{
    public class SqlItemInfoRepository : IItemInfoRepository
    {
        private readonly ISqlConnectionWrapper _connectionWrapper;

        private readonly IArtifactPermissionsRepository _artifactPermissionsRepository;

        public SqlItemInfoRepository()
            : this(new SqlConnectionWrapper(ServiceConstants.RaptorMain))
        {
        }

        public SqlItemInfoRepository(ISqlConnectionWrapper connectionWrapper)
        {
            _connectionWrapper = connectionWrapper;
            _artifactPermissionsRepository = new SqlArtifactPermissionsRepository(connectionWrapper);
        }

        public async Task<IEnumerable<ItemLabel>> GetItemsLabels(int userId, IEnumerable<int> itemIds, bool addDrafts = true, int revisionId = int.MaxValue)
        {
            var parameters = new DynamicParameters();
            parameters.Add("@itemIds", SqlConnectionWrapper.ToDataTable(itemIds));
            parameters.Add("@userId", userId);
            parameters.Add("@addDrafts", addDrafts);
            parameters.Add("@revisionId", revisionId);

            return (await _connectionWrapper.QueryAsync<ItemLabel>("GetItemsLabels", parameters, commandType: CommandType.StoredProcedure));
        }

        public async Task<IEnumerable<ItemDetails>> GetItemsDetails(int userId, IEnumerable<int> itemIds, bool addDrafts = true, int revisionId = int.MaxValue, IDbTransaction transaction = null)
        {
            if (itemIds.IsEmpty())
            {
                return Enumerable.Empty<ItemDetails>();
            }

            var parameters = new DynamicParameters();
            parameters.Add("@userId", userId);
            parameters.Add("@itemIds", SqlConnectionWrapper.ToDataTable(itemIds));
            parameters.Add("@addDrafts", addDrafts);
            parameters.Add("@revisionId", revisionId);

            if (transaction == null)
            {
                return await _connectionWrapper.QueryAsync<ItemDetails>("GetItemsDetails", parameters, commandType: CommandType.StoredProcedure);
            }

            return await transaction.Connection.QueryAsync<ItemDetails>("GetItemsDetails", parameters, transaction, commandType: CommandType.StoredProcedure);
        }

        public async Task<string> GetItemDescription(int itemId, int userId, bool? addDrafts = true, int? revisionId = int.MaxValue)
        {
            // SP [GetItemDescription] returns last published version for deleted items when revisionId is NULL.
            var parameters = new DynamicParameters();
            parameters.Add("@itemId", itemId);
            parameters.Add("@userId", userId);
            parameters.Add("@addDrafts", addDrafts);
            parameters.Add("@revisionId", revisionId);

            return (await _connectionWrapper.QueryAsync<string>("GetItemDescription", parameters, commandType: CommandType.StoredProcedure)).SingleOrDefault();
        }

        public async Task<IEnumerable<ItemRawDataCreatedDate>> GetItemsRawDataCreatedDate(int userId, IEnumerable<int> itemIds, bool addDrafts = true, int revisionId = int.MaxValue)
        {
            var parameters = new DynamicParameters();
            parameters.Add("@itemIds", SqlConnectionWrapper.ToDataTable(itemIds));
            parameters.Add("@userId", userId);
            parameters.Add("@addDrafts", addDrafts);
            parameters.Add("@revisionId", revisionId);

            return (await _connectionWrapper.QueryAsync<ItemRawDataCreatedDate>("GetItemsRawDataCreatedDate", parameters, commandType: CommandType.StoredProcedure));
        }

        private async Task<int> GetRevisionIdByVersionIndex(int artifactId, int versionIndex)
        {
            var parameters = new DynamicParameters();
            parameters.Add("@artifactId", artifactId);
            parameters.Add("@versionIndex", versionIndex);

            return (await _connectionWrapper.QueryAsync<int>("GetRevisionIdByVersionIndex", parameters, commandType: CommandType.StoredProcedure)).SingleOrDefault();
        }

        public async Task<int> GetRevisionId(int artifactId, int userId, int? versionId = null, int? baselineId = null)
        {
            var revisionId = int.MaxValue;

            if (versionId != null)
            {
                if (versionId < 0)
                {
                    throw new BadRequestException($"Version Id out of range.", ErrorCodes.OutOfRangeParameter);
                }
                revisionId = await GetRevisionIdByVersionIndex(artifactId, versionId.Value);
            }
            else if (baselineId != null)
            {
                if (baselineId < 0)
                {
                    throw new BadRequestException($"Baseline Id out of range.", ErrorCodes.OutOfRangeParameter);
                }
                revisionId = await GetRevisionIdFromBaselineId(baselineId.Value, userId);
            }

            if (revisionId <= 0)
            {
                throw new ResourceNotFoundException($"Version Index or Baseline Timestamp is not found.", ErrorCodes.ResourceNotFound);
            }

            return revisionId;
        }

        public async Task<int> GetRevisionIdFromBaselineId(int baselineId, int userId, bool addDrafts = true, int revisionId = int.MaxValue)
        {
            var itemRawData = (await GetItemsRawData(new List<int> { baselineId }, userId, addDrafts, revisionId)).SingleOrDefault();
            if (itemRawData != null)
            {
                var rawData = itemRawData.RawData;
                var snapTime = BaselineRawDataHelper.ExtractTimestamp(rawData);
                if (snapTime != null)
                {
                    return await GetRevisionIdByTime(snapTime.Value);
                }

                return int.MaxValue;
            }

            return -1;
        }

        private async Task<int> GetRevisionIdByTime(DateTime time)
        {
            var utcTime = time.ToUniversalTime();
            var minDateTime = (DateTime)SqlDateTime.MinValue;
            var maxDateTime = (DateTime)SqlDateTime.MaxValue;
            if (utcTime < minDateTime || utcTime > maxDateTime)
            {
                return -1;
            }

            var prm = new DynamicParameters();
            prm.Add("@time", utcTime);
            var queryText = "SELECT MAX([RevisionId]) FROM [dbo].[Revisions] WHERE ([Timestamp] <= @time) AND ([RevisionId] > 1);";

            return (await _connectionWrapper.QueryAsync<int>(queryText, prm)).SingleOrDefault();
        }

        public async Task<ISet<int>> GetBaselineArtifacts(int baselineId, int userId, bool addDrafts = true, int revisionId = int.MaxValue)
        {
            var itemRawData = (await GetItemsRawData(new List<int> { baselineId }, userId, addDrafts, revisionId)).SingleOrDefault();
            if (itemRawData != null)
            {
                var rawData = itemRawData.RawData;
                return BaselineRawDataHelper.ExtractBaselineArtifacts(rawData);
            }

            string errorMessage = I18NHelper.FormatInvariant("Baseline (Id:{0}) is not found.", baselineId);
            throw new ResourceNotFoundException(errorMessage, ErrorCodes.ResourceNotFound);
        }

        private async Task<IEnumerable<ItemRawData>> GetItemsRawData(IEnumerable<int> itemIds, int userId, bool addDrafts = true, int revisionId = int.MaxValue)
        {
            var prm = new DynamicParameters();
            prm.Add("@itemIds", SqlConnectionWrapper.ToDataTable(itemIds));
            prm.Add("@userId", userId);
            prm.Add("@addDrafts", addDrafts);
            prm.Add("@revisionId", revisionId);

            return (await _connectionWrapper.QueryAsync<ItemRawData>("GetItemsRawDataCreatedDate", prm, commandType: CommandType.StoredProcedure));
        }

        public async Task<int> GetTopRevisionId(IDbTransaction transaction)
        {
            const string queryText = "SELECT MAX([RevisionId]) FROM [dbo].[Revisions];";
            int result;
            if (transaction == null)
            {
                result = (await _connectionWrapper.QueryAsync<int>(queryText)).SingleOrDefault();
            }
            else
            {
                result = (await transaction.Connection.QueryAsync<int>(queryText, transaction: transaction)).SingleOrDefault();
            }
            return result;
        }
    }
}
