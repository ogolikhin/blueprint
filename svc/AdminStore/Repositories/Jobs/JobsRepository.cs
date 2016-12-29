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
using ServiceLibrary.Models.Messaging;
using ServiceLibrary.Models;

namespace AdminStore.Repositories.Jobs
{
    public class JobsRepository : IJobsRepository
    {
        internal readonly ISqlConnectionWrapper ConnectionWrapper;
        internal readonly ISqlArtifactRepository _sqlArtifactRepository;
        internal readonly IArtifactPermissionsRepository _artifactPermissionsRepository;

        public JobsRepository() : 
            this(
                new SqlConnectionWrapper(ServiceConstants.RaptorMain), 
                new SqlArtifactRepository(), 
                new SqlArtifactPermissionsRepository())
        {
        }

        internal JobsRepository(
            ISqlConnectionWrapper connectionWrapper, 
            ISqlArtifactRepository sqlArtifactRepository,
            IArtifactPermissionsRepository artifactPermissionsRepository)
        {
            ConnectionWrapper = connectionWrapper;
            _sqlArtifactRepository = sqlArtifactRepository;
            _artifactPermissionsRepository = artifactPermissionsRepository;
        }

        #region Public Methods
        public async Task<IEnumerable<JobInfo>> GetVisibleJobs(int? userId, int? offset, int? limit, JobType? jobType = JobType.None)
        {
            var dJobMessages = await GetJobMessages(userId, offset, limit, jobType, false);
            var systemMessageMap = await GetRelevantUnfinishCancelSystemJobSystemMessageMap(dJobMessages.Select(job => job.JobMessageId), true);
            var projectNameIdMap = await GetProjectNamesForUserMapping(
                dJobMessages.Where(job => job.ProjectId.HasValue).Select(job => job.ProjectId.Value).Distinct(), userId);
            return dJobMessages.Select(job => GetJobInfo(job, systemMessageMap, projectNameIdMap));
        }
        #endregion

        #region Private Methods
        private async Task<IEnumerable<DJobMessage>> GetJobMessages(
            int? userId, 
            int? offset, 
            int? limit, 
            JobType? jobType = JobType.None, 
            bool? hidden = null, 
            bool? addFinished = true,
            bool? doNotFetchResult = false)
        {
            var param = new DynamicParameters();

            param.Add("@hidden", hidden);
            param.Add("@userId", userId);
            param.Add("@projectId", null);
            param.Add("@addFinished", addFinished);
            param.Add("@receiverJobServiceId", null);
            param.Add("@doNotFetchResult", doNotFetchResult);
            param.Add("@offset", offset ?? 0);
            param.Add("@limit", limit ?? WebApiConfig.JobDetailsPageSize);
            param.Add("@jobTypeFilter", jobType != JobType.None ? jobType : null);

            try
            {
                return (await ConnectionWrapper.QueryAsync<DJobMessage>("GetJobMessagesNova", param, commandType: CommandType.StoredProcedure));                                
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

        private JobInfo GetJobInfo(
            DJobMessage jobMessage, 
            IDictionary<int, List<SystemMessage>> systemMessageMap, 
            IDictionary<int, string> projectNameMap)
        {
            var jobInfo = new JobInfo();
            jobInfo.UserDisplayName = jobMessage.DisplayName;
            jobInfo.JobId = jobMessage.JobMessageId;
            jobInfo.SubmittedDateTime = jobMessage.SubmittedTimestamp.Value;
            jobInfo.JobStartDateTime = jobMessage.StartTimestamp;
            jobInfo.JobEndDateTime = jobMessage.EndTimestamp;
            jobInfo.JobType = jobMessage.Type;
            jobInfo.Progress = jobMessage.Progress;
            jobInfo.Project = jobMessage.ProjectId.HasValue ? projectNameMap[jobMessage.ProjectId.Value] : null;
            jobInfo.Server = jobMessage.ExecutorJobServiceId;
            jobInfo.Status = jobMessage.Status.Value;
            jobInfo.UserId = jobMessage.UserId;
            jobInfo.Output = jobMessage.StatusDescription;
            jobInfo.StatusChanged = (jobMessage.StatusChangedTimestamp != null);
            jobInfo.HasCancelJob = systemMessageMap.ContainsKey(jobMessage.JobMessageId);
            jobInfo.ProjectId = jobMessage.ProjectId;
            return jobInfo;
        }

        // Copied from raptor JobMessageDataProvider
        private async Task<IDictionary<int, List<SystemMessage>>> GetRelevantUnfinishCancelSystemJobSystemMessageMap(IEnumerable<int> jobIds, bool? doNotFetchResult = false)
        {
            var allUnfinishSystemJobs = await GetJobMessages(null, null, null, JobType.System, true, false, doNotFetchResult);
            return allUnfinishSystemJobs
                .Where(j => j.Parameters != null)
                .Where(j => j.Type == JobType.System)
                .Select(j => SerializationHelper.FromXml<SystemMessage>(j.Parameters))
                .Where(sysMsg => sysMsg != null)
                .Where(sysMsg => sysMsg.Command == SystemJobCommand.TerminateJob)
                .Where(sysMsg => sysMsg.TargetJobId.HasValue && jobIds.Contains(sysMsg.TargetJobId.Value))
                .GroupBy(sysMsg => sysMsg.TargetJobId ?? 0) //sysMsg.TargetJobId always has value
                .ToDictionary(g => g.Key, g => g.ToList());
        }

        private async Task<IDictionary<int, string>> GetProjectNamesForUserMapping(IEnumerable<int> projectIds, int? userId)
        {
            var projectNameIdDictionary = (await _sqlArtifactRepository.GetProjectNameByIds(projectIds)).ToDictionary(x => x.ItemId, x => x.Name);
            if (userId.HasValue)
            {
                var projectIdPermissions = new List<KeyValuePair<int, RolePermissions>>();

                int iterations = (int)Math.Ceiling((double)projectIds.Count()/50);

                for (int i = 0; i < iterations; i ++)
                {
                    var chunkProjectIds = projectIds.Skip(i * 50).Take(50);
                    var newDictionary = await _artifactPermissionsRepository.GetArtifactPermissions(chunkProjectIds, userId.Value);
                    projectIdPermissions.AddRange(newDictionary.ToList());
                }
                var projectIdPermissionsDictionary = projectIdPermissions.ToDictionary(x => x.Key, x => x.Value);

                foreach (int projectId in projectIds)
                {
                    if(!projectIdPermissionsDictionary.ContainsKey(projectId) || !projectIdPermissionsDictionary[projectId].HasFlag(RolePermissions.Read))
                    {
                        projectNameIdDictionary[projectId] = "<No Permission>";
                    }
                }
            }
            return projectNameIdDictionary;
        }
        #endregion
    }
}