using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using ServiceLibrary.Repositories;

namespace AccessControl.Controllers
{
    [RoutePrefix("status")]
    public class StatusController : ApiController
    {
        private readonly IStatusRepository _statusRepo;

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
                return result && Ready.IsSet ?
                    (IHttpActionResult)Ok() :
                    new System.Web.Http.Results.StatusCodeResult(System.Net.HttpStatusCode.ServiceUnavailable, Request);
            }
            catch
            {
                return InternalServerError();
            }
        }
    }
}
