using Dapper;
using ServiceLibrary.Helpers;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using ServiceLibrary.Models;

namespace ServiceLibrary.Repositories.Webhooks
{
    public class WebhooksRepository : IWebhooksRepository
    {
        private readonly ISqlConnectionWrapper _connectionWrapper;

        public WebhooksRepository() : this(new SqlConnectionWrapper(ServiceConstants.RaptorMain)) { }

        public WebhooksRepository(ISqlConnectionWrapper connectionWrapper)
        {
            _connectionWrapper = connectionWrapper;
        }

        public async Task<IEnumerable<SqlWebhooks>> CreateWebhooks(IEnumerable<SqlWebhooks> webhooks, IDbTransaction transaction = null)
        {
            if (webhooks == null)
            {
                throw new ArgumentException(nameof(webhooks));
            }

            IEnumerable<SqlWebhooks> result = new List<SqlWebhooks>();

            var dWebhooks = webhooks.ToList();
            if (dWebhooks.Any())
            {
                var parameters = new DynamicParameters();
                parameters.Add("@webhooks", ToWebhooksCollectionDataTable(dWebhooks));

                if (transaction == null)
                {
                    result = await _connectionWrapper.QueryAsync<SqlWebhooks>("CreateWebhooks", parameters, commandType: CommandType.StoredProcedure);
                }
                else
                {
                    result = await transaction.Connection.QueryAsync<SqlWebhooks>("CreateWebhooks", parameters, transaction,
                        commandType: CommandType.StoredProcedure);
                }
            }

            return result;
        }

        public async Task<IEnumerable<SqlWebhooks>> UpdateWebhooks(IEnumerable<SqlWebhooks> webhooks, IDbTransaction transaction = null)
        {
            if (webhooks == null)
            {
                throw new ArgumentException(nameof(webhooks));
            }

            IEnumerable<SqlWebhooks> result = new List<SqlWebhooks>();

            var dWebhooks = webhooks.ToList();
            if (dWebhooks.Any())
            {
                var parameters = new DynamicParameters();
                parameters.Add("@webhooks", ToWebhooksCollectionDataTable(dWebhooks));

                if (transaction == null)
                {
                    result = await _connectionWrapper.QueryAsync<SqlWebhooks>("UpdateWebhooks", parameters, commandType: CommandType.StoredProcedure);
                }
                else
                {
                    result = await transaction.Connection.QueryAsync<SqlWebhooks>("UpdateWebhooks", parameters, transaction,
                        commandType: CommandType.StoredProcedure);
                }
            }

            return result;
        }

        public async Task<IEnumerable<SqlWebhooks>> GetWebhooks(IEnumerable<int> webhookIds, IDbTransaction transaction = null)
        {
            if (webhookIds == null)
            {
                throw new ArgumentException(nameof(webhookIds));
            }

            IEnumerable<SqlWebhooks> result = new List<SqlWebhooks>();

            var ids = webhookIds.ToList();
            if (ids.Any())
            {
                var parameters = new DynamicParameters();
                parameters.Add("@webhookIds", SqlConnectionWrapper.ToDataTable(ids));

                if (transaction == null)
                {
                    result = await _connectionWrapper.QueryAsync<SqlWebhooks>("GetWebhooks", parameters, commandType: CommandType.StoredProcedure);
                }
                else
                {
                    result = await transaction.Connection.QueryAsync<SqlWebhooks>("GetWebhooks", parameters, transaction, commandType: CommandType.StoredProcedure);
                }
            }

            return result;
        }

        private static DataTable ToWebhooksCollectionDataTable(IEnumerable<SqlWebhooks> webhooks)
        {
            var table = new DataTable { Locale = CultureInfo.InvariantCulture };
            table.SetTypeName("WebhooksCollection");
            table.Columns.Add("WebhookId", typeof(int));
            table.Columns.Add("Url", typeof(string));
            table.Columns.Add("SecurityInfo", typeof(string));
            table.Columns.Add("State", typeof(bool));
            table.Columns.Add("Scope", typeof(string));
            table.Columns.Add("EventType", typeof(int));
            table.Columns.Add("WorkflowId", typeof(int));

            foreach (var webhook in webhooks)
            {
                table.Rows.Add(webhook.WebhookId, webhook.Url, webhook.SecurityInfo, webhook.State, webhook.Scope,
                    webhook.EventType, webhook.WorkflowId);
            }

            return table;
        }

        public async Task<IReadOnlyList<ArtifactPropertyInfo>> GetArtifactsWithPropertyValuesAsync(
            int userId, IEnumerable<int> artifactIds, IEnumerable<int> propertyTypePredefineds, IEnumerable<int> propertyTypeIds)
        {
            var parameters = new DynamicParameters();
            parameters.Add("@UserId", userId, DbType.Int32);
            parameters.Add("@AddDrafts", true, DbType.Boolean);
            parameters.Add("@ArtifactIds", SqlConnectionWrapper.ToDataTable(artifactIds));
            parameters.Add("@PropertyTypePredefineds", SqlConnectionWrapper.ToDataTable(propertyTypePredefineds));
            parameters.Add("@PropertyTypeIds", SqlConnectionWrapper.ToDataTable(propertyTypeIds));

            var result = await _connectionWrapper.QueryAsync<ArtifactPropertyInfo>(
                "GetPropertyValuesForArtifacts", parameters, commandType: CommandType.StoredProcedure);

            return result.ToList();
        }

        public async Task<IEnumerable<RevisionDataInfo>> GetRevisionInfos(IEnumerable<int> revisionIds)
        {
            var parameters = new DynamicParameters();
            parameters.Add("@revisionIds", SqlConnectionWrapper.ToDataTable(revisionIds));

            var result = await _connectionWrapper.QueryAsync<RevisionDataInfo>(
                "GetRevisionInfos", parameters, commandType: CommandType.StoredProcedure);

            return result.ToList();

        }
    }
}
