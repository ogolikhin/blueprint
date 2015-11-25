using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Authentication;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using ServiceLibrary.Helpers;
using System.Linq;
using AccessControl.Models;
using AdminStore.Repositories;
using Newtonsoft.Json;

namespace AdminStore.Controllers
{
    [RoutePrefix("users")]
    public class UsersController : ApiController
    {
        private readonly IHttpClientProvider _httpClientProvider;

        private readonly ISqlUserRepository _userRepository;

        public UsersController()
            : this(new SqlUserRepository(), new HttpClientProvider())
        {
        }

        internal UsersController(ISqlUserRepository userRepository, IHttpClientProvider httpClientProvider)
        {
            _httpClientProvider = httpClientProvider;
            _userRepository = userRepository;
        }

        [HttpGet]
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
            catch (ApplicationException)
            {
                return Conflict();
            }
            catch (ArgumentNullException)
            {
                return BadRequest();
            }
            catch (FormatException)
            {
                return BadRequest();
            }
            catch
            {
                return InternalServerError();
            }
        }

        private async Task<Session> RequestPutSessionAsync(string op = "op", int aid = 1)
        {
            using (var http = _httpClientProvider.Create())
            {
                http.BaseAddress = new Uri(WebApiConfig.AccessControl);
                http.DefaultRequestHeaders.Accept.Clear();
                http.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                http.DefaultRequestHeaders.Add("Session-Token", Request.Headers.GetValues("Session-Token").First());
                var result = await http.PutAsync(string.Format("sessions/{0}/{1}", op, aid), new StringContent(""));
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
