using System;
using System.Collections.Generic;
using System.Globalization;
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
    public class UsersController : BaseApiController
    {
        internal readonly IAuthenticationRepository _authenticationRepository;
        internal readonly ISqlUserRepository _userRepository;
        internal readonly ISqlSettingsRepository _settingsRepository;
        internal readonly IEmailHelper _emailHelper;
        internal readonly IApplicationSettingsRepository _applicationSettingsRepository;
        internal readonly IServiceLogRepository _log;
        internal readonly IHttpClientProvider _httpClientProvider;
        internal readonly IPrivilegesRepository _privilegesRepository;
        private const string PasswordResetTokenExpirationInHoursKey = "PasswordResetTokenExpirationInHours";
        private const int DefaultPasswordResetTokenExpirationInHours = 24;

        public UsersController() : this(new AuthenticationRepository(), new SqlUserRepository(),
            new SqlSettingsRepository(), new EmailHelper(), new ApplicationSettingsRepository(),
            new ServiceLogRepository(), new HttpClientProvider(), new SqlPrivilegesRepository())
        {
        }

        internal UsersController(IAuthenticationRepository authenticationRepository,
            ISqlUserRepository userRepository, ISqlSettingsRepository settingsRepository,
            IEmailHelper emailHelper, IApplicationSettingsRepository applicationSettingsRepository,
            IServiceLogRepository log, IHttpClientProvider httpClientProvider, IPrivilegesRepository privilegesRepository)
        {
            _authenticationRepository = authenticationRepository;
            _userRepository = userRepository;
            _settingsRepository = settingsRepository;
            _emailHelper = emailHelper;
            _applicationSettingsRepository = applicationSettingsRepository;
            _log = log;
            _httpClientProvider = httpClientProvider;
            _privilegesRepository = privilegesRepository;
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
                    throw new AuthenticationException(string.Format("User does not exist with Id: {0}", session.UserId));
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
        /// Get users list according to the input parameters 
        /// </summary>
        /// <param name="settings">Table settings parameters. Such as page number, page size, filter and sort parameters</param>
        /// <returns code="200">OK if admin user session exists and user is permitted to list users</returns>
        /// <returns code="400">BadRequest if page, pageSize are missing or invalid</returns>
        /// <returns code="401">Unauthorized if session token is missing, malformed or invalid (session expired)</returns>
        /// <returns code="403">Forbidden if used doesn’t have permissions to get users list</returns>
        [SessionRequired]
        [Route("")]
        [ResponseType(typeof(QueryResult))]
        public async Task<IHttpActionResult> GetAllUsers([FromUri] TableSettings settings)
        {
            if (settings == null || settings.PageSize <= 0 || settings.Page <= 0)
            {
                return BadRequest(ErrorMessages.InvalidPageOrPageNumber);
            }
            var permissions = new List<int> { Convert.ToInt32(InstanceAdminPrivileges.ViewUsers) };
            if (!await _privilegesRepository.IsUserHasPermissions(permissions, SessionUserId))
            {
                throw new HttpResponseException(HttpStatusCode.Forbidden);
            }
            if (settings.Sort != null)
            {
                settings.Sort = UsersHelper.SortUsers(settings.Sort.ToLower(CultureInfo.InvariantCulture));
            }

            var result = _userRepository.GetUsers(settings);

            return Ok(result);
        }

        /// <summary>
        /// Get user by Identifier
        /// </summary>
        /// <param name="userId">User's identity</param>
        /// <returns>
        /// <response code="200">OK. Returns the specified user's icon.</response>
        /// <response code="401">Unauthorized. The session token is invalid, missing or malformed.</response>
        /// <response code="404">Not Found. The user with the provided ID was not found.</response>
        /// <response code="403">User doesn’t have permission to view users.</response>
        /// </returns>
        [SessionRequired]
        [Route("{userId:int:min(1)}")]
        [ResponseType(typeof(UserDto))]
        public async Task<IHttpActionResult> GetUser(int userId)
        {
            var permissions = new List<int> { Convert.ToInt32(InstanceAdminPrivileges.ViewUsers) };
            if (!await _privilegesRepository.IsUserHasPermissions(permissions, SessionUserId))
            {
                throw new HttpResponseException(HttpStatusCode.Forbidden);
            }
            var user = await _userRepository.GetUserDto(userId);

            if (user.Id == 0)
                return NotFound();

            return Ok(user);
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
        /// Create new database user
        /// </summary>
        /// <remarks>
        /// Returns id of the created user.
        /// </remarks>
        /// <response code="201">OK. The user is created.</response>
        /// <response code="400">BadRequest. Some errors. </response>
        /// <response code="401">Unauthorized. The session token is invalid, missing or malformed.</response>
        /// <response code="403">Forbidden. The user does not have permissions for creating the user.</response>
        [HttpPost]
        [SessionRequired]
        [ResponseType(typeof(int))]
        [Route("")]
        public async Task<HttpResponseMessage> PostUser([FromBody] UserDto user)
        {
            if (user == null)
            {
                throw new BadRequestException(ErrorMessages.UserModelIsEmpty, ErrorCodes.BadRequest);
            }

            if (!(await UserPermissionsValidator.HasValidPermissions(SessionUserId, user, _privilegesRepository)))
            {
                throw new AuthorizationException(ErrorMessages.UserDoesNotHavePermissions, ErrorCodes.Forbidden);
            }
            var databaseUser = UsersHelper.CreateDbUserFromDto(user);

            var userId = await _userRepository.AddUserAsync(databaseUser);
            return Request.CreateResponse<int>(HttpStatusCode.Created, userId);
        }

        /// <summary>
        /// Update database user
        /// </summary>
        /// <param name="userId">User's identity</param>
        /// <param name="user">User's model</param>
        /// <remarks>
        /// Returns Ok result.
        /// </remarks>
        /// <response code="200">OK. The database user is updated.</response>
        /// <response code="400">BadRequest. Some errors. </response>
        /// <response code="401">Unauthorized. The session token is invalid, missing or malformed.</response>
        /// <response code="403">Forbidden. The user does not have permissions for updating the user.</response>
        /// <response code="404">NotFound. The user with the current userId doesn’t exist or removed from the system.</response>
        /// <response code="409">Conflict. The current version from the request doesn’t match the current version in DB.</response>
        [HttpPut]
        [SessionRequired]
        [ResponseType(typeof (HttpResponseMessage))]
        [Route("{userId:int}")]
        public async Task<IHttpActionResult> UpdateUser(int userId, [FromBody] UserDto user)
        {
            if (userId == 0)
            {
                throw new BadRequestException(ErrorMessages.IncorrectUserId, ErrorCodes.BadRequest);
            }

            if (user == null)
            {
                throw new BadRequestException(ErrorMessages.UserModelIsEmpty, ErrorCodes.BadRequest);
            }

            if (!(await UserPermissionsValidator.HasValidPermissions(SessionUserId, user, _privilegesRepository)))
            {
                throw new AuthorizationException(ErrorMessages.UserDoesNotHavePermissions, ErrorCodes.Forbidden);
            }

            var databaseUser = UsersHelper.CreateDbUserFromDto(user, userId);
            await _userRepository.UpdateUserAsync(databaseUser);

            return Ok();
        }
    }
}
