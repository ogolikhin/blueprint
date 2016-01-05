using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using System.Web.Http.Results;
using ServiceLibrary.Attributes;
using ServiceLibrary.Repositories;

namespace AdminStore.Controllers
{
    [RoutePrefix("status")]
    public class StatusController : ApiController
    {
        internal readonly IStatusRepository _statusRepo;

        public StatusController() : this(new SqlStatusRepository(WebApiConfig.AdminStorage, "GetStatus"))
        {
        }

        internal StatusController(IStatusRepository statusRepo)
        {
            _statusRepo = statusRepo;
        }

        [HttpGet, NoCache]
        [Route("")]
        [ResponseType(typeof(HttpResponseMessage))]
        public async Task<IHttpActionResult> GetStatus()
        {
            try
            {
                var result = await _statusRepo.GetStatus();
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
