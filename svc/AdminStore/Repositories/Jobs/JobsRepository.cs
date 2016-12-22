using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using ServiceLibrary.Exceptions;
using ServiceLibrary.Helpers;
using ServiceLibrary.Models.Jobs;
using ServiceLibrary.Repositories;

namespace AdminStore.Repositories.Jobs
{
    public class JobsRepository : IJobsRepository
    {
        internal readonly ISqlConnectionWrapper ConnectionWrapper;

        public JobsRepository() : 
            this(new SqlConnectionWrapper(ServiceConstants.RaptorMain))
        {
        }

        internal JobsRepository(ISqlConnectionWrapper connectionWrapper)
        {
            ConnectionWrapper = connectionWrapper;
        }

        public async Task<IList<JobInfo>> GetVisibleJobs(int? userId, int? offsetId, int? minId, int? limit)
        {
            var param = new DynamicParameters();

            param.Add("@hidden", DBNull.Value);
            param.Add("@userId", userId.HasValue && userId.Value > 0 ? userId.Value : (object)DBNull.Value);
            param.Add("@projectId", DBNull.Value);
            param.Add("@addFinished", true);
            param.Add("@receiverJobServiceId", DBNull.Value);
            param.Add("@doNotFetchResult", false);
            param.Add("@offsetId", offsetId ?? int.MaxValue);
            param.Add("@minId", minId ?? 1);
            param.Add("@limit", limit ?? int.MaxValue);

            try
            {
                return (await ConnectionWrapper.QueryAsync<JobInfo>("GetJobMessages", param, commandType: CommandType.StoredProcedure)).ToList();
            }
            catch (SqlException sqlException)
            {
                switch (sqlException.Number)
                {
                    //Sql timeout error
                    case ErrorCodes.SqlTimeoutNumber:
                        throw new SqlTimeoutException("Server did not respond with a response in the allocated time. Please try again later.", ErrorCodes.Timeout);
                }
                throw;
            }
        }
    }
}