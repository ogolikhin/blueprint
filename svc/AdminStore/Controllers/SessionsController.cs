using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.Remoting;
using System.Security.Authentication;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using AdminStore.Repositories;
using AdminStore.Saml;
using ServiceLibrary.Helpers;

namespace AdminStore.Controllers
{
    [RoutePrefix("sessions")]
    public class SessionsController : ApiController
    {
        internal readonly IAuthenticationRepository _authenticationRepository;
        internal readonly IHttpClientProvider _httpClientProvider;

        public SessionsController(): this(new AuthenticationRepository(), new HttpClientProvider())
        {
        }

        internal SessionsController(IAuthenticationRepository authenticationRepository, IHttpClientProvider httpClientProvider)
        {
            _authenticationRepository = authenticationRepository;
            _httpClientProvider = httpClientProvider;
        }

        [HttpPost]
        [Route("")]
        [ResponseType(typeof(HttpResponseMessage))]
        public async Task<IHttpActionResult> PostSession(string login, string password, bool force = false)
        {
            try
            {
                var user = await _authenticationRepository.AuthenticateUserAsync(login, password);
                return await RequestSessionTokenAsync(user.Id);
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

        private async Task<IHttpActionResult> RequestSessionTokenAsync(int userId, bool force = false)
        {
            if (!force)
            {
                using (var http = _httpClientProvider.Create())
                {
                    http.BaseAddress = new Uri(WebApiConfig.AccessControl);
                    http.DefaultRequestHeaders.Accept.Clear();
                    http.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    var result = await http.GetAsync("sessions/" + userId.ToString());
                    if (result.IsSuccessStatusCode) // session exists
                    {
                        throw new ApplicationException("Conflict");
                    }
                }
            }
            using (var http = _httpClientProvider.Create())
            {
                http.BaseAddress = new Uri(WebApiConfig.AccessControl);
                http.DefaultRequestHeaders.Accept.Clear();
                http.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                var result = await http.PostAsJsonAsync("sessions/" + userId.ToString(), userId);
                if (!result.IsSuccessStatusCode)
                {
                    throw new ServerException();
                }
                var token = result.Headers.GetValues("Session-Token").FirstOrDefault();
                var response = new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(token),
                    StatusCode = HttpStatusCode.OK
                };
                response.Headers.Add("Session-Token", token);
                return ResponseMessage(response);
            }
        }

        [HttpPost]
        [Route("sso")]
        [ResponseType(typeof(HttpResponseMessage))]
        public async Task<IHttpActionResult> PostSessionSingleSignOn(string samlResponse, bool force = false)
        {
            try
            {
                var user = await _authenticationRepository.AuthenticateSamlUserAsync(samlResponse);
                return await RequestSessionTokenAsync(user.Id, force);
            }
            catch (FederatedAuthenticationException e)
            {
                if (e.ErrorCode == FederatedAuthenticationErrorCode.WrongFormat)
                {
                    return BadRequest();
                }
                return Unauthorized();
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

        [HttpDelete]
        [Route("")]
        [ResponseType(typeof(HttpResponseMessage))]
        public async Task<IHttpActionResult> DeleteSession()
        {
            try
            {
                using (var http = _httpClientProvider.Create())
                {
                    http.BaseAddress = new Uri(WebApiConfig.AccessControl);
                    http.DefaultRequestHeaders.Accept.Clear();
                    http.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    http.DefaultRequestHeaders.Add("Session-Token", Request.Headers.GetValues("Session-Token").First());
                    var result = await http.DeleteAsync("sessions");
                    result.EnsureSuccessStatusCode();
                    return Ok();
                }
            }
            catch
            {
                return InternalServerError();
            }
        }
    }
}
