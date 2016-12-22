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

namespace AdminStore.Controllers
{
    [ApiControllerJsonConfig]
    [RoutePrefix("jobs")]
    [BaseExceptionFilter]

    public class JobsController : LoggableApiController
    {
        private readonly IJobsRepository _jobsRepository;

        public override string LogSource => "AdminStore.JobsService";

        public JobsController(IJobsRepository jobsRepository) :
            this(jobsRepository, new ServiceLogRepository())
        {
            _jobsRepository = jobsRepository;
        }

        internal JobsController(IJobsRepository jobsRepository, 
            IServiceLogRepository serviceLogRepository) : base(serviceLogRepository)
        {
            _jobsRepository = jobsRepository;
        }

        public async Task<IList<JobInfo>> GetLatestJobs(int? minId = null, int ? offsetId = null, int? limit = null)
        {
            var userId = ValidateAndExtractUserId();
            return await _jobsRepository.GetVisibleJobs(userId, minId, offsetId, limit);
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