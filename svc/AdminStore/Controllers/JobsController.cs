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

namespace AdminStore.Controllers
{
    [ApiControllerJsonConfig]
    [RoutePrefix("jobs")]
    [BaseExceptionFilter]
    public class JobsController : LoggableApiController
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
        [HttpGet, NoCache]
        [Route(""), SessionRequired]
        [ResponseType(typeof(IEnumerable<JobInfo>))]
        public async Task<IEnumerable<JobInfo>> GetLatestJobs(int? page = 1, int? pageSize = null, JobType jobType = JobType.None)
        {
            try
            {
                var session = GetAuthenticatedSessionFromRequest();
                int jobPageSize = GetPageSize(pageSize);
                int jobPage = GetPage(page, 1, 1);
                int offset = (jobPage - 1) * jobPageSize;

                return await _jobsRepository.GetVisibleJobs(session.UserId, offset, jobPageSize, jobType);
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
                var session = GetAuthenticatedSessionFromRequest();
                return await _jobsRepository.GetJob(jobId, session.UserId);
            }
            catch (Exception exception)
            {
                await Log.LogError(LogSource, exception);
                throw;
            }
        }

        [HttpGet, NoCache]
        [Route("{jobId:int:min(1)}/result/file"), SessionRequired]
        public async Task<HttpResponseMessage> GetJobResultFile(int jobId)
        {
            var session = GetAuthenticatedSessionFromRequest();
            var file = await _jobsRepository.GetJobResultFile(jobId, session.UserId, session.SessionId.ToStringInvariant());

            var response = Request.CreateResponse(HttpStatusCode.OK);
            response.Content = new StreamContent(file.ContentStream);
            response.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue(ContentDispositionType);
            response.Content.Headers.ContentDisposition.FileName = file.Info.Name;
            response.Content.Headers.ContentDisposition.FileNameStar = file.Info.Name;
            response.Content.Headers.ContentType = new MediaTypeHeaderValue(file.Info.Type);
            response.Content.Headers.ContentLength = file.Info.Size;

            return response;
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

        private Session GetAuthenticatedSessionFromRequest()
        {
            object sessionValue;
            if (!Request.Properties.TryGetValue(ServiceConstants.SessionProperty, out sessionValue))
            {
                throw new AuthenticationException("Authorization is required", ErrorCodes.UnauthorizedAccess);
            }

            return (Session)sessionValue;
        }
    }
}
