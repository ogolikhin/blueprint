using System.Data;
using System.Linq;
using System.Threading.Tasks;
using ServiceLibrary.Helpers;
using System.Collections.Generic;
using System;
using ServiceLibrary.Repositories.ConfigControl;
using System.Globalization;

namespace ServiceLibrary.Repositories
{
    public class JobExecutorStatusRepository : IStatusRepository
    {
        public List<StatusResponse> statusResponses = new List<StatusResponse>() ;
        private readonly string _dbSchema;
        internal readonly ISqlConnectionWrapper _connectionWrapper;

        public string Name { get; set; }

        public string AccessInfo { get; set; }


        public JobExecutorStatusRepository(string cxn, string name,  string dbSchema = ServiceConstants.DefaultDBSchema)
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
            var timeSpanSinceLastActivity = jobex.CurrentTimestamp.Subtract(jobex.LastActivityTimestamp).TotalMinutes;
            var responseData = new StatusResponse();
            try {
                
                    responseData.Name = $"JobExecutor-" + jobex.JobServiceId.Remove(jobex.JobServiceId.LastIndexOf("@", StringComparison.Ordinal));
                    responseData.AccessInfo = AccessInfo;
                    responseData.NoErrors = jobex.Status == 2 && timeSpanSinceLastActivity <= 5 ? true : false;// if status = 2 and timeSpanSinceLastActivity <=5 min then NoErrors = true
                    responseData.Result = System.String.Format(CultureInfo.InvariantCulture, 
                        "JobName={0}, Platform= {1}, Type={2}, Status = {3}, LastActivityTimestamp={4}, ExecutingJobMessageId={5}, CurrentTimestamp={6}", 
                        jobex.JobServiceId, jobex.Platform, jobex.Types, jobex.Status == 2 ? "Active" : "Down", jobex.LastActivityTimestamp, jobex.ExecutingJobMessageId, jobex.CurrentTimestamp);

            }
            catch (Exception ex)
            {
                responseData.Name = "JobExecutor";
                responseData.AccessInfo = AccessInfo;
                responseData.NoErrors = false;
                responseData.Result = ex.ToString();
                //await Log.LogError(LogSource, ex);
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
                else {
                    statusResponses.Add(new StatusResponse(){Name = Name,AccessInfo = AccessInfo,Result = "No Executor Found at Database",NoErrors = false});
                    
                }
            }
            catch (Exception ex)
            {
                
                statusResponses.Add(new StatusResponse() { Name = Name, AccessInfo = AccessInfo, Result = ex.ToString(), NoErrors = false });
               
            }

            return  statusResponses;
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
