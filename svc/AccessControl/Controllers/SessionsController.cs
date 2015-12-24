using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Net;
using System.Net.Http;
using System.Runtime.Caching;
using System.Web.Http;
using System.Web.Http.Description;
using AccessControl.Repositories;
using ServiceLibrary.Log;
using ServiceLibrary.Models;

namespace AccessControl.Controllers
{
    [RoutePrefix("sessions")]
    public class SessionsController : ApiController
    {
        private static ObjectCache _cache;
        private static ISessionsRepository _repo = new SqlSessionsRepository(WebApiConfig.AdminStorage);

        internal static void Load(ObjectCache cache)
        {
            Task.Run(() =>
            {
                _cache = cache;
                try
                {
                    LogProvider.Current.WriteEntry(WebApiConfig.ServiceLogSource, "Service starting...", LogEntryType.Information);
                    var ps = 100;
                    var pn = 1;
                    int count;
                    do
                    {
                        count = 0;
                        var sessions = _repo.SelectSessions(ps, pn).Result;
                        foreach (var session in sessions)
                        {
                            ++count;
                            AddSession(Session.Convert(session.SessionId), session);
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
            : this(_cache, _repo)
        {
        }

        internal SessionsController(ObjectCache cache, ISessionsRepository repo)
        {
            _cache = cache;
            _repo = repo;
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
                var session = await _repo.GetUserSession(uid); // reading from database to avoid extending existing session
                if (session == null)
                {
                    throw new KeyNotFoundException();
                }

                if (session.EndTime != null)
                {
                    throw new KeyNotFoundException();
                }

                var response = Request.CreateResponse(HttpStatusCode.OK, session);
                response.Headers.Add("Cache-Control", "no-cache, no-store, must-revalidate"); // HTTP 1.1.
                response.Headers.Add("Pragma", "no-cache"); // HTTP 1.0.
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
                return Ok(await _repo.SelectSessions(psIntValue, pnIntValue)); // reading from database to avoid extending existing session
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
        public async Task<IHttpActionResult> PostSession(int uid, string userName, int licenseLevel, bool isSso = false)
        {
            try
            {
                var guids = await _repo.BeginSession(uid, userName, licenseLevel, isSso);
                if (!guids[0].HasValue)
                {
                    throw new KeyNotFoundException();
                }
                var token = Session.Convert(guids[0].Value);
                if (guids[1].HasValue)
                {
                    _cache.Remove(Session.Convert(guids[1].Value));
                }
                var session = await _repo.GetUserSession(uid);
                AddSession(token, session);
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
                var session = _cache.Get(token) as Session;
                if (session == null)
                {
                    session = await _repo.GetSession(guid);
                    if (session == null || session.EndTime.HasValue)
                    {
                        throw new KeyNotFoundException();
                    }
                    AddSession(token, session);
                }
                var response = Request.CreateResponse(HttpStatusCode.OK, session);
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

                await _repo.EndSession(guid, false);
                if (_cache.Remove(token) == null)
                {
                    throw new KeyNotFoundException();
                }
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

        private static void AddSession(string key, Session session)
        {
            _cache.Add(key, session, new CacheItemPolicy
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
                            _repo.EndSession(Session.Convert(args.CacheItem.Key), true);
                            break;
                    }
                }
            });
        }
    }
}
