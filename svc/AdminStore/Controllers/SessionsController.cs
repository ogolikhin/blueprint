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
using ServiceLibrary.Attributes;
using ServiceLibrary.Helpers;
using ServiceLibrary.Models;
using ServiceLibrary.Repositories.ConfigControl;

namespace AdminStore.Controllers
{
    /// <summary>
    /// SessionsController
    /// </summary>
    [RoutePrefix("sessions")]
    public class SessionsController : ApiController
    {
        internal readonly IAuthenticationRepository _authenticationRepository;
        internal readonly IHttpClientProvider _httpClientProvider;
        internal readonly IServiceLogRepository _log;

        public SessionsController() : this(new AuthenticationRepository(), new HttpClientProvider(), new ServiceLogRepository())
        {
        }

        internal SessionsController(IAuthenticationRepository authenticationRepository, IHttpClientProvider httpClientProvider, IServiceLogRepository log)
        {
            _authenticationRepository = authenticationRepository;
            _httpClientProvider = httpClientProvider;
            _log = log;
        }

        /// <summary>
        /// PostSession
        /// </summary>
        /// <remarks>
        /// Authenticates a Database or Windows user with the given <paramref name="login" /> and <paramref name="password" />
        /// and returns a new session. If a session already exists for the user, returns an error unless the <paramref name="force"/>
        /// parameter is true.
        /// </remarks>
        /// <param name="login">The encrypted user login.</param>
        /// <param name="password">The encrypted password.</param>
        /// <param name="force">True to override the existing session, if any.</param>
        /// <response code="200">OK.</response>
        /// <response code="400">Bad Request. The session token is missing or malformed.</response>
        /// <response code="401">Unauthorized. The session token is invalid.</response>
        /// <response code="409">Conflict. A session already exists for this user and <paramref name="force" /> is false.</response>
        /// <response code="500">Internal Server Error. An error occurred.</response>
        [HttpPost]
        [Route(""), NoSessionRequired]
        [ResponseType(typeof(Session))]
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
                await _log.LogInformation(WebApiConfig.LogSourceSessions, ex.Message);
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.Unauthorized, ex.CreateHttpError()));
            }
            catch (ApplicationException ex)
            {
                await _log.LogInformation(WebApiConfig.LogSourceSessions, ex.Message);
                return Conflict();
            }
            catch (ArgumentNullException ex)
            {
                await _log.LogInformation(WebApiConfig.LogSourceSessions, ex.Message);
                return BadRequest();
            }
            catch (FormatException ex)
            {
                await _log.LogInformation(WebApiConfig.LogSourceSessions, ex.Message);
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.Unauthorized, ex.Message));
            }
            catch (Exception ex)
            {
                await _log.LogError(WebApiConfig.LogSourceSessions, ex);
                return InternalServerError();
            }
        }

        private async Task<IHttpActionResult> RequestSessionTokenAsync(AuthenticationUser user, bool force = false, bool isSso = false)
        {
            try
            {
                if (!force)
                {
                    using (var http = _httpClientProvider.Create(new Uri(WebApiConfig.AccessControl)))
                    {
                        var result = await http.GetAsync("sessions/" + user.Id);
                        if (result.IsSuccessStatusCode) // session exists
                        {
                            throw new ApplicationException("Conflict");
                        }
                    }
                }
                using (var http = _httpClientProvider.Create(new Uri(WebApiConfig.AccessControl)))
                {
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
                await _log.LogError(WebApiConfig.LogSourceSessions, ex);
                return InternalServerError();
            }
        }

        /// <summary>
        /// PostSessionSingleSignOn
        /// </summary>
        /// <remarks>
        /// Processes the SAML response from an Identity Provider and returns a new session.
        /// If a session already exists for the user, returns an error unless the <paramref name="force"/>
        /// parameter is true.
        /// </remarks>
        /// <param name="samlResponse">The SAML response from the Identity Provider.</param>
        /// <param name="force">True to override the existing session, if any.</param>
        /// <response code="200">OK.</response>
        /// <response code="400">Bad Request. The session token is missing or malformed.</response>
        /// <response code="401">Unauthorized. The session token is invalid.</response>
        /// <response code="409">Conflict. A session already exists for this user and <paramref name="force" /> is false.</response>
        /// <response code="500">Internal Server Error. An error occurred.</response>
        [HttpPost]
        [Route("sso"), NoSessionRequired]
        [ResponseType(typeof(Session))]
        public async Task<IHttpActionResult> PostSessionSingleSignOn([FromBody]string samlResponse, bool force = false)
        {
            try
            {
                var user = await _authenticationRepository.AuthenticateSamlUserAsync(samlResponse);
                return await RequestSessionTokenAsync(user, force, true);
            }
            catch (FederatedAuthenticationException ex)
            {
                await _log.LogInformation(WebApiConfig.LogSourceSessions, ex.Message);
                if (ex.ErrorCode == FederatedAuthenticationErrorCode.WrongFormat)
                {
                    return BadRequest();
                }
                return Unauthorized();
            }
            catch (AuthenticationException ex)
            {
                await _log.LogInformation(WebApiConfig.LogSourceSessions, ex.Message);
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.Unauthorized, ex.CreateHttpError()));
            }
            catch (ApplicationException ex)
            {
                await _log.LogInformation(WebApiConfig.LogSourceSessions, ex.Message);
                return Conflict();
            }
            catch (FormatException ex)
            {
                await _log.LogInformation(WebApiConfig.LogSourceSessions, ex.Message);
                return BadRequest();
            }
            catch (Exception ex)
            {
                await _log.LogError(WebApiConfig.LogSourceSessions, ex);
                return InternalServerError();
            }
        }

        /// <summary>
        /// DeleteSession
        /// </summary>
        /// <remarks>
        /// Terminates the current session, releasing the associated license.
        /// </remarks>
        /// <response code="200">OK.</response>
        /// <response code="400">Bad Request. The session token is missing or malformed.</response>
        /// <response code="401">Unauthorized. The session token is invalid.</response>
        /// <response code="500">Internal Server Error. An error occurred.</response>
        [HttpDelete]
        [Route(""), SessionRequired]
        [ResponseType(typeof(void))]
        public async Task<IHttpActionResult> DeleteSession()
        {
            try
            {
                using (var http = _httpClientProvider.Create(new Uri(WebApiConfig.AccessControl)))
                {
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
            catch (ArgumentNullException ex)
            {
                await _log.LogInformation(WebApiConfig.LogSourceSessions, ex.Message);
                return BadRequest();
            }
            catch (Exception ex)
            {
                await _log.LogError(WebApiConfig.LogSourceSessions, ex);
                return InternalServerError();
            }
        }
    }
}
