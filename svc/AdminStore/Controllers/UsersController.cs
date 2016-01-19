using System;
using System.Security.Authentication;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using AdminStore.Models;
using AdminStore.Repositories;
using ServiceLibrary.Attributes;
using ServiceLibrary.Models;
using ServiceLibrary.Repositories.ConfigControl;

namespace AdminStore.Controllers
{
    [RoutePrefix("users")]
    public class UsersController : ApiController
    {
        internal readonly ISqlUserRepository _userRepository;
        internal readonly IServiceLogRepository _log;

        public UsersController() : this(new SqlUserRepository(), new ServiceLogRepository())
        {
        }

        internal UsersController(ISqlUserRepository userRepository, IServiceLogRepository log)
        {
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
    }
}
