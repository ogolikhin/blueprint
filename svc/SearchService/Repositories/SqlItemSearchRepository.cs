using Dapper;
using SearchService.Helpers;
using SearchService.Models;
using ServiceLibrary.Exceptions;
using ServiceLibrary.Helpers;
using ServiceLibrary.Models;
using ServiceLibrary.Repositories;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace SearchService.Repositories
{
    public class SqlItemSearchRepository : IItemSearchRepository
    {
        internal static readonly DataTable Predefineds = SqlConnectionWrapper.ToDataTable(new[]
        {
            4098,
            4099,
            4115,
            16384
        });

        private static readonly DataTable PrimitiveItemTypePredefineds = SqlConnectionWrapper.ToDataTable(new[]
        {
            (int)ItemTypePredefined.Project,   // (4097)
            (int)ItemTypePredefined.Baseline,  // (4098)
            (int)ItemTypePredefined.DataObject // (32769)
        });

        private readonly ISqlConnectionWrapper _connectionWrapper;
        private readonly ISearchConfigurationProvider _searchConfigurationProvider;
        private readonly IArtifactPermissionsRepository _artifactPermissionsRepository;
        private readonly IArtifactRepository _artifactRepository;

        public SqlItemSearchRepository() : this(new SqlConnectionWrapper(ServiceConstants.RaptorMain), new SearchConfiguration())
        {
        }

        internal SqlItemSearchRepository(ISqlConnectionWrapper connectionWrapper, ISearchConfiguration configuration) : this(connectionWrapper, configuration, new SqlArtifactPermissionsRepository(connectionWrapper), new SqlArtifactRepository(connectionWrapper))
        {
        }

        internal SqlItemSearchRepository(
            ISqlConnectionWrapper connectionWrapper,
            ISearchConfiguration configuration,
            IArtifactPermissionsRepository artifactPermissionsRepository,
            IArtifactRepository artifactRepository)
        {
            _connectionWrapper = connectionWrapper;
            _artifactPermissionsRepository = artifactPermissionsRepository;
            _searchConfigurationProvider = new SearchConfigurationProvider(configuration);
            _artifactRepository = artifactRepository;
        }

        /// <summary>
        /// Perform a full text search
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="searchCriteria">SearchCriteria object</param>
        /// <param name="page">Page Number</param>
        /// <param name="pageSize">Page Size</param>
        /// <returns></returns>
        public async Task<FullTextSearchResultSet> SearchFullText(int userId, FullTextSearchCriteria searchCriteria, int page, int pageSize)
        {
            string sql;
            var param = new DynamicParameters();
            param.Add("@userId", userId);
            param.Add("@query", GetQuery(searchCriteria.Query));
            param.Add("@projectIds", SqlConnectionWrapper.ToDataTable(searchCriteria.ProjectIds));
            param.Add("@predefineds", Predefineds);
            param.Add("@primitiveItemTypePredefineds", PrimitiveItemTypePredefineds);
            param.Add("@page", page);
            param.Add("@pageSize", pageSize);
            param.Add("@maxItems", _searchConfigurationProvider.MaxItems);
            param.Add("@maxSearchableValueStringSize", _searchConfigurationProvider.MaxSearchableValueStringSize);
            if (searchCriteria.ItemTypeIds?.ToArray().Length > 0)
            {
                param.Add("@itemTypeIds", SqlConnectionWrapper.ToDataTable(searchCriteria.ItemTypeIds));
                sql = "SearchFullTextByItemTypes";
            }
            else
            {
                sql = "SearchFullText";
            }

            try
            {
                var items = (await _connectionWrapper.QueryAsync<FullTextSearchResult>(sql, param, commandType: CommandType.StoredProcedure, commandTimeout: _searchConfigurationProvider.SearchTimeout)).ToList();
                return new FullTextSearchResultSet
                {
                    Items = items,
                    Page = page,
                    PageItemCount = items.Count,
                    PageSize = pageSize
                };
            }
            catch (SqlException sqlException)
            {
                switch (sqlException.Number)
                {
                    // Sql timeout error
                    case ErrorCodes.SqlTimeoutNumber:
                        throw new SqlTimeoutException("Server did not respond with a response in the allocated time. Please try again later.", ErrorCodes.Timeout);
                }
                throw;
            }
        }


        /// <summary>
        /// Return metadata for a Full Text Search
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="searchCriteria">SearchCriteria object</param>
        /// <returns>FullTextSearchMetaDataResult</returns>
        public async Task<MetaDataSearchResultSet> FullTextMetaData(int userId, FullTextSearchCriteria searchCriteria)
        {
            string sql;
            var param = new DynamicParameters();
            param.Add("@userId", userId);
            param.Add("@query", GetQuery(searchCriteria.Query));
            param.Add("@projectIds", SqlConnectionWrapper.ToDataTable(searchCriteria.ProjectIds));
            param.Add("@predefineds", Predefineds);
            param.Add("@primitiveItemTypePredefineds", PrimitiveItemTypePredefineds);
            param.Add("@maxItems", _searchConfigurationProvider.MaxItems);
            param.Add("@maxSearchableValueStringSize", _searchConfigurationProvider.MaxSearchableValueStringSize);
            if (searchCriteria.ItemTypeIds?.ToArray().Length > 0)
            {
                param.Add("@itemTypeIds", SqlConnectionWrapper.ToDataTable(searchCriteria.ItemTypeIds));
                sql = "SearchFullTextByItemTypesMetaData";
            }
            else
            {
                sql = "SearchFullTextMetaData";
            }

            try
            {
                var result =
                    await
                        _connectionWrapper.QueryMultipleAsync<MetaDataSearchResult, int?>(sql, param,
                            commandType: CommandType.StoredProcedure,
                            commandTimeout: _searchConfigurationProvider.SearchTimeout);
                return new MetaDataSearchResultSet
                {
                    Items = result.Item1,
                    TotalCount = result.Item2.ElementAt(0) ?? 0,
                };
            }
            catch (SqlException sqlException)
            {
                switch (sqlException.Number)
                {
                    // Sql timeout error
                    case ErrorCodes.SqlTimeoutNumber:
                        throw new SqlTimeoutException("Server did not respond with a response in the allocated time. Please try again later.", ErrorCodes.Timeout);
                }
                throw;
            }
        }

        /// <summary>
        /// Perform an Item search by SearchName
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="searchCriteria">SearchCriteria object</param>
        /// <param name="startOffset">Search start offset</param>
        /// <param name="pageSize">Page Size</param>
        /// <returns></returns>
        public async Task<ItemNameSearchResultSet> SearchName(
            int userId,
            ItemNameSearchCriteria searchCriteria,
            int startOffset,
            int pageSize)
        {
            var param = new DynamicParameters();
            param.Add("@userId", userId);
            param.Add("@query", searchCriteria.Query);
            param.Add("@projectIds", SqlConnectionWrapper.ToDataTable(searchCriteria.ProjectIds));
            if (searchCriteria.PredefinedTypeIds != null && searchCriteria.PredefinedTypeIds.Any())
                param.Add("@predefinedTypeIds", SqlConnectionWrapper.ToDataTable(searchCriteria.PredefinedTypeIds));
            if (searchCriteria.ItemTypeIds != null && searchCriteria.ItemTypeIds.Any())
                param.Add("@itemTypeIds", SqlConnectionWrapper.ToDataTable(searchCriteria.ItemTypeIds));
            param.Add("@excludedPredefineds", SqlConnectionWrapper.ToDataTable(GetExcludedPredefineds(searchCriteria)));
            param.Add("@startOffset", startOffset);
            param.Add("@pageSize", pageSize);
            param.Add("@maxSearchableValueStringSize", _searchConfigurationProvider.MaxSearchableValueStringSize);

            List<ItemNameSearchResult> items;

            try
            {
                items = (await _connectionWrapper.QueryAsync<ItemNameSearchResult>("SearchItemNameByItemTypes",
                    param,
                    commandType: CommandType.StoredProcedure,
                    commandTimeout: _searchConfigurationProvider.SearchTimeout)).ToList();
            }
            catch (SqlException sqlException)
            {
                switch (sqlException.Number)
                {
                    // Sql timeout error
                    case ErrorCodes.SqlTimeoutNumber:
                        throw new SqlTimeoutException("Server did not respond with a response in the allocated time. Please try again later.", ErrorCodes.Timeout);
                }
                throw;
            }

            var itemIds = items.Select(i => i.ItemId).ToList();

            var itemIdsPermissions =
                // Always getting permissions for the Head version of an artifact.
                await _artifactPermissionsRepository.GetArtifactPermissions(itemIds, userId);

            IDictionary<int, IEnumerable<Artifact>> itemsNavigationPaths;
            if (searchCriteria.IncludeArtifactPath)
            {
                itemsNavigationPaths =
                    await
                        _artifactRepository.GetArtifactsNavigationPathsAsync(userId, itemIds, false);
            }
            else
            {
                itemsNavigationPaths = new Dictionary<int, IEnumerable<Artifact>>();
            }

            // items without permission should be removed
            items.RemoveAll(item => !itemIdsPermissions.ContainsKey(item.ItemId) || !itemIdsPermissions[item.ItemId].HasFlag(RolePermissions.Read));

            var joinedResult = from item in items
                               join permission in itemIdsPermissions.AsEnumerable()
                                   on item.ItemId equals permission.Key
                               join path in itemsNavigationPaths
                                   on item.Id equals path.Key into paths
                               from lpath in paths.DefaultIfEmpty()
                               select new { item, permission, lpath };
            foreach (var result in joinedResult)
            {
                result.item.Permissions = result.permission.Value;
                if (searchCriteria.IncludeArtifactPath)
                {
                    result.item.ArtifactPath = result.lpath.Value.Select(a => a.Name).ToList();
                    result.item.IdPath = result.lpath.Value.Select(a => a.Id).ToList();
                    result.item.ParentPredefinedType = result.lpath.Value.Select(a => a.PredefinedType).FirstOrDefault();
                }
            }

            return new ItemNameSearchResultSet
            {
                Items = items,
                PageItemCount = items.Count
            };
        }

        internal static IEnumerable<int> GetExcludedPredefineds(ItemNameSearchCriteria searchCriteria)
        {
            return ((ItemTypePredefined[])Enum.GetValues(typeof(ItemTypePredefined))).Where(p =>
            {
                if (p.IsRegularArtifactType())
                {
                    return !searchCriteria.ShowArtifacts;
                }
                if (p.IsBaselinesAndReviewsGroupType() && p != ItemTypePredefined.BaselineArtifactGroup)
                {
                    return !searchCriteria.ShowBaselinesAndReviews;
                }
                if (p.IsCollectionsGroupType() && p != ItemTypePredefined.CollectionArtifactGroup)
                {
                    return !searchCriteria.ShowCollections;
                }
                return false;
            }).Cast<int>();
        }

        internal static string GetQuery(string input)
        {
            // Unfortunately, double-quotes have special meaning inside FTI, so even if you parameterize it, the FTI engine treats it as a phrase delimiter.
            // doubling the quote to "" fixes it.

            return string.IsNullOrWhiteSpace(input) ? string.Empty :
                string.Format(CultureInfo.InvariantCulture, "\"{0}\"", input.Replace("\"", "\"\"").Replace(Environment.NewLine, string.Empty));
            // string.Format(CultureInfo.InvariantCulture, "\"{0}\"", input.Replace("'", "''").Replace("\"", "\"\"").Replace(@"\", @"\\").Replace(Environment.NewLine, string.Empty));
        }
    }
}
