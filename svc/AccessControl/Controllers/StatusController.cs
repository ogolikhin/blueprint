using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using System.Web.Http.Results;
using ServiceLibrary.Repositories;

namespace AccessControl.Controllers
{
    [RoutePrefix("status")]
    public class StatusController : ApiController
    {
        internal readonly IStatusRepository StatusRepo;

        public static ManualResetEventSlim Ready { get; }

        static StatusController()
        {
            Ready = new ManualResetEventSlim(false);
        }

        public StatusController() : this(new SqlStatusRepository(WebApiConfig.AdminStorage, "GetStatus"))
        {
        }

        internal StatusController(IStatusRepository statusRepo)
        {
            StatusRepo = statusRepo;
        }

        [HttpGet]
        [Route("")]
        [ResponseType(typeof(HttpResponseMessage))]
        public async Task<IHttpActionResult> GetStatus()
        {
            try
            {
                var result = Ready.IsSet && await StatusRepo.GetStatus();
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
