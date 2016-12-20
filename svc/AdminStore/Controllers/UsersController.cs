using System;
using System.Collections;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using System.Web.UI.WebControls;
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
        /// <response code="200">OK.</response>
        /// <response code="204">No icon for user exists.</response>
        /// <response code="401">Not Found. The user with the provided ID was not found.</response>
        /// <response code="403">Forbidden. The provided user's icon is not accessible.</response>
        /// <response code="500">Internal Server Error. An error occurred.</response>
        [HttpGet, NoCache]
        [Route("{userId:int:min(1)}/icon"), SessionRequired]
        [ResponseType(typeof(BitArray))]
        public async Task<IHttpActionResult> GetUserIcon(int userId)
        {
            try
            {
                var session = Request.Properties[ServiceConstants.SessionProperty] as Session;
                var loginUser = await _userRepository.GetLoginUserByIdAsync(session.UserId);
                if (loginUser == null)
                {
                    throw new AuthenticationException(string.Format("User does not exist with UserId: {0}", session.UserId));
                }
                if (loginUser.Image_ImageId == null)
                {
                    return StatusCode(HttpStatusCode.NoContent);
                }

                return Ok(loginUser.Image_ImageId);
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
    }
}
