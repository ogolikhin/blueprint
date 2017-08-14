using System.Data;
using System.Linq;
using System.Threading.Tasks;
using ServiceLibrary.Repositories;
using ServiceLibrary.Repositories.InstanceSettings;

namespace ActionHandlerService.Repositories
{
    public interface IActionHandlerServiceRepository : IInstanceSettingsRepository
    {
        Task<string> GetTenantId();
    }

    public class ActionHandlerServiceRepository : SqlInstanceSettingsRepository, IActionHandlerServiceRepository
    {
        public ActionHandlerServiceRepository(string connectionString) : this(new SqlConnectionWrapper(connectionString))
        {
        }

        public ActionHandlerServiceRepository(ISqlConnectionWrapper connectionWrapper) : this(connectionWrapper, new SqlArtifactPermissionsRepository(connectionWrapper))
        {
        }

        public ActionHandlerServiceRepository(ISqlConnectionWrapper connectionWrapper, IArtifactPermissionsRepository artifactPermissionsRepository) : base(connectionWrapper, artifactPermissionsRepository)
        {
        }

        //TODO: remove once we get the tenant db ready
        public async Task<string> GetTenantId()
        {
            return (await ConnectionWrapper.QueryAsync<string>("SELECT TenantId FROM dbo.Instances", commandType: CommandType.Text)).FirstOrDefault();
        }
    }
}
