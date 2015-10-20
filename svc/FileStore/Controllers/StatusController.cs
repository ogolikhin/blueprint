using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using FileStore.Repositories;
using StatusControl.Repositories;

namespace FileStore.Controllers
{
    [RoutePrefix("status")]
    public class StatusController : ApiController
    {
        private readonly IStatusRepository _statusRepo;

        public StatusController() : this(new ConfigRepository())
        {
        }

        internal StatusController(IConfigRepository configRepository) : this(configRepository.FileStoreDatabase, "GetStatus") { }

        internal StatusController(string cxn, string cmd) : this(new SqlStatusRepository(cxn, cmd)) { }

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
                return result ? 
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
