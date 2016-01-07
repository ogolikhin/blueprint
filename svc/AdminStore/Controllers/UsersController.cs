using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Authentication;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using AdminStore.Repositories;
using Newtonsoft.Json;
using ServiceLibrary.Attributes;
using ServiceLibrary.Helpers;
using ServiceLibrary.Models;
using ServiceLibrary.Repositories.ConfigControl;

namespace AdminStore.Controllers
{
    [RoutePrefix("users")]
    public class UsersController : ApiController
    {
        internal readonly IHttpClientProvider _httpClientProvider;
        internal readonly ISqlUserRepository _userRepository;
        internal readonly IServiceLogRepository _log;

        public UsersController()
            : this(new SqlUserRepository(), new HttpClientProvider(), new ServiceLogRepository())
        {
        }

        internal UsersController(ISqlUserRepository userRepository, IHttpClientProvider httpClientProvider, IServiceLogRepository log)
        {
            _httpClientProvider = httpClientProvider;
            _userRepository = userRepository;
            _log = log;
        }

        [HttpGet, NoCache]
        [Route("loginuser")]
        [ResponseType(typeof(HttpResponseMessage))]
        public async Task<IHttpActionResult> GetLoginUser()
        {
            try
            {
                var session = await RequestPutSessionAsync();
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
            catch (ArgumentNullException)
            {
                return BadRequest();
            }
            catch (Exception ex)
            {
                await _log.LogError(WebApiConfig.LogSource_Users, ex);
                return InternalServerError();
            }
        }

        private async Task<Session> RequestPutSessionAsync()
        {
            using (var http = _httpClientProvider.Create())
            {
                http.BaseAddress = new Uri(WebApiConfig.AccessControl);
                http.DefaultRequestHeaders.Accept.Clear();
                http.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                if (!Request.Headers.Contains("Session-Token"))
                {
                    throw new ArgumentNullException();
                }
                http.DefaultRequestHeaders.Add("Session-Token", Request.Headers.GetValues("Session-Token").First());
                var result = await http.PutAsync("sessions", new StringContent(""));
                if (result.IsSuccessStatusCode)
                {
                    var content = await result.Content.ReadAsStringAsync();
                    var session = JsonConvert.DeserializeObject<Session>(content);
                    return session;
                }
                throw new AuthenticationException("Authentication failed.");
            }
        }
    }
}
