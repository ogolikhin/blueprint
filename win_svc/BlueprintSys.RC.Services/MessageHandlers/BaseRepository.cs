using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BlueprintSys.RC.Services.Helpers;
using BluePrintSys.Messaging.CrossCutting.Models.Exceptions;
using BluePrintSys.Messaging.Models.Actions;
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
        /// Validates whether the Revision ID has been committed to the database. This will eventually be replaced with Transaction validation.
        /// </summary>
        Task<RevisionStatus> ValidateRevision(int revisionId, IBaseRepository repository, ActionMessage message, TenantInformation tenant);
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

        private async Task<RevisionStatus> GetRevisionStatus(int revisionId)
        {
            var param = new DynamicParameters();
            param.Add("@revisionId", revisionId);
            var revisionIdCount = await ConnectionWrapper.ExecuteScalarAsync<int>(@"SELECT COUNT(1) FROM [dbo].[Revisions] WHERE [RevisionId] = @revisionId", param, commandType: CommandType.Text);
            var revisionIdExists = revisionIdCount > 0;
            if (revisionIdExists)
            {
                return RevisionStatus.Committed;
            }
            var uncommittedRevisionIdCount = await ConnectionWrapper.ExecuteScalarAsync<int>(@"SELECT COUNT(1) FROM [dbo].[Revisions] WITH(READUNCOMMITTED) WHERE [RevisionId] = @revisionId", param, commandType: CommandType.Text);
            var uncommittedRevisionIdExists = uncommittedRevisionIdCount > 0;
            if (uncommittedRevisionIdExists)
            {
                return RevisionStatus.Uncommitted;
            }
            return RevisionStatus.RolledBack;
        }

        public async Task<RevisionStatus> ValidateRevision(int revisionId, IBaseRepository repository, ActionMessage message, TenantInformation tenant)
        {
            const int millisecondsTimeout = 1000;
            const int triesMax = 3;
            var tries = 1;
            while (true)
            {
                var revisionStatus = await GetRevisionStatus(revisionId);
                switch (revisionStatus)
                {
                    case RevisionStatus.Committed:
                        Logger.Log($"Revision ID {revisionId} exists", message, tenant);
                        return revisionStatus;
                    case RevisionStatus.Uncommitted:
                        if (tries < triesMax)
                        {
                            Logger.Log($"Revision ID {revisionId} exists in an uncommitted transaction. Trying again", message, tenant);
                            Thread.Sleep(millisecondsTimeout);
                            tries++;
                            continue;
                        }
                        var errorMessage = $"Revision ID {revisionId} is still uncommitted. Unable to handle message at this time";
                        Logger.Log(errorMessage, message, tenant);
                        throw new EntityNotFoundException(errorMessage);
                    case RevisionStatus.RolledBack:
                        Logger.Log($"Revision ID {revisionId} does not exist. Message will not be handled", message, tenant, LogLevel.Error);
                        return revisionStatus;
                    default:
                        throw new ArgumentOutOfRangeException($"Unhandled Revision Status {revisionStatus}");
                }
            }
        }
    }

    public enum RevisionStatus
    {
        Committed = 0,
        Uncommitted = 1,
        RolledBack = 2
    }
}
