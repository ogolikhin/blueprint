using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using ServiceLibrary.Models;

namespace ServiceLibrary.Repositories.Webhooks
{
    public interface IWebhooksRepository
    {
        Task<IEnumerable<SqlWebhooks>> CreateWebhooks(IEnumerable<SqlWebhooks> webhooks, IDbTransaction transaction = null);
        Task<IEnumerable<SqlWebhooks>> UpdateWebhooks(IEnumerable<SqlWebhooks> webhooks, IDbTransaction transaction = null);
        Task<IEnumerable<SqlWebhooks>> GetWebhooks(IEnumerable<int> webhookIds, IDbTransaction transaction = null);
        Task<IEnumerable<SqlWebhooks>> GetWebhooksByWorkflowId(int workflowId, IDbTransaction transaction = null);

        Task<IReadOnlyList<ArtifactPropertyInfo>> GetArtifactsWithPropertyValuesAsync(
            int userId, IEnumerable<int> artifactIds, IEnumerable<int> propertyTypePredefineds,
            IEnumerable<int> propertyTypeIds, IDbTransaction transaction = null);

        Task<IEnumerable<RevisionDataInfo>> GetRevisionInfos(IEnumerable<int> revisionIds, IDbTransaction transaction = null);
    }
}
