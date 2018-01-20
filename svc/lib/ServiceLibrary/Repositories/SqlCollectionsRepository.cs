using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using ServiceLibrary.Exceptions;
using ServiceLibrary.Helpers;
using ServiceLibrary.Models.Collection;
using ServiceLibrary.Models.ProjectMeta;
using ServiceLibrary.Models;

namespace ServiceLibrary.Repositories
{
    public class SqlCollectionsRepository : ICollectionsRepository
    {
        private readonly ISqlConnectionWrapper _connectionWrapper;

        internal const string GetArtifactIdsInCollectionQuery =
            "SELECT * FROM [dbo].[GetArtifactIdsInCollection](@userId, @collectionId, @addDrafts)";

        #region Constructors

        public SqlCollectionsRepository() : this(new SqlConnectionWrapper(ServiceConstants.RaptorMain))
        {
        }

        public SqlCollectionsRepository(ISqlConnectionWrapper connectionWrapper)
        {
            _connectionWrapper = connectionWrapper;
        }

        #endregion


        #region Interface implementation

        public async Task<CollectionArtifacts> GetArtifactsWithPropertyValues(int userId,
            IEnumerable<int> artifactIds)
        {
            var propertyTypePredefineds =
                new List<int>
                {
                    (int)PropertyTypePredefined.ArtifactType,
                    (int)PropertyTypePredefined.ID
                }; // ArtifactType = 4148, ID = 4097

            var propertyTypeIds =
                new List<int>(); // TODO: should be filled with real data after implementation of getting list of property type ids from profile settings.

            var prm = new DynamicParameters();
            prm.Add("@UserId", userId, DbType.Int32);
            prm.Add("@AddDrafts", true, DbType.Boolean);
            prm.Add("@ArtifactIds", SqlConnectionWrapper.ToDataTable(artifactIds));
            prm.Add("@PropertyTypePredefineds", SqlConnectionWrapper.ToDataTable(propertyTypePredefineds));
            prm.Add("@PropertyTypeIds", SqlConnectionWrapper.ToDataTable(propertyTypeIds));

            var artifacts = (await _connectionWrapper.QueryAsync<CollectionArtifact>(
                "GetPropertyValuesForArtifacts", prm, commandType: CommandType.StoredProcedure)).ToList();

            var populatedArtifacts = PopulateArtifactsProperties(artifacts);
            return populatedArtifacts;
        }


        public async Task<IReadOnlyList<int>> GetContentArtifactIdsAsync(int collectionId, int userId, bool addDrafts = true)
        {
            var parameters = new DynamicParameters();
            parameters.Add("@userId", userId);
            parameters.Add("@collectionId", collectionId);
            parameters.Add("@addDrafts", addDrafts);

            var result = await _connectionWrapper.QueryAsync<int>(
                GetArtifactIdsInCollectionQuery,
                parameters,
                commandType: CommandType.Text);

            return result.ToList();
        }

        private CollectionArtifacts PopulateArtifactsProperties(List<CollectionArtifact> artifacts)
        {
            var artifactIdsResult = artifacts.Select(x => x.ArtifactId).Distinct().ToList();

            var artifactDtos = new List<ArtifactDto>();
            var settingsColumns = new List<ArtifactListColumn>();
            var areColumnsPopulated = false;

            foreach (var id in artifactIdsResult)
            {
                var artifactProperties = artifacts.Where(x => x.ArtifactId == id).ToList();

                var propertyInfos = new List<PropertyInfo>();
                int? itemTypeId = null;

                foreach (var artifactProperty in artifactProperties)
                {
                    ArtifactListColumn artifactListColumn = null;
                    var propertyInfo = new PropertyInfo();

                    if (!areColumnsPopulated)
                    {
                        artifactListColumn = new ArtifactListColumn { PropertyName = artifactProperty.PropertyName };
                    }

                    if (artifactProperty.PropertyTypeId == null)
                    {
                        int fakeId;

                        if (!ServiceConstants.PropertyTypePredefineds.TryGetValue(
                            (PropertyTypePredefined)artifactProperty.PropertyTypePredefined, out fakeId))
                        {
                            continue;
                        }

                        if (!areColumnsPopulated)
                        {
                            artifactListColumn.PropertyTypeId = fakeId;
                        }

                        propertyInfo.PropertyTypeId = fakeId;

                        if (fakeId == ServiceConstants.IdPropertyFakeId)
                        {
                            propertyInfo.Value = I18NHelper.FormatInvariant("{0}{1}", artifactProperty.Prefix,
                                artifactProperty.ArtifactId);

                            itemTypeId = artifactProperty.ItemTypeId;
                        }
                        else
                        {
                            propertyInfo.Value = artifactProperty.PropertyValue;
                        }
                    }
                    else
                    {
                        if (!areColumnsPopulated)
                        {
                            artifactListColumn.PropertyTypeId = artifactProperty.PropertyTypeId;
                        }

                        propertyInfo.PropertyTypeId = artifactProperty.PropertyTypeId;
                        propertyInfo.Value = artifactProperty.PropertyValue;
                    }

                    if (!areColumnsPopulated)
                    {
                        settingsColumns.Add(artifactListColumn);
                    }

                    propertyInfos.Add(propertyInfo);
                }

                areColumnsPopulated = true;

                artifactDtos.Add(new ArtifactDto
                {
                    ArtifactId = id,
                    ItemTypeId = itemTypeId,
                    PropertyInfos = propertyInfos
                });
            }

            return new CollectionArtifacts
            {
                Items = artifactDtos,
                ArtifactListSettings = new ArtifactListSettings { Columns = settingsColumns }
            };
        }

        #endregion


        public async Task<int> AddArtifactsToCollectionAsync(int userId, int collectionId, List<int> artifactIds, IDbTransaction transaction = null)
        {
            var parameters = new DynamicParameters();

            parameters.Add("@UserId", userId);
            parameters.Add("@CollectionId", collectionId);
            parameters.Add("@ArtifactIds", SqlConnectionWrapper.ToDataTable(artifactIds, "Int32Collection", "Int32Value"));

            if (transaction == null)
            {
                return (await _connectionWrapper.QueryAsync<int>("AddArtifactsToCollection", parameters, commandType: CommandType.StoredProcedure)).FirstOrDefault();
            }
            else
            {
                return (await transaction.Connection.QueryAsync<int>("AddArtifactsToCollection", parameters, transaction, commandType: CommandType.StoredProcedure)).FirstOrDefault();
            }
        }

        public async Task RemoveDeletedArtifactsFromCollection(int collectionId, int userId, IDbTransaction transaction = null)
        {
            var parameters = new DynamicParameters();

            parameters.Add("@UserId", userId);
            parameters.Add("@CollectionId", collectionId);

            if (transaction == null)
            {
                await _connectionWrapper.ExecuteAsync("RemoveDeletedArtifactsFromCollection", parameters, commandType: CommandType.StoredProcedure);
            }
            else
            {
                await transaction.Connection.ExecuteAsync("RemoveDeletedArtifactsFromCollection", parameters, transaction, commandType: CommandType.StoredProcedure);
            }
        }
    }
}