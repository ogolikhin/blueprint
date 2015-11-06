using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Net;
using System.Net.Http;
using System.Runtime.Caching;
using System.Web.Http;
using System.Web.Http.Description;
using AccessControl.Models;
using AccessControl.Repositories;

namespace AccessControl.Controllers
{
	[RoutePrefix("sessions")]
	public class SessionsController : ApiController
	{
		private static readonly ObjectCache Cache = new MemoryCache("SessionsCache");
		private static ISessionsRepository Repo = new SqlSessionsRepository(WebApiConfig.AdminStoreDatabase);

		public static object Trigger = null;

		static SessionsController()
		{
			if (!EventLog.SourceExists(WebApiConfig.ServiceLogSource))
				EventLog.CreateEventSource(WebApiConfig.ServiceLogSource, WebApiConfig.ServiceLogName);
			Task.Run(() =>
			{
				try
				{
					EventLog.WriteEntry(WebApiConfig.ServiceLogSource, "Service starting...", EventLogEntryType.Information);
					var ps = 100;
					var pn = 1;
					var count = 0;
					do
					{
						count = 0;
						var sessions = Repo.SelectSessions(ps, pn).Result;
						foreach (var session in sessions)
						{
							++count;
							AddSession(Session.Convert(session.SessionId), session.UserId);
						}
						++pn;
					} while (count == ps);
					StatusController.Ready.Set();
					EventLog.WriteEntry(WebApiConfig.ServiceLogSource, "Service started.", EventLogEntryType.Information);
				}
				catch (Exception)
				{
					EventLog.WriteEntry(WebApiConfig.ServiceLogSource, "Error loading sessions from database.", EventLogEntryType.Error);
				}
			});
		}

		public SessionsController()
		{
		}

		internal SessionsController(ISessionsRepository repo)
		{
			Repo = repo;
		}

		[HttpGet]
		[Route("{uid}")]
		[ResponseType(typeof(HttpResponseMessage))]
		public async Task<IHttpActionResult> GetSession(int uid)
		{
			try
			{
				var token = Request.Headers.GetValues("Session-Token").FirstOrDefault();
				var session = await Repo.GetSession(Session.Convert(token)); // reading from database to avoid extending existing session
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

		[HttpGet]
		[Route("select?ps={ps}&pn={pn}")]
		[ResponseType(typeof(HttpResponseMessage))]
		public async Task<IHttpActionResult> SelectSessions(int ps, int pn)
		{
			try
			{
				var token = Request.Headers.GetValues("Session-Token").FirstOrDefault();
				var guid = Session.Convert(token);
				return Ok(await Repo.SelectSessions(ps, pn)); // reading from database to avoid extending existing session
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
		[Route("{uid}")]
		[ResponseType(typeof(HttpResponseMessage))]
		public async Task<IHttpActionResult> PostSession(int uid)
		{
			try
			{
				var guids = await Repo.BeginSession(uid);
				if (!guids[0].HasValue)
				{
					throw new KeyNotFoundException();
				}
				var token = Session.Convert(guids[0].Value);
				if (guids[1].HasValue)
				{
					Cache.Remove(Session.Convert(guids[1].Value));
				}
				AddSession(token, uid);
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
		[Route("{op}/{aid}")]
		[ResponseType(typeof(HttpResponseMessage))]
		public IHttpActionResult PutSession(string op, int aid)
		{
			try
			{
				var token = Request.Headers.GetValues("Session-Token").FirstOrDefault();
				Session.Convert(token);
				var uid = Cache.Get(token);
				if (uid == null)
				{
					throw new KeyNotFoundException();
				}
				var response = Request.CreateResponse(HttpStatusCode.OK, uid);
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
				await Repo.EndSession(guid);
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

		private static void AddSession(string key, int id)
		{
			Cache.Add(key, (object) id, new CacheItemPolicy()
			{
				SlidingExpiration = TimeSpan.FromSeconds(WebApiConfig.SessionTimeoutInterval),
				UpdateCallback = new CacheEntryUpdateCallback(a => Repo.EndSession(Session.Convert(a.Key)))
			});
		}
	}
}
