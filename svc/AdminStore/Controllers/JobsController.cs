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

namespace AdminStore.Controllers
{
    [ApiControllerJsonConfig]
    [RoutePrefix("jobs")]
    [BaseExceptionFilter]

    public class JobsController : LoggableApiController
    {
        private readonly IJobsRepository _jobsRepository;

        public override string LogSource => "AdminStore.JobsService";

        public JobsController() :
            this(new JobsRepository(), new ServiceLogRepository())
        {
        }

        internal JobsController(IJobsRepository jobsRepository, 
            IServiceLogRepository serviceLogRepository) : base(serviceLogRepository)
        {
            _jobsRepository = jobsRepository;
        }

        /// <summary>
        /// GetLatestJobs
        /// </summary>
        /// <remarks>
        /// Returns the latest jobs.
        /// </remarks>
        /// <response code="200">OK.</response>
        [HttpGet]
        [Route("latest"), SessionRequired]
        [ResponseType(typeof(IEnumerable<JobInfo>))]
        public async Task<IList<JobInfo>> GetLatestJobs(int? minId = null, int ? offsetId = null, int? limit = null)
        {
            var userId = ValidateAndExtractUserId();
            try
            {
                return await _jobsRepository.GetVisibleJobs(userId, minId, offsetId, limit);
            }
            catch (Exception exception)
            {
                await Log.LogError(LogSource, exception);
                throw;
            }
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