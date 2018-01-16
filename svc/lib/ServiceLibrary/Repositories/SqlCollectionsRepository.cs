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

        public async Task<ArtifactsOfCollection> GetArtifactsOfCollectionAsync(int userId, IEnumerable<int> artifactIds)
        {
            var propertyTypePredefineds = new List<int> { (int)PropertyTypePredefined.ArtifactType }; // ArtifactType = 4148
            var propertyTypeIds = new List<int>(); // TODO: should be filled with real data after implementation of getting list of propertyType ids.

            var prm = new DynamicParameters();
            prm.Add("@UserId", userId, DbType.Int32);
            prm.Add("@AddDrafts", true, DbType.Boolean);
            prm.Add("@ArtifactIds", SqlConnectionWrapper.ToDataTable(artifactIds));
            prm.Add("@PropertyTypePredefineds", SqlConnectionWrapper.ToDataTable(propertyTypePredefineds));
            prm.Add("@PropertyTypeIds", SqlConnectionWrapper.ToDataTable(propertyTypeIds));

            var items = await _connectionWrapper.QueryAsync<ArtifactOfCollection>(
                "GetPropertyValuesForArtifacts", prm, commandType: CommandType.StoredProcedure);

            var artifactsOfCollection = items as ArtifactOfCollection[] ?? items.ToArray();
            var artifactIdsResult = artifactsOfCollection.Select(x => x.ArtifactId).Distinct().ToList();

            var artifactDtos = new List<ArtifactDto>();
            var settingsColumns = new List<Column>();

            if (artifactIdsResult.Any())
            {
                var propertiesOfTheFirstArtifact = artifactsOfCollection.Where(x => x.ArtifactId == artifactIdsResult[0]).ToList();

                foreach (var artifactProperty in propertiesOfTheFirstArtifact)
                {
                    int fakeId;
                    if (artifactProperty.PropertyTypeId == null && ServiceConstants
                            .PropertyTypePredefineds
                            .TryGetValue((PropertyTypePredefined)artifactProperty.PropertyTypePredefined, out fakeId))
                    {
                        settingsColumns.Add(new Column
                        {
                            PropertyTypeId = fakeId,
                            PropertyName = artifactProperty.PropertyName
                        });
                    }
                    else
                    {
                        settingsColumns.Add(new Column
                        {
                            PropertyTypeId = artifactProperty.PropertyTypeId,
                            PropertyName = artifactProperty.PropertyName
                        });
                    }
                }

                // Add ID column for Artifact
                settingsColumns.Add(new Column
                {
                    PropertyTypeId = ServiceConstants.PropertyTypePredefineds[PropertyTypePredefined.ID],
                    PropertyName = "ID"
                });
            }

            foreach (var id in artifactIdsResult)
            {
                var artifactProperties = artifactsOfCollection.Where(x => x.ArtifactId == id).ToList();

                var propertyInfos = new List<PropertyInfo>();

                foreach (var artifactProperty in artifactProperties)
                {
                    int fakeId;
                    if (artifactProperty.PropertyTypeId == null && ServiceConstants
                            .PropertyTypePredefineds
                            .TryGetValue((PropertyTypePredefined)artifactProperty.PropertyTypePredefined, out fakeId) && settingsColumns.Any(x => x.PropertyTypeId == fakeId))
                    {
                        propertyInfos.Add(new PropertyInfo
                        {
                            PropertyTypeId = fakeId,
                            Value = artifactProperty.PropertyValue
                        });
                    }
                    else if (settingsColumns.Any(x => x.PropertyTypeId == artifactProperty.PropertyTypeId))
                    {
                        propertyInfos.Add(new PropertyInfo
                        {
                            PropertyTypeId = artifactProperty.PropertyTypeId,
                            Value = artifactProperty.PropertyValue
                        });
                    }
                }

                // Add ID property for Artifact
                propertyInfos.Add(new PropertyInfo
                {
                    PropertyTypeId = ServiceConstants.PropertyTypePredefineds[PropertyTypePredefined.ID],
                    Value =
                        $"{artifactProperties.FirstOrDefault(x => !string.IsNullOrEmpty(x.Prefix))?.Prefix}{artifactProperties.FirstOrDefault(x => !string.IsNullOrEmpty(x.Prefix))?.ArtifactId}"
                });

                artifactDtos.Add(new ArtifactDto
                {
                    ArtifactId = id,
                    ItemTypeId = artifactProperties.First(x => x.ItemTypeId != null)?.ItemTypeId,
                    PropertyInfos = propertyInfos
                });
            }

            return new ArtifactsOfCollection { Items = artifactDtos, Settings = new Settings { Columns = settingsColumns } };
        }

        #endregion
    }
}
