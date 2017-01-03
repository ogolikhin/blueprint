using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Http;
using AdminStore.Repositories.Jobs;
using ServiceLibrary.Attributes;
using ServiceLibrary.Exceptions;
using ServiceLibrary.Helpers;
using ServiceLibrary.Models;
using ServiceLibrary.Models.Jobs;
using ServiceLibrary.Repositories.ConfigControl;
using System.Web.Http.Description;
using ServiceLibrary.Repositories;

namespace AdminStore.Controllers
{
    [ApiControllerJsonConfig]
    [RoutePrefix("jobs")]
    [BaseExceptionFilter]

    public class JobsController : LoggableApiController
    {
        internal readonly IJobsRepository _jobsRepository;
        internal readonly IUsersRepository _sqlUserRepository;

        public override string LogSource => "AdminStore.JobsService";

        public JobsController() :
            this(new JobsRepository(), new ServiceLogRepository(), new SqlUsersRepository())
        {
        }

        internal JobsController(IJobsRepository jobsRepository, 
            IServiceLogRepository serviceLogRepository,
            IUsersRepository sqlUserRepository) : base(serviceLogRepository)
        {
            _jobsRepository = jobsRepository;
            _sqlUserRepository = sqlUserRepository;
        }
        #region public methods
        /// <summary>
        /// GetLatestJobs
        /// </summary>
        /// <remarks>
        /// Returns the latest jobs.
        /// </remarks>
        /// <response code="200">OK.</response>
        [HttpGet, NoCache]
        [Route(""), SessionRequired]
        [ResponseType(typeof(IEnumerable<JobInfo>))]
        public async Task<IEnumerable<JobInfo>> GetLatestJobs(int? page = 1, int? pageSize = null, JobType jobType = JobType.None)
        {
            int? userId = ValidateAndExtractUserId();
            try
            {
                bool isUserInstanceAdmin = await _sqlUserRepository.IsInstanceAdmin(false, userId.Value);
                if (isUserInstanceAdmin)
                {
                    userId = null;
                }
                int jobPageSize = GetPageSize(pageSize);
                int jobPage = GetPage(page, 1, 1);
                int offset = (jobPage - 1) * jobPageSize;
                return await _jobsRepository.GetVisibleJobs(userId, offset, jobPageSize, jobType);
            }
            catch (Exception exception)
            {
                await Log.LogError(LogSource, exception);
                throw;
            }
        }

        /// <summary>
        /// GetJob
        /// </summary>
        /// <remarks>
        /// GetJob
        /// </remarks>
        /// <response code="200">OK.</response>
        [HttpGet, NoCache]
        [Route("{jobId:int:min(1)}"), SessionRequired]
        [ResponseType(typeof(JobInfo))]
        public async Task<JobInfo> GetJob(int jobId)
        {
            int? userId = ValidateAndExtractUserId();
            try
            {
                bool isUserInstanceAdmin = await _sqlUserRepository.IsInstanceAdmin(false, userId.Value);
                if (isUserInstanceAdmin)
                {
                    userId = null;
                }
                return await _jobsRepository.GetJob(jobId, userId);
            }
            catch (Exception exception)
            {
                await Log.LogError(LogSource, exception);
                throw;
            }
        }
        #endregion
        private int GetPageSize(int? pageSize)
        {
            var jobPageSize = pageSize.GetValueOrDefault(WebApiConfig.JobDetailsPageSize);
            if (jobPageSize <= 0)
            {
                return WebApiConfig.JobDetailsPageSize;
            }
            return jobPageSize;
        }

        private int GetPage(int? requestedPage, int minPage, int defaultPage)
        {
            int page = requestedPage.GetValueOrDefault(defaultPage);
            return page < minPage ? defaultPage : page;
        }
        private int ValidateAndExtractUserId()
        {
            // get the UserId from the session
            object sessionValue;
            if (!Request.Properties.TryGetValue(ServiceConstants.SessionProperty, out sessionValue))
            {
                throw new AuthenticationException("Authorization is required", ErrorCodes.UnauthorizedAccess);
            }
            return ((Session)sessionValue).UserId;
        }
    }
}