using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using AdminStore.Repositories.Jobs;
using ServiceLibrary.Attributes;
using ServiceLibrary.Exceptions;
using ServiceLibrary.Helpers;
using ServiceLibrary.Models;
using ServiceLibrary.Models.Jobs;
using ServiceLibrary.Repositories.ConfigControl;

namespace AdminStore.Controllers
{
    [ApiControllerJsonConfig]
    [RoutePrefix("jobs")]
    [BaseExceptionFilter]
    public class JobsController : LoggableApiController
    {
        internal readonly IJobsRepository _jobsRepository;

        public override string LogSource => "AdminStore.JobsService";

        public JobsController() : this(new JobsRepository(), new ServiceLogRepository())
        {
        }

        internal JobsController(IJobsRepository jobsRepository, IServiceLogRepository serviceLogRepository) : base(serviceLogRepository)
        {
            _jobsRepository = jobsRepository;
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
            try
            {
                int jobPageSize = GetPageSize(pageSize);
                int jobPage = GetPage(page, 1, 1);
                int offset = (jobPage - 1) * jobPageSize;

                return await _jobsRepository.GetVisibleJobs(GetAuthenticatedUserIdFromSession(), offset, jobPageSize, jobType);
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
            try
            {
                return await _jobsRepository.GetJob(jobId, GetAuthenticatedUserIdFromSession());
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

        private int GetAuthenticatedUserIdFromSession()
        {
            object sessionValue;
            if (!Request.Properties.TryGetValue(ServiceConstants.SessionProperty, out sessionValue))
            {
                throw new AuthenticationException("Authorization is required", ErrorCodes.UnauthorizedAccess);
            }

            return ((Session)sessionValue).UserId;
        }
    }
}
