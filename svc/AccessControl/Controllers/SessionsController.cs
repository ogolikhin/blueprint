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
using ServiceLibrary.Log;

namespace AccessControl.Controllers
{
    [RoutePrefix("sessions")]
    public class SessionsController : ApiController
    {
        private static ObjectCache Cache;
        private static ISessionsRepository Repo = new SqlSessionsRepository(WebApiConfig.AdminStorage);        

        internal static void Load(ObjectCache cache)
        {           
            Task.Run(() =>
            {
                Cache = cache;
                try
                {
                    LogProvider.Current.WriteEntry(WebApiConfig.ServiceLogSource, "Service starting...", LogEntryType.Information);
                    var ps = 100;
                    var pn = 1;
                    int count;
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
                    LogProvider.Current.WriteEntry(WebApiConfig.ServiceLogSource, "Service started.", LogEntryType.Information);
                }
                catch (Exception)
                {
                    LogProvider.Current.WriteEntry(WebApiConfig.ServiceLogSource, "Error loading sessions from database.", LogEntryType.Error);
                }
            });
        }

        public SessionsController()
        {
        }

        internal SessionsController(ObjectCache cache, ISessionsRepository repo)
        {
            Cache = cache;
            Repo = repo;
        }

        private string GetHeaderSessionToken()
        {
            if(Request.Headers.Contains("Session-Token") == false)
                throw new ArgumentNullException();
            return Request.Headers.GetValues("Session-Token").FirstOrDefault();
        }

        [HttpGet]
        [Route("{uid}")]
        [ResponseType(typeof(HttpResponseMessage))]
        public async Task<IHttpActionResult> GetSession(int uid)
        {
            try
            {
                var session = await Repo.GetUserSession(uid); // reading from database to avoid extending existing session
                if (session == null)
                {
                    throw new KeyNotFoundException();
                }

                if (session.EndTime != null)
                {
                    return Unauthorized();
                }

                var token = Session.Convert(session.SessionId);

                var response = Request.CreateResponse(HttpStatusCode.OK, session);
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

        [HttpGet]
        [Route("select")]
        [ResponseType(typeof(HttpResponseMessage))]
        public async Task<IHttpActionResult> SelectSessions(string ps = "100", string pn = "1")
        {
            try
            {
                int psIntValue, pnIntValue;
                if (int.TryParse(ps, out psIntValue) == false || 
                    int.TryParse(pn, out pnIntValue) == false ||
                    psIntValue <= 0 ||
                    pnIntValue <= 0)
                    throw new FormatException("Specified parameter is not valid.");

                var token = GetHeaderSessionToken();
                //Todo: We need to use this guid in future to check validity of token for other calls rather than AdminStore
                var guid = Session.Convert(token);
                return Ok(await Repo.SelectSessions(psIntValue, pnIntValue)); // reading from database to avoid extending existing session
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
        public async Task<IHttpActionResult> PutSession(string op, int aid)
        {
            try
            {
                var token = GetHeaderSessionToken();
                var guid = Session.Convert(token);
                var userId = Cache.Get(token);
                if (userId == null)
                {
                    var session = await Repo.GetSession(guid);
                    if (session == null || session.EndTime.HasValue)
                    {
                        throw new KeyNotFoundException();
                    }
                    userId = session.UserId;
                }
                var response = Request.CreateResponse(HttpStatusCode.OK, userId);
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
                var token = GetHeaderSessionToken();
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
            Cache.Add(key, id, new CacheItemPolicy
            {
                SlidingExpiration = TimeSpan.FromSeconds(WebApiConfig.SessionTimeoutInterval),
                RemovedCallback = args =>
                {
                    switch (args.RemovedReason)
                    {
                        case CacheEntryRemovedReason.Evicted:
                            LogProvider.Current.WriteEntry(WebApiConfig.ServiceLogSource, "Not enough memory", LogEntryType.Error);
                            break;
                        case CacheEntryRemovedReason.Expired:
                            Repo.EndSession(Session.Convert(args.CacheItem.Key));
                            break;
                    }
                }
            });
        }
    }
}
