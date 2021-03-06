﻿using ServiceLibrary.Helpers;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace ServiceLibrary.Repositories
{
    public class JobExecutorStatusRepository : IStatusRepository
    {
        private readonly string _dbSchema;
        private readonly ISqlConnectionWrapper _connectionWrapper;

        public List<StatusResponse> statusResponses = new List<StatusResponse>();

        public string Name { get; set; }

        public string AccessInfo { get; set; }

        private enum JobExecutorStatusEnum
        {
            Stopped = 0,
            Started = 1,
            Idle = 2,
            Active = 3,
            NotResponding = 4,
        };

        public JobExecutorStatusRepository(string cxn, string name, string dbSchema = ServiceConstants.DefaultDBSchema)
            : this(new SqlConnectionWrapper(cxn), cxn, name)
        {
            _dbSchema = dbSchema;
        }

        internal JobExecutorStatusRepository(ISqlConnectionWrapper connectionWrapper, string accessInfo, string name)
        {
            _connectionWrapper = connectionWrapper;
            Name = name;
            AccessInfo = accessInfo;
        }

        private StatusResponse ParseStatus(JobExecutorModel jobex)
        {
            string jobExecutorStatus = string.Format(CultureInfo.InvariantCulture, "{0} ({1})", jobex.Status, Enum.GetName(typeof(JobExecutorStatusEnum), jobex.Status));
            var timeSpanSinceLastActivity = jobex.CurrentTimestamp.Subtract(jobex.LastActivityTimestamp).TotalMinutes;
            var responseData = new StatusResponse();

            try
            {
                responseData.Name = "JobExecutor-" + jobex.JobServiceId.Remove(jobex.JobServiceId.LastIndexOf("@", StringComparison.Ordinal));
                responseData.AccessInfo = AccessInfo;
                responseData.NoErrors = jobex.Status != (int)JobExecutorStatusEnum.NotResponding;
                responseData.Result = System.String.Format(CultureInfo.InvariantCulture,
                    "JobName={0}, Platform= {1}, Type={2}, Status = {3}, LastActivityTimestamp={4}, ExecutingJobMessageId={5}, CurrentTimestamp={6}",
                    jobex.JobServiceId, jobex.Platform, jobex.Types, jobExecutorStatus, jobex.LastActivityTimestamp, jobex.ExecutingJobMessageId, jobex.CurrentTimestamp);
            }
            catch (Exception ex)
            {
                responseData.Name = "JobExecutor";
                responseData.AccessInfo = AccessInfo;
                responseData.NoErrors = false;
                responseData.Result = ex.ToString();
            }

            return responseData;
        }

        public async Task<List<StatusResponse>> GetStatuses(int timeout)
        {
            try
            {
                var query = await _connectionWrapper.QueryAsync<JobExecutorModel>("GetJobServices", commandType: CommandType.StoredProcedure);
                var result = query.ToList();
                if (result.Count > 0)
                {
                    foreach (var jobex in result)
                    {
                        statusResponses.Add(ParseStatus(jobex));
                    }
                }
                else
                {
                    statusResponses.Add(new StatusResponse() { Name = Name, AccessInfo = AccessInfo, Result = "No Executor Found at Database", NoErrors = false });

                }
            }
            catch (Exception ex)
            {
                statusResponses.Add(new StatusResponse() { Name = Name, AccessInfo = AccessInfo, Result = ex.ToString(), NoErrors = false });
            }

            return statusResponses;
        }
    }
}

public class JobExecutorModel
{
    public int Types { get; set; }

    public int Platform { get; set; }

    public string JobServiceId { get; set; }

    public int Status { get; set; }

    public string ExecutingJobMessageId { get; set; }

    public DateTime LastActivityTimestamp { get; set; }

    public DateTime CurrentTimestamp { get; set; }
}
