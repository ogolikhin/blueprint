using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Description;
using AdminStore.Repositories;
using AdminStore.Models;

namespace AdminStore.Controllers
{
	[RoutePrefix("sessions")]
	public class SessionsController : ApiController
	{
		public SessionsController()
		{
		}

		[HttpGet]
		[Route("select?ps={ps}&pn={pn}")]
		[ResponseType(typeof(HttpResponseMessage))]
		public async Task<IHttpActionResult> SelectSessions(int ps, int pn)
		{
			try
			{
				var token = Request.Headers.GetValues("Session-Token").FirstOrDefault();
				// var guid = Session.Convert(token);
				// return Ok(await Repo.SelectSessions(ps, pn)); // reading from database to avoid extending existing session
				return Ok(); // TODO: read from AccessControl SelectSessions
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
		[Route("")]
		[ResponseType(typeof(HttpResponseMessage))]
		public async Task<IHttpActionResult> PostSession(string un, string pw)
		{
			try
			{
				var token = Request.Headers.GetValues("Session-Token").FirstOrDefault();
				//var session = await _repoSessions.GetSession(Session.Convert(token)); // reading from database to avoid extending existing session
				object session = null; // read from AccessControl getSession
				if (session == null)
				{
					throw new KeyNotFoundException();
				}
				var response = Request.CreateResponse(HttpStatusCode.OK, session);
				response.Headers.Add("Cache-Control", "no-cache, no-store, must-revalidate"); // HTTP 1.1.
				response.Headers.Add("Pragma", "no-cache"); // HTTP 1.0.
				return ResponseMessage(response);
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
				return Ok();
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
				// call access control to kill session
				return Ok();
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
	}
}
