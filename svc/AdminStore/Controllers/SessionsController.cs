using AdminStore.Models;
using AdminStore.Repositories;
using AdminStore.Saml;
using ServiceLibrary.Attributes;
using ServiceLibrary.Exceptions;
using ServiceLibrary.Helpers;
using ServiceLibrary.Helpers.Security;
using ServiceLibrary.Models;
using ServiceLibrary.Repositories.ConfigControl;
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.Remoting;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Http.Description;

namespace AdminStore.Controllers
{
    /// <summary>
    /// SessionsController
    /// </summary>
    [ApiControllerJsonConfig]
    [RoutePrefix("sessions")]
    public class SessionsController : ApiController
    {
        private readonly IAuthenticationRepository _authenticationRepository;
        private readonly IHttpClientProvider _httpClientProvider;
        private readonly IServiceLogRepository _log;

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
                var http = _httpClientProvider.Create(new Uri(WebApiConfig.AccessControl));
                if (!force)
                {
                    var result2 = await http.GetAsync("sessions/" + user.Id);
                    if (result2.IsSuccessStatusCode) // session exists
                    {
                        throw new ApplicationException("Conflict");
                    }
                }

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
                await _log.LogInformation(WebApiConfig.LogSourceSessions, $"{ex.Message}.{ex.InnerException?.Message ?? ""}" );
                if (ex.ErrorCode == FederatedAuthenticationErrorCode.WrongFormat)
                {
                    throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.BadRequest, ex.CreateHttpError(ErrorCodes.FederatedAuthenticationException)));
                }

                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.Unauthorized, ex.CreateHttpError(ErrorCodes.FederatedAuthenticationException)));
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
        /// <response code="401">Unauthorized. The session token is invalid, missing or malformed.</response>
        /// <response code="500">Internal Server Error. An error occurred.</response>
        [HttpDelete]
        [Route(""), SessionRequired]
        [ResponseType(typeof(void))]
        public async Task<IHttpActionResult> DeleteSession()
        {
            try
            {
                var uri = new Uri(WebApiConfig.AccessControl);
                var http = _httpClientProvider.Create(uri);
                if (!Request.Headers.Contains("Session-Token"))
                {
                    throw new ArgumentNullException();
                }

                var request = new HttpRequestMessage { RequestUri = new Uri(uri, "sessions"), Method = HttpMethod.Delete };
                request.Headers.Add("Session-Token", Request.Headers.GetValues("Session-Token").First());
                var result = await http.SendAsync(request);
                if (result.IsSuccessStatusCode)
                {
                    return Ok();
                }

                return ResponseMessage(result);
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

        /// <summary>
        /// IsSessionAlive
        /// </summary>
        /// <remarks>
        /// Returns 200 OK. Used to check if session is alive.
        /// </remarks>
        /// <response code="200">OK.</response>
        /// <response code="401">Unauthorized. The session token is invalid, missing or malformed.</response>
        [HttpGet, NoCache]
        [Route("alive"), SessionRequired]
        public IHttpActionResult IsSessionAlive()
        {
            return Ok();
        }
    }
}
