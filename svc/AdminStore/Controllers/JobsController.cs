using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
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
using AdminStore.Helpers;

namespace AdminStore.Controllers
{
    [ApiControllerJsonConfig]
    [RoutePrefix("jobs")]
    [BaseExceptionFilter]
    public partial class JobsController : LoggableApiController
    {
        private const string ContentDispositionType = "attachment";

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
        /// <response code="400">BadRequest.</response>
        [HttpGet, NoCache]
        [Route(""), SessionRequired]
        [ResponseType(typeof(IEnumerable<JobInfo>))]
        public async Task<IHttpActionResult> GetLatestJobs(int? page = null, int? pageSize = null, JobType jobType = JobType.None)
        {
            try
            {
                var session = GetSession(Request);

                JobsValidationHelper jobsHelper = new JobsValidationHelper();
                jobsHelper.Validate(page, pageSize);

                int offset = (page.Value - 1) * pageSize.Value;

                return Ok(await _jobsRepository.GetVisibleJobs(session.UserId, offset, pageSize, jobType));
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
            try
            {
                var session = GetSession(Request);

                return Ok(await _jobsRepository.GetJob(jobId, session.UserId));
            }
            catch (Exception exception)
            {
                await Log.LogError(LogSource, exception);
                throw;
            }
        }

        [HttpGet, NoCache]
        [Route("{jobId:int:min(1)}/result/file"), SessionRequired(true)]
        public async Task<IHttpActionResult> GetJobResultFile(int jobId)
        {
            var session = GetSession(Request);
            var baseAddress = WebApiConfig.FileStore != null ? new Uri(WebApiConfig.FileStore) : Request.RequestUri;
            var file = await _jobsRepository.GetJobResultFile(jobId, session.UserId, baseAddress, Session.Convert(session.SessionId));

            var response = Request.CreateResponse(HttpStatusCode.OK);
            response.Content = new StreamContent(file.ContentStream);
            response.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue(ContentDispositionType);
            response.Content.Headers.ContentDisposition.FileName = file.Info.Name;
            response.Content.Headers.ContentDisposition.FileNameStar = file.Info.Name;
            response.Content.Headers.ContentType = new MediaTypeHeaderValue(file.Info.Type);
            response.Content.Headers.ContentLength = file.Info.Size;

            return ResponseMessage(response);
        }

        #endregion

        private Session GetSession(HttpRequestMessage request)
        {
            object sessionValue;
            if (!request.Properties.TryGetValue(ServiceConstants.SessionProperty, out sessionValue))
            {
                throw new AuthenticationException("Authorization is required", ErrorCodes.UnauthorizedAccess);
            }

            return (Session)sessionValue;
        }
    }
}
