using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using System.Web.Http.Results;
using FileStore.Repositories;
using ServiceLibrary.Repositories;

namespace FileStore.Controllers
{
    [RoutePrefix("status")]
    public class StatusController : ApiController
    {
        internal readonly IStatusRepository _statusRepo;

        public StatusController() : this(new SqlStatusRepository(new ConfigRepository().FileStoreDatabase, "GetStatus"))
        {
        }

        internal StatusController(IStatusRepository statusRepo)
        {
            _statusRepo = statusRepo;
        }

        [HttpGet]
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
