using System;
using System.Collections.Generic;
using System.Diagnostics;
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

namespace AdminStore.Controllers
{
    [RoutePrefix("sessions")]
    public class SessionsController : ApiController
    {
        private readonly IAuthenticationRepository _authenticationRepository;

        public SessionsController(): this(new AuthenticationRepository())
        {
        }

        internal SessionsController(IAuthenticationRepository authenticationRepository)
        {
            _authenticationRepository = authenticationRepository;
        }

		[HttpGet]
		[Route("select")]
		[ResponseType(typeof(HttpResponseMessage))]
		public async Task<IHttpActionResult> SelectSessions(int ps, int pn)
		{
			try
			{
                using (var http = CreateHttpClient())
				{
					http.BaseAddress = new Uri(WebApiConfig.AccessControl);
					http.DefaultRequestHeaders.Accept.Clear();
					http.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
					http.DefaultRequestHeaders.Add("Session-Token", Request.Headers.GetValues("Session-Token").FirstOrDefault());
					var result = await http.GetAsync(String.Format("sessions/select?ps={0}&pn={1}", ps, pn));
					result.EnsureSuccessStatusCode();
					var response = Request.CreateResponse(HttpStatusCode.OK, result.Content);
					response.Headers.Add("Cache-Control", "no-cache, no-store, must-revalidate"); // HTTP 1.1.
					response.Headers.Add("Pragma", "no-cache"); // HTTP 1.0.
					return ResponseMessage(response);
				}
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

        [HttpPost]
        [Route("")]
        [ResponseType(typeof(HttpResponseMessage))]
        public async Task<IHttpActionResult> PostSession(string login, string password, bool force = false)
        {
            try
            {
                var user = await _authenticationRepository.AuthenticateUserAsync(login, password);
                if (!force)
                {
                    using (var http = CreateHttpClient())
                    {
                        http.BaseAddress = new Uri(WebApiConfig.AccessControl);
                        http.DefaultRequestHeaders.Accept.Clear();
                        http.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                        var result = await http.GetAsync("sessions/" + user.Id.ToString());
                        if (result.IsSuccessStatusCode) // session exists
                        {
                            throw new ApplicationException("Conflict");
                        }
                    }
                }
                return await RequestSessionTokenAsync(user.Id);
            }
            catch (AuthenticationException)
            {
                return NotFound();
            }
            catch (ApplicationException ex)
            {
                Debug.Write(ex.Message);
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
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch
            {
                return InternalServerError();
            }
        }

        private async Task<IHttpActionResult> RequestSessionTokenAsync(int userId)
        {
            using (var http = CreateHttpClient())
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
                var response = Request.CreateResponse(HttpStatusCode.OK, token);
                response.Headers.Add("Session-Token", token);
                return ResponseMessage(response);
            }
        }

        [HttpPost]
        [Route("sso")]
        [ResponseType(typeof(HttpResponseMessage))]
        public async Task<IHttpActionResult> PostSessionSingleSignOn(string samlResponse)
        {
            try
            {
                var user = await _authenticationRepository.AuthenticateSamlUserAsync(samlResponse);
                return await RequestSessionTokenAsync(user.Id);
            }
            catch (FederatedAuthenticationException)
            {
                return NotFound();
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
                using (var http = CreateHttpClient())
                {
                    http.BaseAddress = new Uri(WebApiConfig.AccessControl);
                    http.DefaultRequestHeaders.Accept.Clear();
                    http.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    http.DefaultRequestHeaders.Add("Session-Token", Request.Headers.GetValues("Session-Token").FirstOrDefault());
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

        /// <summary>
        /// Creates HttpClient. Hook for unit tests.
        /// </summary>
        /// <returns></returns>
        protected virtual HttpClient CreateHttpClient()
        {
            return new HttpClient(new FakeResponseHandler());
        }
    }

    public class FakeResponseHandler : DelegatingHandler
    {
        protected async override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, System.Threading.CancellationToken cancellationToken)
        {
            return await Task.Run(() =>
            {
                var httpResponseMessage = new HttpResponseMessage(HttpStatusCode.OK);
                httpResponseMessage.Headers.Add("Session-Token", Guid.NewGuid().ToString());
                return httpResponseMessage;
            }, cancellationToken);
        }
    }
}
