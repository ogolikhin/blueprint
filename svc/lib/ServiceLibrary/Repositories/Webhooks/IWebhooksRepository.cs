using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace ServiceLibrary.Repositories.Webhooks
{
    public interface IWebhooksRepository
    {
        Task<IEnumerable<SqlWebhooks>> CreateWebhooks(IEnumerable<SqlWebhooks> webhooks, IDbTransaction transaction = null);
        Task<IEnumerable<SqlWebhooks>> UpdateWebhooks(IEnumerable<SqlWebhooks> webhooks, IDbTransaction transaction = null);
        Task<IEnumerable<SqlWebhooks>> GetWebhooks(IEnumerable<int> webhookIds, IDbTransaction transaction = null);
    }
}
