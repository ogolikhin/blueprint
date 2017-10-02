using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using ServiceLibrary.Helpers;
using ServiceLibrary.Models.Enums;
using ServiceLibrary.Models.Reuse;

namespace ServiceLibrary.Repositories.Reuse
{
    public class ReuseRepository : SqlBaseArtifactRepository, IReuseRepository
    {
        public ReuseRepository()
            : this(new SqlConnectionWrapper(ServiceConstants.RaptorMain))
        {
        }

        public ReuseRepository(ISqlConnectionWrapper connectionWrapper)
            : this(connectionWrapper, new SqlArtifactPermissionsRepository(connectionWrapper))
        {
        }

        public ReuseRepository(ISqlConnectionWrapper connectionWrapper,
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
        /// <param name="transaction"></param>
        /// <returns>Dictionary with Type Id as Key</returns>
        public async Task<IDictionary<int, ItemTypeReuseTemplate>> GetReuseItemTypeTemplatesAsyc(
            IEnumerable<int> instanceItemTypeIds,
            IDbTransaction transaction = null)
        {
            var parameters = new DynamicParameters();
            parameters.Add("@instanceItemTypeIds", SqlConnectionWrapper.ToDataTable(instanceItemTypeIds));
            IEnumerable<IGrouping<int, SqlItemTypeReuseTemplate>> result;

            if (transaction == null)
            {
                result = 
                (
                    await ConnectionWrapper.QueryAsync<SqlItemTypeReuseTemplate>
                    (
                        "GetReuseItemTypeTemplates",
                        parameters,
                        commandType: CommandType.StoredProcedure
                    )
                ).GroupBy(v => v.TypeId);
            }
            else
            {
                result = 
                (
                    await transaction.Connection.QueryAsync<SqlItemTypeReuseTemplate>
                    (
                        "GetReuseItemTypeTemplates",
                        parameters,
                        transaction,
                        commandType: CommandType.StoredProcedure
                    )
                ).GroupBy(v => v.TypeId);
            }

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

        public async Task<IEnumerable<SqlModifiedItems>> GetModificationsForRevisionIdAsyc(int revisionId, IDbTransaction transaction = null)
        {
            var parameters = new DynamicParameters();
            parameters.Add("@revisionId", revisionId);

            if (transaction == null)
            {
                return await ConnectionWrapper.QueryAsync<SqlModifiedItems>
                (
                    "GetModificationsForRevisionId",
                    parameters,
                    commandType: CommandType.StoredProcedure
                );
            }

            return await transaction.Connection.QueryAsync<SqlModifiedItems>
            (
                "GetModificationsForRevisionId",
                parameters,
                transaction,
                commandType: CommandType.StoredProcedure
            );
        }

        public async Task<Dictionary<int, bool>> DoItemsContainReadonlyReuse(IEnumerable<int> itemIds, IDbTransaction transaction = null)
        {
            var parameters = new DynamicParameters();
            parameters.Add("@itemIds", SqlConnectionWrapper.ToDataTable(itemIds));

            if (transaction == null)
            {
                return 
                (
                    await ConnectionWrapper.QueryAsync<dynamic>
                    (
                        "DoItemsContainReadonlyReuse",
                        parameters,
                        commandType: CommandType.StoredProcedure
                    )
                ).ToDictionary(a => (int)a.ItemId, b => (bool)b.IsReadOnlyReuse);
            }
            return 
            (
                await transaction.Connection.QueryAsync<dynamic>
                (
                    "DoItemsContainReadonlyReuse", 
                    parameters, 
                    transaction,
                    commandType: CommandType.StoredProcedure
                )
            ).ToDictionary(a => (int)a.ItemId, b => (bool)b.IsReadOnlyReuse);
        }
    }
}
