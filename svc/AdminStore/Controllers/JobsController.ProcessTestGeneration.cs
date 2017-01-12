using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using ServiceLibrary.Attributes;
using ServiceLibrary.Exceptions;
using ServiceLibrary.Helpers;
using ServiceLibrary.Models.Jobs;

namespace AdminStore.Controllers
{
	public partial class JobsController
	{
        /// <summary>
        /// Schedules a job for test generation from the provided processes
        /// </summary>
        /// <remarks>
        /// Schedules a test generation job.
        /// </remarks>
        /// <response code="200">OK.</response>
        /// <response code="400">BadRequest.</response>
        /// <response code="401">Unauthorized.</response>
        /// <response code="404">NotFound.</response>
        /// <response code="500">InternalServerError.</response>
        [HttpPost]
        [Route("process/testgen"), SessionRequired]
        [ResponseType(typeof(JobInfo))]
        public async Task<IHttpActionResult> QueueGenerateProcessTestsJob([FromBody] GenerateProcessTestsJobParameters request)
        {
            ValidateRequest(request);

            var session = GetSession(Request);    
            var parameters = SerializationHelper.ToXml(request);
            var hostUri = GetBaseHostUri();
            var queuedJobInfo = await _jobsRepository.AddJobMessage(JobType.GenerateProcessTests, 
                false, 
                parameters , 
                null, 
                request.ProjectId,
                request.ProjectName, 
                session.UserId, 
                session.UserName,
                hostUri.ToString());

            if (queuedJobInfo == null)
            {
                return InternalServerError();
            }

            return Ok(queuedJobInfo);
	    }

        #region private methods

	    void ValidateRequest(GenerateProcessTestsJobParameters request)
	    {
	        if (request?.Processes == null || !request.Processes.Any() || request.ProjectId <= 0)
	        {
	            throw new BadRequestException();
	        }
	    }

        Uri GetBaseHostUri()
        {
            var hostUri = Request.RequestUri;
            var builder = new UriBuilder();
            builder.Scheme = hostUri.Scheme;
            builder.Host = hostUri.Host;
            builder.Port = hostUri.Port;
            return builder.Uri;
        }
	    #endregion
    }
}