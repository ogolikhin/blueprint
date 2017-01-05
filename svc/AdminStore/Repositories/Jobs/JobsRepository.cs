﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using ServiceLibrary.Exceptions;
using ServiceLibrary.Helpers;
using ServiceLibrary.Models;
using ServiceLibrary.Models.Files;
using ServiceLibrary.Models.Jobs;
using ServiceLibrary.Models.Messaging;
using ServiceLibrary.Repositories;

namespace AdminStore.Repositories.Jobs
{
    public class JobsRepository : IJobsRepository
    {
        internal readonly ISqlConnectionWrapper ConnectionWrapper;
        internal readonly ISqlArtifactRepository _sqlArtifactRepository;
        internal readonly IArtifactPermissionsRepository _artifactPermissionsRepository;
        internal readonly IUsersRepository _usersRepository;
        internal readonly IFileRepository _fileRepository;

        public JobsRepository() : 
            this
            (
                new SqlConnectionWrapper(ServiceConstants.RaptorMain),
                new SqlArtifactRepository(),
                new SqlArtifactPermissionsRepository(),
                new SqlUsersRepository(),
                new FileRepository()
            )
        {
        }

        internal JobsRepository
        (
            ISqlConnectionWrapper connectionWrapper, 
            ISqlArtifactRepository sqlArtifactRepository,
            IArtifactPermissionsRepository artifactPermissionsRepository,
            IUsersRepository userRepository,
            IFileRepository fileRepository
        )
        {
            ConnectionWrapper = connectionWrapper;
            _sqlArtifactRepository = sqlArtifactRepository;
            _artifactPermissionsRepository = artifactPermissionsRepository;
            _usersRepository = userRepository;
            _fileRepository = fileRepository;
        }

        #region Public Methods

        public async Task<IEnumerable<JobInfo>> GetVisibleJobs
        (
            int userId, 
            int? offset, 
            int? limit, 
            JobType? jobType = JobType.None
        )
        {
            var actualUserId = await GetActualUserId(userId);
            var dJobMessages = await GetJobMessages(actualUserId, offset, limit, jobType, false);
            var systemMessageMap = await GetRelevantUnfinishCancelSystemJobSystemMessageMap(dJobMessages.Select(job => job.JobMessageId), true);
            var projectNameIdMap = await GetProjectNamesForUserMapping(
                dJobMessages.Where(job => job.ProjectId.HasValue).Select(job => job.ProjectId.Value).Distinct(), actualUserId);

            return dJobMessages.Select(job => GetJobInfo(job, systemMessageMap, projectNameIdMap));
        }

        public async Task<JobInfo> GetJob(int jobId, int userId)
        {
            var actualUserId = await GetActualUserId(userId);

            var job = await GetJobMessage(jobId);
            if (job == null)
            {
                return null;
            }

            var systemMessageMap = await GetRelevantUnfinishCancelSystemJobSystemMessageMap(new[] { jobId });
            var projectNameMappings = job.ProjectId.HasValue ? 
                await GetProjectNamesForUserMapping(new[] { job.ProjectId.Value }, actualUserId) : 
                new Dictionary<int, string>();

            return GetJobInfo(job, systemMessageMap, projectNameMappings);
        }

        public async Task<File> GetJobResultFile(Uri baseUri, int jobId, int userId, string sessionToken)
        {
            var job = await GetJob(jobId, userId);
            if (job == null)
            {
                throw new ResourceNotFoundException("Job is not found", ErrorCodes.ResourceNotFound);
            }

            if (job.Status != JobStatus.Completed)
            {
                throw new BadRequestException("Job is not completed", ErrorCodes.JobNotCompleted);
            }

            if (string.IsNullOrEmpty(job.Result))
            {
                throw new ResourceNotFoundException("Job doesn't have a result", ErrorCodes.ResourceNotFound);
            }

            switch (job.JobType)
            {
                case JobType.ProjectExport:
                    var projectExportTaskStatus = SerializationHelper.FromXml<ProjectExportTaskStatus>(job.Result);
                    var file = await _fileRepository.GetFileAsync(baseUri, projectExportTaskStatus.Details.FileGuid, sessionToken);
                    if (file == null)
                    {
                        throw new ResourceNotFoundException("Job doesn't have a result file", ErrorCodes.ResourceNotFound);
                    }

                    return file;

                default:
                    throw new BadRequestException("Job doesn't support downloadable result files", ErrorCodes.JobResultFileNotSupported);
            }
        }

        #endregion

        #region Private Methods

        private async Task<int?> GetActualUserId(int userId)
        {
            return await _usersRepository.IsInstanceAdmin(false, userId) ? (int?)null : userId;
        }

        private async Task<DJobMessage> GetJobMessage(int jobId)
        {
            var param = new DynamicParameters();

            param.Add("@jobMessageId", jobId);

            try
            {
                return (await ConnectionWrapper.QueryAsync<DJobMessage>("GetJobMessage", param, commandType: CommandType.StoredProcedure)).SingleOrDefault();
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

        private async Task<IEnumerable<DJobMessage>> GetJobMessages
        (
            int? userId,
            int? offset,
            int? limit,
            JobType? jobType = JobType.None,
            bool? hidden = null,
            bool? addFinished = true,
            bool? doNotFetchResult = false
        )
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

        private JobInfo GetJobInfo
        (
            DJobMessage jobMessage,
            IDictionary<int, List<SystemMessage>> systemMessageMap,
            IDictionary<int, string> projectNameMap
        )
        {
            return new JobInfo
            {
                UserDisplayName = jobMessage.DisplayName,
                JobId = jobMessage.JobMessageId,
                SubmittedDateTime = jobMessage.SubmittedTimestamp.Value,
                JobStartDateTime = jobMessage.StartTimestamp,
                JobEndDateTime = jobMessage.EndTimestamp,
                JobType = jobMessage.Type,
                Progress = jobMessage.Progress,
                Project = jobMessage.ProjectId.HasValue ? projectNameMap[jobMessage.ProjectId.Value] : jobMessage.ProjectLabel,
                Server = jobMessage.ExecutorJobServiceId,
                Status = jobMessage.Status.Value,
                UserId = jobMessage.UserId,
                Output = jobMessage.StatusDescription,
                StatusChanged = jobMessage.StatusChangedTimestamp != null,
                HasCancelJob = systemMessageMap.ContainsKey(jobMessage.JobMessageId),
                ProjectId = jobMessage.ProjectId,
                Result = jobMessage.Result
            };
        }

        // Copied from raptor JobMessageDataProvider
        private async Task<IDictionary<int, List<SystemMessage>>> GetRelevantUnfinishCancelSystemJobSystemMessageMap
        (
            IEnumerable<int> jobIds, 
            bool? doNotFetchResult = false
        )
        {
            var allUnfinishSystemJobs = await GetJobMessages(null, 0, int.MaxValue, JobType.System, true, false, doNotFetchResult);

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

        private async Task<Dictionary<int, string>> GetProjectNamesForUserMapping(IEnumerable<int> projectIds, int? userId)
        {
            var projectNameIdDictionary = (await _sqlArtifactRepository.GetProjectNameByIds(projectIds)).ToDictionary(x => x.ItemId, x => x.Name);
            if (!userId.HasValue)
            {
                return projectNameIdDictionary;
            }

            var projectIdPermissions = new List<KeyValuePair<int, RolePermissions>>();

            int iterations = (int)Math.Ceiling((double)projectIds.Count() / 50);

            for (int i = 0; i < iterations; i++)
            {
                var chunkProjectIds = projectIds.Skip(i * 50).Take(50);
                var newDictionary = await _artifactPermissionsRepository.GetArtifactPermissions(chunkProjectIds, userId.Value);
                projectIdPermissions.AddRange(newDictionary.ToList());
            }

            var projectIdPermissionsDictionary = projectIdPermissions.ToDictionary(x => x.Key, x => x.Value);

            foreach (int projectId in projectIds)
            {
                if (!projectIdPermissionsDictionary.ContainsKey(projectId) || !projectIdPermissionsDictionary[projectId].HasFlag(RolePermissions.Read))
                {
                    projectNameIdDictionary[projectId] = ServiceConstants.NoPermissions;
                }
            }

            return projectNameIdDictionary;
        }

        #endregion
    }
}
