using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using ServiceLibrary.Helpers;
using ServiceLibrary.Models.Collection;
using ServiceLibrary.Models.ProjectMeta;

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

        public async Task<ArtifactsOfCollection> GetArtifactsWithPropertyValues(int userId,
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

            var artifacts = (await _connectionWrapper.QueryAsync<ArtifactOfCollection>(
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

        private ArtifactsOfCollection PopulateArtifactsProperties(List<ArtifactOfCollection> artifacts)
        {
            var artifactIdsResult = artifacts.Select(x => x.ArtifactId).Distinct().ToList();

            var artifactDtos = new List<ArtifactDto>();
            var settingsColumns = new List<Column>();
            var areColumnsPopulated = false;

            foreach (var id in artifactIdsResult)
            {
                var artifactProperties = artifacts.Where(x => x.ArtifactId == id).ToList();

                var propertyInfos = new List<PropertyInfo>();
                int? itemTypeId = null;

                foreach (var artifactProperty in artifactProperties)
                {
                    Column column = null;
                    var propertyInfo = new PropertyInfo();

                    if (!areColumnsPopulated)
                    {
                        column = new Column { PropertyName = artifactProperty.PropertyName };
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
                            column.PropertyTypeId = fakeId;
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
                            column.PropertyTypeId = artifactProperty.PropertyTypeId;
                        }

                        propertyInfo.PropertyTypeId = artifactProperty.PropertyTypeId;
                        propertyInfo.Value = artifactProperty.PropertyValue;
                    }

                    if (!areColumnsPopulated)
                    {
                        settingsColumns.Add(column);
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

            return new ArtifactsOfCollection
            {
                Items = artifactDtos,
                Settings = new Settings { Columns = settingsColumns }
            };
        }

        #endregion
    }
}
