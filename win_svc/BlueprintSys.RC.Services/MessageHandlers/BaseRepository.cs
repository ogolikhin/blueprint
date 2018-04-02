using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using BlueprintSys.RC.Services.Helpers;
using Dapper;
using ServiceLibrary.Models;
using ServiceLibrary.Repositories;
using ServiceLibrary.Repositories.InstanceSettings;

namespace BlueprintSys.RC.Services.MessageHandlers
{
    public interface IBaseRepository : IInstanceSettingsRepository
    {
        IUsersRepository UsersRepository { get; }
        Task<SqlUser> GetUser(int userId);
        Task<List<TenantInformation>> GetTenantsFromTenantsDb();
        Task<bool> IsProjectMaxArtifactBoundaryReached(int projectId);

        /// <summary>
        /// Calls the [dbo].[GetTransactionStatus] stored procedure.
        /// </summary>
        Task<int> GetTransactionStatus(long transactionId);

        Task<int> GetNServiceBusBatchSize();
    }

    public class BaseRepository : SqlInstanceSettingsRepository, IBaseRepository
    {
        public BaseRepository(string connectionString) : this(new SqlConnectionWrapper(connectionString))
        {
        }

        public BaseRepository(ISqlConnectionWrapper connectionWrapper) : this(connectionWrapper, new SqlArtifactPermissionsRepository(connectionWrapper), new SqlUsersRepository(connectionWrapper))
        {
        }

        public BaseRepository(ISqlConnectionWrapper connectionWrapper, IArtifactPermissionsRepository artifactPermissionsRepository, IUsersRepository usersRepository) : base(connectionWrapper, artifactPermissionsRepository)
        {
            UsersRepository = usersRepository;
        }

        public IUsersRepository UsersRepository { get; }

        public async Task<SqlUser> GetUser(int userId)
        {
            var userIds = new[]
            {
                userId
            };
            return (await UsersRepository.GetExistingUsersByIdsAsync(userIds)).FirstOrDefault(u => u.UserId == userId);
        }

        public async Task<List<TenantInformation>> GetTenantsFromTenantsDb()
        {
            return (await ConnectionWrapper.QueryAsync<TenantInformation>(@"SELECT [TenantId], [TenantName], [PackageLevel], [PackageName], [StartDate], [ExpirationDate], [BlueprintConnectionString], [AdminStoreLog] FROM [tenants].[Tenants]", commandType: CommandType.Text)).ToList();
        }

        public async Task<bool> IsProjectMaxArtifactBoundaryReached(int projectId)
        {
            return await CheckMaxArtifactsPerProjectBoundary(projectId) == 2;
        }

        public async Task<int> GetTransactionStatus(long transactionId)
        {
            var param = new DynamicParameters();
            param.Add("@transactionId", transactionId);
            return await ConnectionWrapper.ExecuteScalarAsync<int>("[dbo].[GetTransactionStatus]", param, commandType: CommandType.StoredProcedure);
        }

        public async Task<int> GetNServiceBusBatchSize()
        {
            var setting = (await ConnectionWrapper.QueryAsync<string>(@"SELECT [Value] FROM [dbo].[ApplicationSettings] WHERE [Key] = 'NServiceBusBatchSize'", commandType: CommandType.Text)).FirstOrDefault();
            int batchSize;
            if (int.TryParse(setting, out batchSize) && batchSize > 0)
            {
                return batchSize;
            }
            return 1000;
        }
    }
}
