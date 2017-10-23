using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;
using Dapper;
using ServiceLibrary.Helpers;

namespace ServiceLibrary.Repositories
{
    public class SqlLockArtifactsRepository : ILockArtifactsRepository
    {
        protected ISqlConnectionWrapper _connectionWrapper;

        private ISqlHelper _sqlHelper;

        public SqlLockArtifactsRepository() : this(new SqlConnectionWrapper(ServiceConstants.RaptorMain), new SqlHelper())
        {
        }

        public SqlLockArtifactsRepository(ISqlConnectionWrapper connectionWrapper, ISqlHelper sqlHelper)
        {
            _connectionWrapper = connectionWrapper;
            _sqlHelper = sqlHelper;
        }

        /// <summary>
        /// Locks an artifact
        /// </summary>
        /// <param name="artifactId"></param>
        /// <param name="userId"></param>
        /// <returns>
        /// Returns True if artifact has been successfully locked
        /// Returns False if artifact has already been locked by specified user
        /// </returns>
        public Task<bool> LockArtifactAsync(int artifactId, int userId)
        {
            return _sqlHelper.RetryOnSqlDealLockAsync(async () =>
            {
                return await LockArtifactInternalAsync(artifactId, userId);
            }, 10);

        }

        private async Task<bool> LockArtifactInternalAsync(int artifactId, int userId)
        {
            var prm = new DynamicParameters();
            prm.Add("@userId", userId);
            prm.Add("@artifactId", artifactId);

            try
            {
                await _connectionWrapper.ExecuteAsync("LockArtifact", prm, commandType: CommandType.StoredProcedure);
            }
            catch (SqlException e)
            {
                if (e.Number == 50000) // Default error generated from SQL by RAISERROR ('Artifact already locked',16,1)
                {
                    return false;
                }
                throw;
            }
            return true;
        }
    }
}
