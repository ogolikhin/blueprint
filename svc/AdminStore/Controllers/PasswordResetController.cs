using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using AdminStore.Repositories;
using ServiceLibrary.Attributes;
using ServiceLibrary.Helpers;
using sl = ServiceLibrary.Repositories.ConfigControl;

namespace AdminStore.Controllers
{
    [ApiControllerJsonConfig]
    [RoutePrefix("passwordreset")]
    public class PasswordResetController : ApiController
    {
        const string defaultLocale = "en-US";
        internal readonly IConfigRepository _configRepo;
        internal readonly IApplicationSettingsRepository _appSettingsRepo;
        internal readonly IHttpClientProvider _httpClientProvider;
        internal readonly IAuthenticationRepository _authenticationRepository;
        internal readonly sl.IServiceLogRepository _log;

        public PasswordResetController() : this(new SqlConfigRepository(), new ApplicationSettingsRepository(),  new HttpClientProvider(), new AuthenticationRepository(), new sl.ServiceLogRepository())
        {

        }

        internal PasswordResetController(IConfigRepository configRepo, IApplicationSettingsRepository settingsRepo, IHttpClientProvider httpClientProvider, IAuthenticationRepository authenticationRepository, sl.IServiceLogRepository log)
        {
            _configRepo = configRepo;
            _appSettingsRepo = settingsRepo;
            _httpClientProvider = httpClientProvider;
            _authenticationRepository = authenticationRepository;
            _log = log;
        }

        /// <summary>
        /// PostRequestPasswordReset
        /// </summary>
        /// <remarks>
        /// Initiates a password reset
        /// </remarks>
        /// <response code="200">OK. See body for result.</response>
        /// <response code="500">Internal Server Error. An error occurred.</response>
        [HttpPost, NoCache]
        [Route("request"), NoSessionRequired]
        [ResponseType(typeof(int))]
        public async Task<IHttpActionResult> PostRequestPasswordReset([FromBody]string login)
        {
            try
            {
                var response = Request.CreateResponse(HttpStatusCode.OK);
                //response.Content = new StringContent(login);
                return ResponseMessage(response);
            }
            catch (Exception ex)
            {
                await _log.LogError(WebApiConfig.LogSourceConfig, ex);
                return InternalServerError();
            }
        }
    }
}
