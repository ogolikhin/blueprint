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
        [ResponseType(typeof(AddJobResult))]
        public async Task<IHttpActionResult> QueueGenerateProcessTestsJob([FromBody] GenerateProcessTestsJobParameters request)
        {
            ValidateRequest(request);

            var session = GetSession(Request);    
            var parameters = SerializationHelper.ToXml(request.Processes);
            var hostUri = ServerUriHelper.BaseHostUri;
            var jobId = await _jobsRepository.AddJobMessage(JobType.GenerateProcessTests, 
                false, 
                parameters , 
                null, 
                request.ProjectId,
                request.ProjectName, 
                session.UserId, 
                session.UserName,
                hostUri.ToString());

            if (!jobId.HasValue)
            {
                return InternalServerError();
            }

            return Ok(new AddJobResult() { JobMessageId = jobId.Value });
	    }

        #region private methods

	    void ValidateRequest(GenerateProcessTestsJobParameters request)
	    {
	        if (request?.Processes == null || !request.Processes.Any() || request.ProjectId <= 0)
	        {
	            throw new BadRequestException();
	        }
	    }
	    #endregion
    }
}