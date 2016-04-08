using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using AdminStore.Helpers;
using AdminStore.Models;
using AdminStore.Repositories;
using Newtonsoft.Json;
using ServiceLibrary.Attributes;
using ServiceLibrary.Models;
using ServiceLibrary.Repositories.ConfigControl;

namespace AdminStore.Controllers
{
    public class ResetPostContent
    {
        public string NewPass;
        public string OldPass;
    }

    [RoutePrefix("users")]
    public class UsersController : ApiController
    {
        internal readonly IAuthenticationRepository _authenticationRepository;
        internal readonly ISqlUserRepository _userRepository;
        internal readonly IServiceLogRepository _log;

        public UsersController() : this(new AuthenticationRepository(), new SqlUserRepository(), new ServiceLogRepository())
        {
        }

        internal UsersController(IAuthenticationRepository authenticationRepository, ISqlUserRepository userRepository, IServiceLogRepository log)
        {
            _authenticationRepository = authenticationRepository;
            _userRepository = userRepository;
            _log = log;
        }

        /// <summary>
        /// GetLoginUser
        /// </summary>
        /// <remarks>
        /// Returns information about the user of the current session.
        /// </remarks>
        /// <response code="200">OK.</response>
        /// <response code="400">Bad Request. The session token is missing or malformed.</response>
        /// <response code="401">Unauthorized. The session token is invalid.</response>
        /// <response code="500">Internal Server Error. An error occurred.</response>
        [HttpGet, NoCache]
        [Route("loginuser"), SessionRequired]
        [ResponseType(typeof(LoginUser))]
        public async Task<IHttpActionResult> GetLoginUser()
        {
            try
            {
                var session = Request.Properties["Session"] as Session;
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
        /// PostResetPassword
        /// </summary>
        /// <remarks>
        /// Reset password to the new provided one.
        /// </remarks>
        /// <response code="200">OK.</response>
        /// <response code="400">Bad request. The new password is not valid.</response>
        /// <response code="401">Unauthorized. The old password is not valid.</response>
        /// <response code="500">Internal Server Error. An error occurred.</response>
        [HttpPost, NoCache]
        [Route("reset")]
        [ResponseType(typeof(HttpResponseMessage))]
        public async Task<IHttpActionResult> PostReset(string login, [FromBody]ResetPostContent body)
        {
            try
            {
                //var body = JsonConvert.DeserializeObject<ResetPostContent>(content);
                var decodedLogin = SystemEncryptions.Decode(login);
                var decodedOldPassword = SystemEncryptions.Decode(body.OldPass);
                var decodedNewPassword = SystemEncryptions.Decode(body.NewPass);

                if (decodedNewPassword == decodedOldPassword)
                {
                    return BadRequest();
                }

                var user = await _authenticationRepository.AuthenticateUserForResetAsync(decodedLogin, decodedOldPassword);

                string errorMsg;
                if (!PasswordValidationHelper.ValidatePassword(decodedNewPassword, true, out errorMsg))
                {
                    return BadRequest();
                }

                await _authenticationRepository.ResetPassword(user, decodedNewPassword);
                
                return Ok();
            }
            catch (AuthenticationException ex)
            {
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.Unauthorized, ex.CreateHttpError()));
            }
            catch (Exception ex)
            {
                await _log.LogError(WebApiConfig.LogSourceUsers, ex);
                return InternalServerError();
            }
        }
    }
}
