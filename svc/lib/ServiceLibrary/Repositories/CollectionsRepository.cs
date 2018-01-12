using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using ServiceLibrary.Exceptions;
using ServiceLibrary.Helpers;
using ServiceLibrary.Models;
using ServiceLibrary.Models.Collection;

namespace ServiceLibrary.Repositories
{
    public class CollectionsRepository : ICollectionsRepository
    {
        private readonly ISqlConnectionWrapper _connectionWrapper;

        #region Constructors

        public CollectionsRepository()
            : this(new SqlConnectionWrapper(ServiceConstants.RaptorMain))
        {
        }

        public CollectionsRepository(ISqlConnectionWrapper connectionWrapper)
        {
            _connectionWrapper = connectionWrapper;
        }

        #endregion

        #region Interface implementation

        public async Task<ArtifactsOfCollectionDto> GetArtifactsOfCollectionAsync(int userId, IEnumerable<int> artifactIds)
        {
            var propertyTypePredefineds = new List<int> { 4148 }; // ArtifactType = 4148
            var propertyTypeIds = new List<int> { 4098, 4099 }; // Name = 4098, Description = 4099

            var prm = new DynamicParameters();
            prm.Add("@UserId", userId, DbType.Int32);
            prm.Add("@AddDrafts", true, DbType.Boolean);
            prm.Add("@ArtifactIds", SqlConnectionWrapper.ToDataTable(artifactIds));
            prm.Add("@PropertyTypePredefineds", SqlConnectionWrapper.ToDataTable(propertyTypePredefineds));
            prm.Add("@PropertyTypeIds", SqlConnectionWrapper.ToDataTable(propertyTypeIds));

            var items = await _connectionWrapper.QueryAsync<ArtifactsOfCollection>(
                "GetPropertyValuesForArtifacts", prm, commandType: CommandType.StoredProcedure);

            var artifactsOfCollection = items as ArtifactsOfCollection[] ?? items.ToArray();
            var artifactIdsResult = artifactsOfCollection.Select(x => x.ArtifactId).Distinct().ToList();

            var artifactDtos = new List<ArtifactDto>();
            var settingsColumns = new List<ColumnDto>();

            foreach (var id in artifactIdsResult)
            {
                var artifactProperties = artifactsOfCollection.Where(x => x.ArtifactId == id).ToList();

                var propertyInfos = artifactProperties.Select(artifactProperty => new PropertyInfoDto
                {
                    PropertyTypeId = artifactProperty.PropertyTypeId,
                    Value = artifactProperty.PropertyValue
                }).ToList();

                // Add prefix property for Artifact
                propertyInfos.Add(new PropertyInfoDto
                {
                    PropertyTypeId = null,
                    Value = artifactProperties.First(x => !string.IsNullOrEmpty(x.Prefix)).Prefix
                });

                // Add ItemTypeId property for Artifact
                propertyInfos.Add(new PropertyInfoDto
                {
                    PropertyTypeId = null,
                    Value = artifactProperties.First(x => x.ItemTypeId != null).ItemTypeId.ToString()
                });

                artifactDtos.Add(new ArtifactDto
                {
                    ArtifactId = id,
                    PropertyInfos = propertyInfos
                });

                settingsColumns = artifactProperties.Select(artifactProperty => new ColumnDto
                {
                    PropertyTypeId = artifactProperty.PropertyTypeId,
                    PropertyName = artifactProperty.PropertyName
                }).ToList();

                // Add prefix column for Artifact
                settingsColumns.Add(new ColumnDto
                {
                    PropertyTypeId = null,
                    PropertyName = "Prefix"
                });

                // Add ItemTypeId column for Artifact
                settingsColumns.Add(new ColumnDto
                {
                    PropertyTypeId = null,
                    PropertyName = "ItemTypeId"
                });
            }

            return new ArtifactsOfCollectionDto { Items = artifactDtos, Settings = new SettingsDto { Columns = settingsColumns } };
        }

        #endregion
    }
}
