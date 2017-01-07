using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using ServiceLibrary.Attributes;
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
        [HttpPost, NoCache]
        [Route("process/testgen"), SessionRequired]
        [ResponseType(typeof(ProcessTestGenerationResult))]
        public async Task<IHttpActionResult> GenerateProcessTests([FromBody] ProcessTestGenerationRequest request)
        {
            await Task.FromResult(true);

            return Ok(new ProcessTestGenerationResult
	        {
	            JobId = -1
	        });
	    }
	}
}