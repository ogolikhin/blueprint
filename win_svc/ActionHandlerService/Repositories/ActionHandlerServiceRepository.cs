using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using ActionHandlerService.Models;
using ServiceLibrary.Repositories;
using ServiceLibrary.Repositories.InstanceSettings;

namespace ActionHandlerService.Repositories
{
    public interface IActionHandlerServiceRepository : IInstanceSettingsRepository
    {
        Task<List<TenantInformation>> GetTenantsFromTenantsDb();
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

        public async Task<List<TenantInformation>> GetTenantsFromTenantsDb()
        {
            var tenants = await ConnectionWrapper.QueryAsync<TenantInformation>(
                @"SELECT [TenantId]
                ,[TenantName]
                ,[PackageLevel]
                ,[PackageName]
                ,[StartDate]
                ,[ExpirationDate]
                ,[BlueprintConnectionString]
                ,[AdminStoreLog]
                FROM [tenants].[Tenants]",
                commandType: CommandType.Text);
            return tenants.ToList();
        }
    }
}
