using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using AdminStore.Helpers;
using AdminStore.Repositories;
using ServiceLibrary.Attributes;
using sl = ServiceLibrary.Repositories.ConfigControl;

namespace AdminStore.Controllers
{
    [ApiControllerJsonConfig]
    [RoutePrefix("passwordreset")]
    public class PasswordResetController : ApiController
    {
        private readonly ISqlUserRepository _sqlUserRepository;
        private readonly sl.IServiceLogRepository _log;
        private readonly ISqlSettingsRepository _settingsRepository;

        public PasswordResetController() : this(new SqlUserRepository(), new sl.ServiceLogRepository(), new SqlSettingsRepository())
        {

        }

        internal PasswordResetController(ISqlUserRepository sqlUserRepository, sl.IServiceLogRepository log, ISqlSettingsRepository settingsRepository)
        {
            _settingsRepository = settingsRepository;
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
            var instanceSettings = await _settingsRepository.GetInstanceSettingsAsync();
            
            bool passwordResetAllowed = await _sqlUserRepository.CanUserResetPassword(login);
            bool passwordRequestLimitExceeded = await _sqlUserRepository.HasUserExceededPasswordRequestLimit(login);

            var user = await _sqlUserRepository.GetUserByLoginAsync(login);

            try
            {
                var response = Request.CreateResponse(HttpStatusCode.OK);
                if (passwordResetAllowed && !passwordRequestLimitExceeded)
                {
                    if (instanceSettings?.EmailSettingsDeserialized?.HostName == null || user == null)
                    {
                        response.Content = new StringContent("no");
                        return ResponseMessage(response);
                    }

                    EmailHelper emailHelper = new EmailHelper(instanceSettings.EmailSettingsDeserialized);

                    emailHelper.SendEmail(user.Email);

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
