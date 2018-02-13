using Dapper;
using ServiceLibrary.Helpers;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace ServiceLibrary.Repositories.Webhooks
{
    public class WebhookRepository : IWebhookRepository
    {
        private readonly ISqlConnectionWrapper _connectionWrapper;

        public WebhookRepository() : this(new SqlConnectionWrapper(ServiceConstants.RaptorMain)) { }

        public WebhookRepository(ISqlConnectionWrapper connectionWrapper)
        {
            _connectionWrapper = connectionWrapper;
        }

        public async Task<IEnumerable<SqlWebhook>> CreateWebhooks(IEnumerable<SqlWebhook> webhooks, IDbTransaction transaction = null)
        {
            if (webhooks == null)
            {
                throw new ArgumentException(nameof(webhooks));
            }

            IEnumerable<SqlWebhook> result = new List<SqlWebhook>();

            var dWebhooks = webhooks.ToList();
            if (dWebhooks.Any())
            {
                var parameters = new DynamicParameters();
                parameters.Add("@webhooks", ToWebhooksCollectionDataTable(dWebhooks));

                if (transaction == null)
                {
                    result = await _connectionWrapper.QueryAsync<SqlWebhook>("CreateWebhooks", parameters, commandType: CommandType.StoredProcedure);
                }
                else
                {
                    result = await transaction.Connection.QueryAsync<SqlWebhook>("CreateWebhooks", parameters, transaction,
                        commandType: CommandType.StoredProcedure);
                }
            }

            return result;
        }

        public async Task<IEnumerable<SqlWebhook>> UpdateWebhooks(IEnumerable<SqlWebhook> webhooks, IDbTransaction transaction = null)
        {
            if (webhooks == null)
            {
                throw new ArgumentException(nameof(webhooks));
            }

            IEnumerable<SqlWebhook> result = new List<SqlWebhook>();

            var dWebhooks = webhooks.ToList();
            if (dWebhooks.Any())
            {
                var parameters = new DynamicParameters();
                parameters.Add("@webhooks", ToWebhooksCollectionDataTable(dWebhooks));

                if (transaction == null)
                {
                    result = await _connectionWrapper.QueryAsync<SqlWebhook>("UpdateWebhooks", parameters, commandType: CommandType.StoredProcedure);
                }
                else
                {
                    result = await transaction.Connection.QueryAsync<SqlWebhook>("UpdateWebhooks", parameters, transaction,
                        commandType: CommandType.StoredProcedure);
                }
            }

            return result;
        }

        public async Task<IEnumerable<SqlWebhook>> GetWebhooks(IEnumerable<int> webhookIds, IDbTransaction transaction = null)
        {
            if (webhookIds == null)
            {
                throw new ArgumentException(nameof(webhookIds));
            }

            IEnumerable<SqlWebhook> result = new List<SqlWebhook>();

            var ids = webhookIds.ToList();
            if (ids.Any())
            {
                var parameters = new DynamicParameters();
                parameters.Add("@webhookIds", SqlConnectionWrapper.ToDataTable(ids));

                if (transaction == null)
                {
                    result = await _connectionWrapper.QueryAsync<SqlWebhook>("GetWebhooks", parameters, commandType: CommandType.StoredProcedure);
                }
                else
                {
                    result = await transaction.Connection.QueryAsync<SqlWebhook>("GetWebhooks", parameters, transaction, commandType: CommandType.StoredProcedure);
                }
            }

            return result;
        }

        private static DataTable ToWebhooksCollectionDataTable(IEnumerable<SqlWebhook> webhooks)
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
    }
}
