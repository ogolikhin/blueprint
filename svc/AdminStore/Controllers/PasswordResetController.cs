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
        internal readonly ISqlUserRepository _sqlUserRepository;
        internal readonly sl.IServiceLogRepository _log;

        public PasswordResetController() : this(new SqlConfigRepository(), new ApplicationSettingsRepository(),  new HttpClientProvider(), new SqlUserRepository(), new sl.ServiceLogRepository())
        {

        }

        internal PasswordResetController(IConfigRepository configRepo, IApplicationSettingsRepository settingsRepo, IHttpClientProvider httpClientProvider, ISqlUserRepository sqlUserRepository, sl.IServiceLogRepository log)
        {
            _configRepo = configRepo;
            _appSettingsRepo = settingsRepo;
            _httpClientProvider = httpClientProvider;
            _sqlUserRepository = sqlUserRepository;
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
            bool passwordResetAllowed = await _sqlUserRepository.CanUserResetPassword(login);

            try
            {
                var response = Request.CreateResponse(HttpStatusCode.OK);

                if (passwordResetAllowed)
                {
                    await _sqlUserRepository.UpdatePasswordRecoveryTokens(login);

                    response.Content = new StringContent("ok");
                } else {
                    response.Content = new StringContent("no");
                }

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
