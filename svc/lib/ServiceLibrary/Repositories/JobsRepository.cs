using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using ServiceLibrary.Exceptions;
using ServiceLibrary.Helpers;
using ServiceLibrary.Models.Jobs;

namespace ServiceLibrary.Repositories
{
    public class JobsRepository
    {
        private readonly ISqlConnectionWrapper _connectionWrapper;

        public JobsRepository() :
            this
            (
                new SqlConnectionWrapper(ServiceConstants.RaptorMain)
            )
        {
        }

        internal JobsRepository
        (
            ISqlConnectionWrapper connectionWrapper
        )
        {
            _connectionWrapper = connectionWrapper;
        }

        public async Task<int?> AddJobMessage(JobType type, bool hidden, string parameters, string receiverJobServiceId,
            int? projectId, string projectLabel, int userId, string userName, string hostUri)
        {
            var jobMessage = await AddJobMessageQuery(type, hidden, parameters,
                receiverJobServiceId, projectId, projectLabel, userId, userName, hostUri);

            if (jobMessage == null)
            {
                return null;
            }

            return jobMessage.JobMessageId;
        }

        private async Task<DJobMessage> AddJobMessageQuery(JobType type, bool hidden, string parameters, string receiverJobServiceId,
            int? projectId, string projectLabel, int userId, string userName, string hostUri)
        {

            var param = new DynamicParameters();

            param.Add("@type", (long)type);
            param.Add("@hidden", hidden);
            param.Add("@parameters", parameters);
            param.Add("@receiverJobServiceId", receiverJobServiceId);
            param.Add("@userId", userId);
            param.Add("@userLogin", userName);
            param.Add("@projectId", projectId);
            param.Add("@projectLabel", projectLabel);
            param.Add("@hostUri", hostUri);

            try
            {
                return (await _connectionWrapper.QueryAsync<DJobMessage>("AddJobMessage", param, commandType: CommandType.StoredProcedure)).SingleOrDefault();
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
