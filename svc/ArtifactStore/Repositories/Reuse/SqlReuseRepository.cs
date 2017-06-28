using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using ArtifactStore.Models.Reuse;
using Dapper;
using ServiceLibrary.Helpers;
using ServiceLibrary.Models.Enums;
using ServiceLibrary.Repositories;

namespace ArtifactStore.Repositories.Reuse
{
    public interface ISqlReuseRepository
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="artifactIds"></param>
        /// <returns>Dictionary with Item Id as Key</returns>
        Task<IDictionary<int, SqlItemTypeInfo>> GetStandardTypeIdsForArtifactsIdsAsync(ISet<int> artifactIds);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="propertyIds"></param>
        /// <returns>Dictionary with Property Type Id as Key</returns>
        Task<IDictionary<int, SqlPropertyTypeInfo>> GetStandardPropertyTypeIdsForPropertyIdsAsync(ISet<int> propertyIds);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="instanceItemTypeIds"></param>
        /// <returns>Dictionary with Type Id as Key</returns>
        Task<IDictionary<int, ItemTypeReuseTemplate>> GetReuseItemTypeTemplatesAsyc(IEnumerable<int> instanceItemTypeIds);
    }

    public class SqlReuseRepository : SqlBaseArtifactRepository, ISqlReuseRepository
    {
        public SqlReuseRepository()
            : this(new SqlConnectionWrapper(ServiceConstants.RaptorMain))
        {
        }

        public SqlReuseRepository(ISqlConnectionWrapper connectionWrapper)
            : this(connectionWrapper, new SqlArtifactPermissionsRepository(connectionWrapper))
        {
        }

        public SqlReuseRepository(ISqlConnectionWrapper connectionWrapper,
            IArtifactPermissionsRepository artifactPermissionsRepository)
            : base(connectionWrapper, artifactPermissionsRepository)
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="artifactIds"></param>
        /// <returns>Dictionary with Item Id as Key</returns>
        public async Task<IDictionary<int, SqlItemTypeInfo>> GetStandardTypeIdsForArtifactsIdsAsync(ISet<int> artifactIds)
        {

            var param = new DynamicParameters();
            param.Add("@artifactIds", SqlConnectionWrapper.ToDataTable(artifactIds));

            return (await ConnectionWrapper.QueryAsync<SqlItemTypeInfo>("GetStandardTypeIdsForArtifactsIds", param,
                            commandType: CommandType.StoredProcedure))
                            .ToDictionary(k => k.ItemId);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="propertyIds"></param>
        /// <returns>Dictionary with Property Type Id as Key</returns>
        public async Task<IDictionary<int, SqlPropertyTypeInfo>> GetStandardPropertyTypeIdsForPropertyIdsAsync(ISet<int> propertyIds)
        {

            var param = new DynamicParameters();
            param.Add("@propertyIds", SqlConnectionWrapper.ToDataTable(propertyIds));

            return (await ConnectionWrapper.QueryAsync<SqlPropertyTypeInfo>("GetStandardPropertyTypeIdsForPropertyIds", param,
                            commandType: CommandType.StoredProcedure))
                            .ToDictionary(k => k.PropertyTypeId);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="instanceItemTypeIds"></param>
        /// <returns>Dictionary with Type Id as Key</returns>
        public async Task<IDictionary<int, ItemTypeReuseTemplate>> GetReuseItemTypeTemplatesAsyc(IEnumerable<int> instanceItemTypeIds)
        {
            var param = new DynamicParameters();
            param.Add("@instanceItemTypeIds", SqlConnectionWrapper.ToDataTable(instanceItemTypeIds));
            var result =
                (await ConnectionWrapper.QueryAsync<SqlItemTypeReuseTemplate>("GetReuseItemTypeTemplates", param,
                    commandType: CommandType.StoredProcedure)).GroupBy(v => v.TypeId);
            var templates = new Dictionary<int, ItemTypeReuseTemplate>();
            foreach (var templateInfo in result)
            {
                var typeId = templateInfo.Key;
                var itemTemplate = templateInfo.First();

                var reuseTemplate = itemTemplate.TemplateId.HasValue ? new ItemTypeReuseTemplate() : null;
                templates[typeId] = reuseTemplate;
                if (!itemTemplate.TemplateId.HasValue || reuseTemplate == null)
                {
                    continue;
                }
                reuseTemplate.AllowReadOnlyOverride = itemTemplate.TypeAllowReadOnlyOverride;
                reuseTemplate.ItemTypeId = itemTemplate.TypeId;
                reuseTemplate.ItemTypeReuseTemplateId = itemTemplate.TemplateId.Value;
                reuseTemplate.ReadOnlySettings = itemTemplate.TypeReadOnlySettings ??
                                                     ItemTypeReuseTemplateSetting.None;
                reuseTemplate.SensitivitySettings = itemTemplate.TypeSensitivitySettings ??
                                                    ItemTypeReuseTemplateSetting.All;
                foreach (var propertyTemplateInfo in templateInfo)
                {
                    if (reuseTemplate.PropertyTypeReuseTemplates.ContainsKey(propertyTemplateInfo.PropertyTypeId))
                    {
                        continue;
                    }
                    reuseTemplate.PropertyTypeReuseTemplates.Add(propertyTemplateInfo.PropertyTypeId,
                        new PropertyTypeReuseTemplate
                        {
                            PropertyTypePredefined = propertyTemplateInfo.PropertyTypePredefined,
                            PropertyTypeId = propertyTemplateInfo.PropertyTypeId,
                            ItemTypeReuseTemplateId = reuseTemplate.ItemTypeReuseTemplateId,
                            Settings = propertyTemplateInfo.PropertySettings ?? PropertyTypeReuseTemplateSettings.None
                        });
                }
            }
            return templates;
        }
    }
}