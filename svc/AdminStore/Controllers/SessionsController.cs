using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.Remoting;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Http.Description;
using AdminStore.Helpers;
using AdminStore.Models;
using AdminStore.Repositories;
using AdminStore.Saml;
using ServiceLibrary.Helpers;
using ServiceLibrary.Repositories.ConfigControl;

namespace AdminStore.Controllers
{
    [RoutePrefix("sessions")]
    public class SessionsController : ApiController
    {
        internal readonly IAuthenticationRepository _authenticationRepository;
        internal readonly IHttpClientProvider _httpClientProvider;
        internal readonly IServiceLogRepository _log;

        public SessionsController(): this(new AuthenticationRepository(), new HttpClientProvider(), new ServiceLogRepository())
        {
        }

        internal SessionsController(IAuthenticationRepository authenticationRepository, IHttpClientProvider httpClientProvider, IServiceLogRepository log)
        {
            _authenticationRepository = authenticationRepository;
            _httpClientProvider = httpClientProvider;
            _log = log;
        }

        [HttpPost]
        [Route("")]
        [ResponseType(typeof(HttpResponseMessage))]
        public async Task<IHttpActionResult> PostSession(string login, [FromBody]string password, bool force = false)
        {
            try
            {
                var decodedLogin = SystemEncryptions.Decode(login);
                var decodedPassword = SystemEncryptions.Decode(password);
                var user = await _authenticationRepository.AuthenticateUserAsync(decodedLogin, decodedPassword);
                return await RequestSessionTokenAsync(user, force);
            }
            catch (AuthenticationException ex)
            {
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.Unauthorized, ex.CreateHttpError()));
            }
            catch (ApplicationException)
            {              
                return Conflict();
            }
            catch (ArgumentNullException)
            {
                return BadRequest();
            }
            catch (FormatException ex)
            {
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.Unauthorized, ex.Message));
            }
            catch (Exception ex)
            {
                await _log.LogError(WebApiConfig.LogSource_Sessions, ex);
                return InternalServerError();
            }
        }

        private async Task<IHttpActionResult> RequestSessionTokenAsync(AuthenticationUser user, bool force = false, bool isSso = false)
        {
            try
            {
                if (!force)
                {
                    using (var http = _httpClientProvider.Create())
                    {
                        http.BaseAddress = new Uri(WebApiConfig.AccessControl);
                        http.DefaultRequestHeaders.Accept.Clear();
                        http.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                        var result = await http.GetAsync("sessions/" + user.Id);
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

                    var queryParams = HttpUtility.ParseQueryString(string.Empty);
                    queryParams.Add("userName", user.Login);
                    queryParams.Add("licenseLevel", user.LicenseType.ToString());
                    queryParams.Add("isSso", isSso.ToString());

                    var result = await http.PostAsJsonAsync("sessions/" + user.Id + "?" + queryParams, user.Id);
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
            catch (ApplicationException)
            {
                throw;
            }
            catch (Exception ex)
            {
                await _log.LogError(WebApiConfig.LogSource_Sessions, ex);
                return InternalServerError();
            }
        }

        [HttpPost]
        [Route("sso")]
        [ResponseType(typeof(HttpResponseMessage))]
        public async Task<IHttpActionResult> PostSessionSingleSignOn([FromBody]string samlResponse, bool force = false)
        {
            try
            {
                var user = await _authenticationRepository.AuthenticateSamlUserAsync(samlResponse);
                return await RequestSessionTokenAsync(user, force, true);
            }
            catch (FederatedAuthenticationException e)
            {
                if (e.ErrorCode == FederatedAuthenticationErrorCode.WrongFormat)
                {
                    return BadRequest();
                }
                return Unauthorized();
            }
            catch (ApplicationException)
            {
                return Conflict();
            }
            catch (FormatException)
            {
                return BadRequest();
            }
            catch (Exception ex)
            {
                await _log.LogError(WebApiConfig.LogSource_Sessions, ex);
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
                    if (!Request.Headers.Contains("Session-Token"))
                    {
                        throw new ArgumentNullException();
                    }
                    http.DefaultRequestHeaders.Add("Session-Token", Request.Headers.GetValues("Session-Token").First());
                    var result = await http.DeleteAsync("sessions");
                    if (result.IsSuccessStatusCode)
                    {
                        return Ok();
                    }
                    return ResponseMessage(result);
                }
            }
            catch (ArgumentNullException)
            {
                return BadRequest();
            }
            catch (Exception ex)
            {
                await _log.LogError(WebApiConfig.LogSource_Sessions, ex);
                return InternalServerError();
            }
        }
    }
}
