using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using AccessControl.Repositories;
using System.Net;
using System.Text;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.Caching;
using System.Web.Http;
using System.Web.Http.Description;
using System.Web;
using System.Web.Http.Results;
using AccessControl;
using AccessControl.Models;

namespace AccessCotrol.Controllers
{
	[RoutePrefix("sessions")]
	public class SessionsController : ApiController
	{
		private static readonly MemoryCache Cache = new MemoryCache("SessionsCache");

		private readonly ISessionsRepository _repo = new SqlSessionsRepository();

		static SessionsController()
		{
			var repo = new SqlSessionsRepository();
			int ps = 100;
			int pn = 0;
			IEnumerable<Session> sessions = null;
			do
			{
				sessions = repo.SelectSessions(ps, pn).Result;
				foreach (var session in sessions)
				{
					Cache.Add(Session.Convert(session.SessionId), session.UserId, new CacheItemPolicy()
					{
						SlidingExpiration = TimeSpan.FromSeconds(WebApiConfig.SessionTimeoutInterval),
						//UpdateCallback = new CacheEntryUpdateCallback(i => repo.EndSession(Session.Convert(i.Key)))
					});
				}
			} while (sessions.Count() == ps);
		}

		public SessionsController()
		{
		}

		internal SessionsController(ISessionsRepository repo)
		{
			_repo = repo;
		}

		[HttpPost]
		[Route("{id}")]
		[ResponseType(typeof(HttpResponseMessage))]
		public async Task<IHttpActionResult> PostSession(int id)
		{
			try
			{
				var val = await _repo.BeginSession(id);
				if (val == null)
				{
					throw new KeyNotFoundException();
				}
				var guid = val.Value;
				var token = Session.Convert(guid);
				Cache.Add(token, (object) id, new CacheItemPolicy()
				{
					SlidingExpiration = TimeSpan.FromSeconds(WebApiConfig.SessionTimeoutInterval),
					UpdateCallback = new CacheEntryUpdateCallback(i => _repo.EndSession(Session.Convert(i.Key)))
				});
				var response = Request.CreateResponse(HttpStatusCode.OK);
				response.Headers.Add("Cache-Control", "no-cache, no-store, must-revalidate"); // HTTP 1.1.
				response.Headers.Add("Pragma", "no-cache"); // HTTP 1.0.
				response.Headers.Add("Session-Token", token);
				return ResponseMessage(response);
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

		[HttpPut]
		[Route("{op}/{id}")]
		[ResponseType(typeof(HttpResponseMessage))]
		public IHttpActionResult PutSession(string op, int id)
		{
			try
			{
				var token = Request.Headers.GetValues("Session-Token").FirstOrDefault();
				Session.Convert(token);
				var val = Cache.Get(token);
				if (val == null)
				{
					throw new KeyNotFoundException();
				}
				var response = Request.CreateResponse(HttpStatusCode.OK);
				response.Headers.Add("Cache-Control", "no-cache, no-store, must-revalidate"); // HTTP 1.1.
				response.Headers.Add("Pragma", "no-cache"); // HTTP 1.0.
				response.Headers.Add("Session-Token", token);
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

		[HttpDelete]
		[Route("")]
		[ResponseType(typeof(HttpResponseMessage))]
		public async Task<IHttpActionResult> DeleteSession()
		{
			try
			{
				var token = Request.Headers.GetValues("Session-Token").FirstOrDefault();
				var guid = Session.Convert(token);
				if (Cache.Remove(token) == null)
				{
					throw new KeyNotFoundException();
				}
				await _repo.EndSession(guid);
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


		[HttpGet]
		[Route("{ps}/{pn}")]
		[ResponseType(typeof(HttpResponseMessage))]
		public async Task<IHttpActionResult> SelectSessions(int ps, int pn)
		{
			try
			{
				var token = Request.Headers.GetValues("Session-Token").FirstOrDefault();
				var guid = Session.Convert(token);
				return Ok(await _repo.SelectSessions(ps, pn)); // reading from database to avoid extending existing session timeouts
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
