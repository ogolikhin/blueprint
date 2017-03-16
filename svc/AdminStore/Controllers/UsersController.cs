using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using AdminStore.Helpers;
using AdminStore.Models;
using AdminStore.Repositories;
using ServiceLibrary.Attributes;
using ServiceLibrary.Exceptions;
using ServiceLibrary.Helpers;
using ServiceLibrary.Models;
using ServiceLibrary.Repositories.ConfigControl;

namespace AdminStore.Controllers
{
    [ApiControllerJsonConfig]
    [RoutePrefix("users")]
    [BaseExceptionFilter]
    public class UsersController : ApiController
    {
        internal readonly IAuthenticationRepository _authenticationRepository;
        internal readonly ISqlUserRepository _userRepository;
        internal readonly ISqlSettingsRepository _settingsRepository;
        internal readonly IEmailHelper _emailHelper;
        internal readonly IApplicationSettingsRepository _applicationSettingsRepository;
        internal readonly IServiceLogRepository _log;
        internal readonly IHttpClientProvider _httpClientProvider;

        public UsersController() : this(new AuthenticationRepository(), new SqlUserRepository(), 
            new SqlSettingsRepository(), new EmailHelper(), new ApplicationSettingsRepository(), 
            new ServiceLogRepository(), new HttpClientProvider())
        {
        }

        internal UsersController(IAuthenticationRepository authenticationRepository, 
            ISqlUserRepository userRepository, ISqlSettingsRepository settingsRepository, 
            IEmailHelper emailHelper, IApplicationSettingsRepository applicationSettingsRepository, 
            IServiceLogRepository log, IHttpClientProvider httpClientProvider)
        {
            _authenticationRepository = authenticationRepository;
            _userRepository = userRepository;
            _settingsRepository = settingsRepository;
            _emailHelper = emailHelper;
            _applicationSettingsRepository = applicationSettingsRepository;
            _log = log;
            _httpClientProvider = httpClientProvider;
        }

        /// <summary>
        /// GetLoginUser
        /// </summary>
        /// <remarks>
        /// Returns information about the user of the current session.
        /// </remarks>
        /// <response code="200">OK.</response>
        /// <response code="401">Unauthorized. The session token is invalid, missing or malformed.</response>
        /// <response code="500">Internal Server Error. An error occurred.</response>
        [HttpGet, NoCache]
        [Route("loginuser"), SessionRequired]
        [ResponseType(typeof(LoginUser))]
        public async Task<IHttpActionResult> GetLoginUser()
        {
            try
            {
                var session = Request.Properties[ServiceConstants.SessionProperty] as Session;
                var loginUser = await _userRepository.GetLoginUserByIdAsync(session.UserId);
                if (loginUser == null)
                {
                    throw new AuthenticationException(string.Format("User does not exist with UserId: {0}", session.UserId));
                }
                loginUser.LicenseType = session.LicenseLevel;
                loginUser.IsSso = session.IsSso;
                return Ok(loginUser);
            }
            catch (AuthenticationException)
            {
                return Unauthorized();
            }
            catch (Exception ex)
            {
                await _log.LogError(WebApiConfig.LogSourceUsers, ex);
                return InternalServerError();
            }
        }

        /// <summary>
        /// GetUserIcon
        /// </summary>
        /// <remarks>
        /// Returns information about the user of the current session.
        /// </remarks>
        /// <response code="200">OK. Returns the specified user's icon.</response>
        /// <response code="204">No Content. No icon for user exists.</response>
        /// <response code="401">Unauthorized. The session token is invalid, missing or malformed.</response>
        /// <response code="404">Not Found. The user with the provided ID was not found.</response>
        /// <response code="500">Internal Server Error. An error occurred.</response>
        [HttpGet, NoCache]
        [Route("{userId:int:min(1)}/icon"), SessionRequired(true)]
        [ResponseType(typeof(byte[]))]
        public async Task<HttpResponseMessage> GetUserIcon(int userId)
        {
            try
            {
                var imageContent = await _userRepository.GetUserIconByUserIdAsync(userId);
                if (imageContent == null)
                {
                    throw new ResourceNotFoundException($"User does not exist with UserId: {userId}", ErrorCodes.ResourceNotFound);
                }
                if (imageContent.Content == null)
                {
                    return Request.CreateResponse(HttpStatusCode.NoContent);
                }

                var httpResponseMessage = Request.CreateResponse(HttpStatusCode.OK);
                httpResponseMessage.Content = ImageHelper.CreateByteArrayContent(imageContent.Content);
                return httpResponseMessage;
            }
            catch (Exception ex)
            {
                await _log.LogError(WebApiConfig.LogSourceUsers, ex);
                throw;
            }
        }

        /// <summary>
        /// PostResetPassword
        /// </summary>
        /// <remarks>
        /// Reset password to the new provided one.
        /// </remarks>
        /// <response code="200">OK.</response>
        /// <response code="400">Bad request. The new password is not valid.</response>
        /// <response code="401">Unauthorized. The old password is not valid.</response>
        /// <response code="500">Internal Server Error. An error occurred.</response>
        [HttpPost]
        [Route("reset"), NoSessionRequired]
        [ResponseType(typeof(HttpResponseMessage))]
        [BaseExceptionFilter]
        public async Task<IHttpActionResult> PostReset(string login, [FromBody]ResetPostContent body)
        {
            var decodedLogin = SystemEncryptions.Decode(login);
            var decodedOldPassword = SystemEncryptions.Decode(body.OldPass);
            var decodedNewPassword = SystemEncryptions.Decode(body.NewPass);
            var user = await _authenticationRepository.AuthenticateUserForResetAsync(decodedLogin, decodedOldPassword);
            await _authenticationRepository.ResetPassword(user, decodedOldPassword, decodedNewPassword);
            return Ok();
        }

        /// <summary>
        /// PostRequestPasswordReset
        /// </summary>
        /// <remarks>
        /// Initiates a request for a password reset
        /// </remarks>
        /// <response code="200">OK.</response>
        /// <response code="409">Generic error when recovery not allowed.</response>
        /// <response code="500">Internal Server Error. An error occurred.</response>
        [HttpPost]
        [Route("passwordrecovery/request"), NoSessionRequired]
        [ResponseType(typeof(int))]
        public async Task<IHttpActionResult> PostRequestPasswordResetAsync([FromBody]string login)
        {
            try
			{
            	const string IsPasswordRecoveryEnabledKey = "IsPasswordRecoveryEnabled";
	            var applicationSettings = await _applicationSettingsRepository.GetSettings();
    	        var matchingSetting = applicationSettings.FirstOrDefault(s => s.Key == IsPasswordRecoveryEnabledKey);
        	    if (matchingSetting == null || matchingSetting.Value != "true")
	            {
	                return ResponseMessage(Request.CreateResponse(HttpStatusCode.Conflict));
	            }
	
	            var instanceSettings = await _settingsRepository.GetInstanceSettingsAsync();

                bool passwordResetAllowed = await _userRepository.CanUserResetPasswordAsync(login);
                bool passwordRequestLimitExceeded = await _userRepository.HasUserExceededPasswordRequestLimitAsync(login);

                var user = await _userRepository.GetUserByLoginAsync(login);
                bool passwordResetCooldownInEffect = await _authenticationRepository.IsChangePasswordCooldownInEffect(user);

                if (passwordResetAllowed && !passwordRequestLimitExceeded && !passwordResetCooldownInEffect && instanceSettings?.EmailSettingsDeserialized?.HostName != null)
                {
                    var recoveryToken = SystemEncryptions.CreateCryptographicallySecureGuid();

                    _emailHelper.Initialize(instanceSettings.EmailSettingsDeserialized);
                    _emailHelper.SendEmail(user);

                    await _userRepository.UpdatePasswordRecoveryTokensAsync(login, recoveryToken);
                    return Ok();
                }

                return ResponseMessage(Request.CreateResponse(HttpStatusCode.Conflict));
            }
            catch (Exception ex)
            {
                await _log.LogError(WebApiConfig.LogSourceConfig, ex);
                return InternalServerError();
            }
        }

        /// <summary>
        /// PostPasswordReset
        /// </summary>
        /// <remarks>
        /// Initiates a password reset
        /// </remarks>
        /// <response code="200">OK.</response>
        /// <response code="400">Error. Password provided is invalid..</response>
        /// <response code="409">Error. Token is invalid.</response>
        /// <response code="500">Internal Server Error. An error occurred.</response>
        [HttpPost]
        [Route("passwordrecovery/reset"), NoSessionRequired]
        [ResponseType(typeof(int))]
        [BaseExceptionFilter]
        public async Task<IHttpActionResult> PostPasswordResetAsync([FromBody]ResetPasswordContent content)
        {
            //the deserializer creates a zero filled guid when none provided
            if (content.Token == null || content.Token.GetHashCode() == 0)
            {
                throw new BadRequestException("Password reset failed, token not provided", ErrorCodes.PasswordResetEmptyToken);
            }

            var tokens = (await _userRepository.GetPasswordRecoveryTokensAsync(content.Token)).ToList();
            if (!tokens.Any())
            {
                //user did not request password reset
                throw new ConflictException("Password reset failed, recovery token not found.", ErrorCodes.PasswordResetTokenNotFound);
            }
            if (tokens.First().RecoveryToken != content.Token)
            {
                //provided token doesn't match last requested
                throw new ConflictException("Password reset failed, a more recent recovery token exists.", ErrorCodes.PasswordResetTokenNotLatest);
            }
            if (tokens.First().CreationTime.AddHours(24) < DateTime.Now)
            {
                //token expired
                throw new ConflictException("Password reset failed, recovery token expired.", ErrorCodes.PasswordResetTokenExpired);
            }

            var userLogin = tokens.First().Login;
            var user = await _userRepository.GetUserByLoginAsync(userLogin);
            if (user == null)
            {
                //user does not exist
                throw new ConflictException("Password reset failed, the token is invalid.", ErrorCodes.PasswordResetTokenInvalid);
            }
            if (!user.IsEnabled)
            {
                //user is disabled
                throw new ConflictException("Password reset failed, the login for this user is disabled.", ErrorCodes.PasswordResetUserDisabled);
            }

            var decodedNewPassword = SystemEncryptions.Decode(content.Password);
                
            //reset password
            await _authenticationRepository.ResetPassword(user, null, decodedNewPassword);

            //drop user session
            var uri = new Uri(WebApiConfig.AccessControl);
            var http = _httpClientProvider.Create(uri);
            var request = new HttpRequestMessage { RequestUri = new Uri(uri, $"sessions/{user.Id}"), Method = HttpMethod.Delete };
            await http.SendAsync(request);

            return Ok();
        }
    }
}
