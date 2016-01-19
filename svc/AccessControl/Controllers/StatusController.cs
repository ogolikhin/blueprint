using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using System.Web.Http.Results;
using ServiceLibrary.Attributes;
using ServiceLibrary.Repositories;

namespace AccessControl.Controllers
{
    [RoutePrefix("status")]
    public class StatusController : ApiController
    {
        internal readonly IStatusRepository StatusRepo;

        public StatusController() : this(new SqlStatusRepository(WebApiConfig.AdminStorage, "GetStatus"))
        {
        }

        internal StatusController(IStatusRepository statusRepo)
        {
            StatusRepo = statusRepo;
        }

        /// <summary>
        /// GetStatus
        /// </summary>
        /// <remarks>
        /// Returns the current status of AccessControl service.
        /// </remarks>
        /// <response code="200">OK.</response>
        /// <response code="500">Internal Server Error. An error occurred.</response>
        /// <response code="503">Service Unavailable.</response>
        [HttpGet, NoCache]
        [Route("")]
        [ResponseType(typeof(void))]
        public async Task<IHttpActionResult> GetStatus()
        {
            try
            {
                var result = await StatusRepo.GetStatus();
                if (result)
                {
                    return Ok();
                }
                return new StatusCodeResult(HttpStatusCode.ServiceUnavailable, Request);
            }
            catch
            {
                return InternalServerError();
            }
        }
    }
}
