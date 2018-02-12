using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace ServiceLibrary.Repositories.Webhooks
{
    public interface IWebhookRepository
    {
        Task<IEnumerable<SqlWebhook>> CreateWebhooks(IEnumerable<SqlWebhook> webhooks, IDbTransaction transaction = null);
        Task<IEnumerable<SqlWebhook>> UpdateWebhooks(IEnumerable<SqlWebhook> webhooks, IDbTransaction transaction = null);
        Task<IEnumerable<SqlWebhook>> GetWebhooks(IEnumerable<int> webhookIds, IDbTransaction transaction = null);
    }
}
