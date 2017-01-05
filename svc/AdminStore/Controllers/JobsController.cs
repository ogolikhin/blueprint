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
using ServiceLibrary.Repositories;
using ServiceLibrary.Repositories.ConfigControl;
using AdminStore.Helpers;

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

        internal JobsController
        (
            IJobsRepository jobsRepository, 
            IServiceLogRepository serviceLogRepository,
            IUsersRepository sqlUserRepository
        ) : base(serviceLogRepository)
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
        /// <response code="400">BadRequest.</response>
        [HttpGet, NoCache]
        [Route(""), SessionRequired]
        [ResponseType(typeof(IEnumerable<JobInfo>))]
        public async Task<IHttpActionResult> GetLatestJobs(int? page = null, int? pageSize = null, JobType jobType = JobType.None)
        {
            int? userId = ValidateAndExtractUserId();
            try
            {
                bool isUserInstanceAdmin = await _sqlUserRepository.IsInstanceAdmin(false, userId.Value);
                if (isUserInstanceAdmin)
                {
                    userId = null;
                }

                JobsValidationHelper jobsHelper = new JobsValidationHelper();
                var validationMessage = jobsHelper.Validate(page, pageSize);
                if (!String.IsNullOrEmpty(validationMessage))
                {
                    return BadRequest(validationMessage);
                }

                int offset = (page.Value - 1) * pageSize.Value;

                return Ok(await _jobsRepository.GetVisibleJobs(userId, offset, pageSize, jobType));
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
        public async Task<IHttpActionResult> GetJob(int jobId)
        {
            int? userId = ValidateAndExtractUserId();
            try
            {
                bool isUserInstanceAdmin = await _sqlUserRepository.IsInstanceAdmin(false, userId.Value);
                if (isUserInstanceAdmin)
                {
                    userId = null;
                }

                return Ok(await _jobsRepository.GetJob(jobId, userId));
            }
            catch (Exception exception)
            {
                await Log.LogError(LogSource, exception);
                throw;
            }
        }

        #endregion


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
