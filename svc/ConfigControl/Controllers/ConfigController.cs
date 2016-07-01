using System.Net.Http;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using System.Net;
using ConfigControl.Repositories;
using ServiceLibrary.Attributes;

namespace ConfigControl.Controllers
{
    [ApiControllerJsonConfig]
    [RoutePrefix("settings")]
    public class ConfigController : ApiController
    {
        internal readonly IConfigRepository _configRepo;

        public ConfigController() : this(new SqlConfigRepository())
        {
        }

        internal ConfigController(IConfigRepository configRepo)
        {
            _configRepo = configRepo;
        }

        [HttpGet, NoCache]
        [Route("{allowRestricted}")]
        [ResponseType(typeof(HttpResponseMessage))]
        public async Task<IHttpActionResult> GetConfig(bool allowRestricted)
        {
            try
            {
                var settings = await _configRepo.GetSettings(allowRestricted);
                var map = settings.GroupBy(it => it.Group).ToDictionary(it=>it.Key, it=>it.ToDictionary(i=>i.Key, i=>i.Value));

                var response = Request.CreateResponse(HttpStatusCode.OK, map);
                return ResponseMessage(response);
            }
            catch
            {
                return InternalServerError();
            }
        }
    }
}
