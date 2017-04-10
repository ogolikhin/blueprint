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
        private const string PasswordResetTokenExpirationInHoursKey = "PasswordResetTokenExpirationInHours";
        private const int DefaultPasswordResetTokenExpirationInHours = 24;

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

                var matchingSetting = await _applicationSettingsRepository.GetValue(IsPasswordRecoveryEnabledKey, false);
                if (!matchingSetting)
                {
                    await _log.LogInformation(WebApiConfig.LogSourceUsersPasswordReset, "Password recovery is disabled");
                    return Conflict();
                }

                var instanceSettings = await _settingsRepository.GetInstanceSettingsAsync();
                if (instanceSettings?.EmailSettingsDeserialized?.HostName == null)
                {
                    await _log.LogInformation(WebApiConfig.LogSourceUsersPasswordReset, "Invalid instance email settings");
                    return Conflict();
                }

                var user = await _userRepository.GetUserByLoginAsync(login);
                if (user == null)
                {
                    await _log.LogInformation(WebApiConfig.LogSourceUsersPasswordReset, "The user doesn't exist");
                    return Conflict();
                }

                bool passwordResetAllowed = await _userRepository.CanUserResetPasswordAsync(login);
                if (!passwordResetAllowed)
                {
                    await _log.LogInformation(WebApiConfig.LogSourceUsersPasswordReset, "The user isn't allowed to reset the password");
                    return Conflict();
                }

                bool passwordRequestLimitExceeded = await _userRepository.HasUserExceededPasswordRequestLimitAsync(login);
                if (passwordRequestLimitExceeded)
                {
                    await _log.LogInformation(WebApiConfig.LogSourceUsersPasswordReset, "Exceeded requests limit");
                    return Conflict();
                }


                bool passwordResetCooldownInEffect = await _authenticationRepository.IsChangePasswordCooldownInEffect(user);
                if (passwordResetCooldownInEffect)
                {
                    await _log.LogInformation(WebApiConfig.LogSourceUsersPasswordReset, "Cooldown is in effect");
                    return Conflict();
                }

                var recoveryToken = SystemEncryptions.CreateCryptographicallySecureGuid();
                var recoveryUrl = new Uri(Request.RequestUri, ServiceConstants.ForgotPasswordResetUrl + "/" + recoveryToken).AbsoluteUri;

                _emailHelper.Initialize(instanceSettings.EmailSettingsDeserialized);

                _emailHelper.SendEmail(user.Email, "Reset Password",
                    $@"
                        <html>
                            <div>Hello {user.DisplayName}.</div>
                            <br>
                            <div>We have received a request to reset your password.</div>
                            <br>
                            <div>To confirm this password reset, visit the following link:</div>
                            <a href='{recoveryUrl}'>Reset password</a>
                            <br><br>
                            <div>If you did not make this request, you can ignore this email, and no changes will be made.</div>
                            <br>
                            <div>If you have any questions, please contact your administrator. </div>
                        </html>");

                await _userRepository.UpdatePasswordRecoveryTokensAsync(login, recoveryToken);
                return Ok();
            }
            catch (Exception ex)
            {
                await _log.LogError(WebApiConfig.LogSourceUsersPasswordReset, ex);
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
        /// <response code="401">Unauthorized. The session token is invalid, missing or malformed.</response>
        /// <response code="403">Forbidden. The user does not have permissions for the project.</response>
        /// <response code="404">Not found. A project for the specified id is not found, does not exist or is deleted.</response>
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
            var tokenLifespan = await _applicationSettingsRepository.GetValue<int>(PasswordResetTokenExpirationInHoursKey, DefaultPasswordResetTokenExpirationInHours);
            if (tokens.First().CreationTime.AddHours(tokenLifespan) < DateTime.Now)
            {
                //token expired
                throw new ConflictException("Password reset failed, recovery token expired.", ErrorCodes.PasswordResetTokenExpired);
            }

            var userLogin = tokens.First().Login;
            var user = await _userRepository.GetUserByLoginAsync(userLogin);
            if (user == null)
            {
                //user does not exist
                throw new ConflictException("Password reset failed, the user does not exist.", ErrorCodes.PasswordResetUserNotFound);
            }
            if (!user.IsEnabled)
            {
                //user is disabled
                throw new ConflictException("Password reset failed, the login for this user is disabled.", ErrorCodes.PasswordResetUserDisabled);
            }

            string decodedNewPassword;
            try
            {
                decodedNewPassword = SystemEncryptions.Decode(content.Password);
            }
            catch (Exception)
            {
                throw new BadRequestException("Password reset failed, the provided password was not encoded correctly", ErrorCodes.PasswordDecodingError);
            }

            if (decodedNewPassword != null && user.Password == HashingUtilities.GenerateSaltedHash(decodedNewPassword, user.UserSalt))
            {
                throw new BadRequestException("Password reset failed, new password cannot be equal to the old one", ErrorCodes.SamePassword);
            }
                
            //reset password
            await _authenticationRepository.ResetPassword(user, null, decodedNewPassword);

            //drop user session
            var uri = new Uri(WebApiConfig.AccessControl);
            var http = _httpClientProvider.Create(uri);
            var request = new HttpRequestMessage { RequestUri = new Uri(uri, $"sessions/{user.Id}"), Method = HttpMethod.Delete };
            await http.SendAsync(request);

            return Ok();
        }

        /// <summary>
        /// PostUser
        /// </summary>
        /// <remarks>
        /// Returns id of the created user.
        /// </remarks>
        /// <response code="201">OK. The user is created.</response>
        /// <response code="400">BadRequest. Some errors. </response>
        /// <response code="401">Unauthorized. The session token is invalid, missing or malformed.</response>
        /// <response code="403">Forbidden. The user does not have permissions for creating the user.</response>
        [HttpPost]
        //[SessionRequired]
        [ResponseType(typeof(HttpResponseMessage))]
        [Route(""), BaseExceptionFilter]
        public async Task<HttpResponseMessage> PostUser([FromBody]User user)
        {
            //var session = Request.Properties[ServiceConstants.SessionProperty] as Session;

            //if (session == null)
            //{
            //    throw new BadRequestException(ErrorMessages.SessionIsEmpty);
            //}

            var hasPermissions = await _userRepository.HasPermissions(2,
                new InstanceAdminPrivileges[] { InstanceAdminPrivileges.ManageUsers, InstanceAdminPrivileges.AssignAdminRoles });

            if (!hasPermissions)
                throw new AuthorizationException(ErrorMessages.UserDoesNotHavePermissions, ErrorCodes.Forbidden);


            if (string.IsNullOrEmpty(user.Login))
            {
                throw new BadRequestException(ErrorMessages.LoginRequered, ErrorCodes.BadRequest);
            }

            if (user.Login.Length < 4 || user.Login.Length > 256)
            {
                throw new BadRequestException(ErrorMessages.LoginFieldLimitation, ErrorCodes.BadRequest);
            }

            var existLogin = await _userRepository.GetUserByLoginAsync(user.Login);
            if (existLogin != null)
            {
                throw new BadRequestException(ErrorMessages.LoginNameUnique);
            }

            if (string.IsNullOrEmpty(user.DisplayName))
            {
                throw new BadRequestException(ErrorMessages.DisplayNameRequered, ErrorCodes.BadRequest);
            }

            if (user.DisplayName.Length < 2 || user.DisplayName.Length > 255)
            {
                throw new BadRequestException(ErrorMessages.DisplayNameFieldLimitation, ErrorCodes.BadRequest);
            }

            if (string.IsNullOrEmpty(user.FirstName))
            {
                throw new BadRequestException(ErrorMessages.FirstNameRequered, ErrorCodes.BadRequest);
            }

            if (user.FirstName.Length < 2 || user.FirstName.Length > 255)
            {
                throw new BadRequestException(ErrorMessages.FirstNameFieldLimitation, ErrorCodes.BadRequest);
            }

            if (string.IsNullOrEmpty(user.LastName))
            {
                throw new BadRequestException(ErrorMessages.LastNameRequered, ErrorCodes.BadRequest);
            }

            if (user.LastName.Length < 2 || user.LastName.Length > 255)
            {
                throw new BadRequestException(ErrorMessages.LastNameFieldLimitation, ErrorCodes.BadRequest);
            }


            if (!string.IsNullOrEmpty(user.Email) && (user.Email.Length < 4 || user.Email.Length > 255))
            {
                throw new BadRequestException(ErrorMessages.EmailFieldLimitation, ErrorCodes.BadRequest);
            }

            if (!string.IsNullOrEmpty(user.Title) && (user.Title.Length < 2 || user.Title.Length > 255))
            {
                throw new BadRequestException(ErrorMessages.TitleFieldLimitation, ErrorCodes.BadRequest);
            }

            if (!string.IsNullOrEmpty(user.Title) && (user.Title.Length < 1 || user.Title.Length > 255))
            {
                throw new BadRequestException(ErrorMessages.DepartmentFieldLimitation, ErrorCodes.BadRequest);
            }

            var newGuid = Guid.NewGuid();
            user.UserSALT = newGuid;

            if (user.AllowFallback == null || (user.AllowFallback != null && (bool)!user.AllowFallback))
            {
                if (string.IsNullOrEmpty(user.NewPassword))
                {
                    throw new BadRequestException(ErrorMessages.PasswordRequered, ErrorCodes.BadRequest);
                }

                var decodedPasword = SystemEncryptions.Decode(user.NewPassword);
                user.NewPassword = HashingUtilities.GenerateSaltedHash(decodedPasword, (Guid)user.UserSALT);
            }
            else
            {
                user.NewPassword = null;
            }

            var userId = await _userRepository.AddUser(user);

            return Request.CreateResponse<int>(HttpStatusCode.Created, userId);

        }
    }
}
