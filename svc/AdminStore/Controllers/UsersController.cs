using System;
using System.Net.Http;
using System.Security.Authentication;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
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

        [HttpGet, NoCache]
        [Route("loginuser"), SessionRequired]
        [ResponseType(typeof(HttpResponseMessage))]
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
                await _log.LogError(WebApiConfig.LogSource_Users, ex);
                return InternalServerError();
            }
        }
    }
}
