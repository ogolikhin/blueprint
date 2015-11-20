using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Authentication;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using ServiceLibrary.Helpers;
using System.Linq;
using AdminStore.Repositories;

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
                var userId = await RequestPutSessionAsync();
                var loginUser = await _userRepository.GetLoginUserByIdAsync(userId);
                if (loginUser == null)
                {
                    throw new InvalidCredentialException(string.Format("User does not exist with UserId: {0}", userId));
                }
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

        private async Task<int> RequestPutSessionAsync()
        {
            using (var http = _httpClientProvider.Create())
            {
                http.BaseAddress = new Uri(WebApiConfig.AccessControl);
                http.DefaultRequestHeaders.Accept.Clear();
                http.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                http.DefaultRequestHeaders.Add("Session-Token", Request.Headers.GetValues("Session-Token").First());
                var result = await http.PutAsync("sessions/?op=op/?aid=aid", new StringContent(null));
                if (result.IsSuccessStatusCode)
                {
                    var content = await result.Content.ReadAsStringAsync();
                    return int.Parse(content);
                }
                throw new AuthenticationException("Authentication failed.");
            }
        }
    }
}
