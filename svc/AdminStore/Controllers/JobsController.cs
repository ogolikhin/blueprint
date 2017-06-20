using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using AdminStore.Helpers;
using AdminStore.Repositories.Jobs;
using ServiceLibrary.Attributes;
using ServiceLibrary.Controllers;
using ServiceLibrary.Exceptions;
using ServiceLibrary.Helpers;
using ServiceLibrary.Helpers.Files;
using ServiceLibrary.Models;
using ServiceLibrary.Models.Jobs;
using ServiceLibrary.Repositories.ConfigControl;
using ServiceLibrary.Repositories.Files;

namespace AdminStore.Controllers
{
    [ApiControllerJsonConfig]
    [RoutePrefix("jobs")]
    [BaseExceptionFilter]
    public partial class JobsController : LoggableApiController
    {
        internal readonly IJobsRepository _jobsRepository;
        internal readonly IFileRepository _fileRepository;

        public JobsController() : this(new JobsRepository(), new ServiceLogRepository())
        {
        }

        internal JobsController(IJobsRepository jobsRepository, IServiceLogRepository serviceLogRepository, IFileRepository fileRepository = null) : base(serviceLogRepository)
        {
            _jobsRepository = jobsRepository;
            _fileRepository = fileRepository;
        }

        public override string LogSource => "AdminStore.JobsService";

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
        [ResponseType(typeof(JobResult))]
        public async Task<IHttpActionResult> GetLatestJobs(int? page = null, int? pageSize = null, JobType jobType = JobType.None)
        {
            var session = ServerHelper.GetSession(Request);

            var jobsHelper = new JobsValidationHelper();
            jobsHelper.Validate(page, pageSize);

            var offset = (page.Value - 1) * pageSize.Value;

            return Ok(await _jobsRepository.GetVisibleJobs(session.UserId, offset, pageSize, jobType));
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
            var session = ServerHelper.GetSession(Request);

            return Ok(await _jobsRepository.GetJob(jobId, session.UserId));
        }

        /// <summary>
        /// Gets the result file of a supported completed job (eg Project Export)
        /// </summary>
        /// <param name="jobId">Job id</param>
        /// <response code="200">Ok.</response>
        /// <response code="400">BadRequest.</response>
        /// <response code="401">Unauthorized.</response>
        /// <response code="404">NotFound.</response>
        /// <response code="500">InternalServerError.</response>
        [HttpGet, NoCache]
        [Route("{jobId:int:min(1)}/result/file"), SessionRequired(true)]
        public async Task<IHttpActionResult> GetJobResultFile(int jobId)
        {
            var session = ServerHelper.GetSession(Request);
            var baseUri = WebApiConfig.FileStore != null ? new Uri(WebApiConfig.FileStore) : Request.RequestUri;
            var fileRepository = _fileRepository ?? new FileRepository(new FileHttpWebClient(baseUri, Session.Convert(session.SessionId)));

            var file = await _jobsRepository.GetJobResultFile(jobId, session.UserId, fileRepository);

            var response = Request.CreateResponse(HttpStatusCode.OK);
            response.Content = new StreamContent(file.ContentStream);
            response.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment")
            {
                FileName = file.Info.Name,
                FileNameStar = file.Info.Name
            };
            response.Content.Headers.ContentType = new MediaTypeHeaderValue(file.Info.Type);
            response.Content.Headers.ContentLength = file.Info.Size;

            return ResponseMessage(response);
        }

        #endregion
    }
}
