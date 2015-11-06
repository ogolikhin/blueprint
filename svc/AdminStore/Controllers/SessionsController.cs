using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Description;
using AdminStore.Helpers;
using AdminStore.Models;
using AdminStore.Repositories;
using System.Net.Http.Headers;
using System.Runtime.Remoting;

namespace AdminStore.Controllers
{
	[RoutePrefix("sessions")]
	public class SessionsController : ApiController
	{
	    private readonly IUserRepository _userRepository;

		public SessionsController() : this(new UserRepository())
		{
		}

        internal SessionsController(IUserRepository userRepository)
		{
            _userRepository = userRepository;
		}

		[HttpGet]
		[Route("select")]
		[ResponseType(typeof(HttpResponseMessage))]
		public async Task<IHttpActionResult> SelectSessions(int ps, int pn)
		{
			try
			{
				using (var http = new HttpClient())
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
			    var user = await _userRepository.GetUserByLogin(login);
			    if (user.Source == UserGroupSource.Database)
			    {
			        var autenticationHelper = new AuthenticationHelper();
			        var authenticationStatus = autenticationHelper.AuthenticateDatabaseUser(user, password, 0);
			    }
				if (!force)
				{
					using (var http = new HttpClient())
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
				using (var http = new HttpClient())
				{
					http.BaseAddress = new Uri(WebApiConfig.AccessControl);
					http.DefaultRequestHeaders.Accept.Clear();
					http.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    var result = await http.PostAsJsonAsync("sessions/" + user.Id.ToString(), user.Id);
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
			catch (KeyNotFoundException)
			{
				return NotFound();
			}
			catch
			{
				return InternalServerError();
			}
		}

        [HttpPost]
        [Route("sso")]
        [ResponseType(typeof(HttpResponseMessage))]
        public async Task<IHttpActionResult> PostSessionSingleSignOn()
        {
            try
            {
                // TODO: Migrate code from blueprint-current to handle SAML response and get user id (uid) from database
                var uid = 0;
                using (var http = new HttpClient())
                {
                    http.BaseAddress = new Uri(WebApiConfig.AccessControl);
                    http.DefaultRequestHeaders.Accept.Clear();
                    http.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    var result = await http.PostAsJsonAsync("sessions/" + uid.ToString(), uid);
                    result.EnsureSuccessStatusCode();
                    var token = result.Headers.GetValues("Session-Token").FirstOrDefault();
                    var response = Request.CreateResponse(HttpStatusCode.OK, token);
                    response.Headers.Add("Session-Token", token);
                    return ResponseMessage(response);
                }
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

        [HttpDelete]
        [Route("")]
        [ResponseType(typeof(HttpResponseMessage))]
        public async Task<IHttpActionResult> DeleteSession()
        {
            try
            {
                using (var http = new HttpClient())
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
    }
}
