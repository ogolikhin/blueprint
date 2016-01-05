using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using System.Web.Http.Results;
using ServiceLibrary.Repositories;
using ConfigControl.Repositories;
using ServiceLibrary.Attributes;

namespace ConfigControl.Controllers
{
    [RoutePrefix("status")]
    public class StatusController : ApiController
    {
        internal readonly IStatusRepository StatusRepo;

        public StatusController() : this(new SqlStatusRepository(SqlConfigRepository.AdminStorageDatabase, "GetStatus"))
        {
        }

        internal StatusController(IStatusRepository statusRepo)
        {
            StatusRepo = statusRepo;
        }

        [HttpGet, NoCache]
        [Route("")]
        [ResponseType(typeof(HttpResponseMessage))]
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
