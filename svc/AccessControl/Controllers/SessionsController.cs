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
using ServiceLibrary.Attributes;
using ServiceLibrary.Models;
using ServiceLibrary.Repositories.ConfigControl;

namespace AccessControl.Controllers
{
    [RoutePrefix("sessions")]
    public class SessionsController : ApiController
    {
        private static readonly ObjectCache Cache = new MemoryCache("SessionsCache");

        internal ObjectCache _cache;
        internal ISessionsRepository _repo;
        internal IServiceLogRepository _log;

        public SessionsController() : this(Cache, new SqlSessionsRepository(), new ServiceLogRepository())
        {
        }

        internal SessionsController(ObjectCache cache, ISessionsRepository repo, IServiceLogRepository log)
        {
            _cache = cache;
            _repo = repo;
            _log = log;
        }

        internal Task LoadAsync()
        {
            return Task.Run(async () =>
            {
                try
                {
                    await _log.LogInformation(WebApiConfig.LogSourceSessions, "Service starting...");
                    var ps = 100;
                    var pn = 1;
                    int count;
                    do
                    {
                        count = 0;
                        var sessions = await _repo.SelectSessions(ps, pn);
                        foreach (var session in sessions)
                        {
                            ++count;
                            SetSession(Session.Convert(session.SessionId), session);
                        }
                        ++pn;
                    } while (count == ps);
                    await _log.LogInformation(WebApiConfig.LogSourceSessions, "Service started.");
                }
                catch (Exception ex)
                {
                    await _log.LogError(WebApiConfig.LogSourceSessions,
                        new Exception("Error loading sessions from database.", ex));
                }
            });
        }

        private string GetHeaderSessionToken()
        {
            if (Request.Headers.Contains("Session-Token") == false)
                throw new ArgumentNullException();
            return Request.Headers.GetValues("Session-Token").FirstOrDefault();
        }

        [HttpGet, NoCache]
        [Route("{uid}")]
        [ResponseType(typeof(HttpResponseMessage))]
        public async Task<IHttpActionResult> GetSession(int uid)
        {
            try
            {
                var session = await _repo.GetUserSession(uid);
                if (session == null || session.IsExpired())
                {
                    throw new KeyNotFoundException();
                }

                var response = Request.CreateResponse(HttpStatusCode.OK, session);
                return ResponseMessage(response);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (Exception ex)
            {
                await _log.LogError(WebApiConfig.LogSourceSessions, ex);
                return InternalServerError();
            }
        }

        [HttpGet, NoCache]
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
            catch (Exception ex)
            {
                await _log.LogError(WebApiConfig.LogSourceSessions, ex);
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
                var session = await _repo.BeginSession(uid, userName, licenseLevel, isSso, id => _cache.Remove(Session.Convert(id)));
                if (session == null)
                {
                    throw new KeyNotFoundException();
                }
                var token = Session.Convert(session.SessionId);
                SetSession(token, session);
                var response = Request.CreateResponse(HttpStatusCode.OK);
                response.Headers.Add("Session-Token", token);
                return ResponseMessage(response);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (Exception ex)
            {
                await _log.LogError(WebApiConfig.LogSourceSessions, ex);
                return InternalServerError();
            }
        }

        [HttpPut]
        [Route("")]
        [ResponseType(typeof(HttpResponseMessage))]
        public async Task<IHttpActionResult> PutSession(string op = "", int aid = 0)
        {
            try
            {
                var token = GetHeaderSessionToken();
                var guid = Session.Convert(token);
                var session = await _repo.ExtendSession(guid);
                if (session == null || session.IsExpired())
                {
                    throw new KeyNotFoundException();
                }
                SetSession(token, session);
                var response = Request.CreateResponse(HttpStatusCode.OK, session);
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
            catch (Exception ex)
            {
                await _log.LogError(WebApiConfig.LogSourceSessions, ex);
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

                if (await _repo.EndSession(guid, false) == null)
                {
                    throw new KeyNotFoundException();
                }
                _cache.Remove(token);
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
            catch (Exception ex)
            {
                await _log.LogError(WebApiConfig.LogSourceSessions, ex);
                return InternalServerError();
            }
        }

        private void SetSession(string key, Session session)
        {
            var slidingExpiration = session.EndTime - DateTime.UtcNow;
            if (slidingExpiration >= TimeSpan.Zero)
            {
                _cache.Set(key, session, new CacheItemPolicy
                {
                    SlidingExpiration = slidingExpiration,
                    RemovedCallback = async args =>
                    {
                        switch (args.RemovedReason)
                        {
                            case CacheEntryRemovedReason.Evicted:
                                await _log.LogError(WebApiConfig.LogSourceSessions, "Not enough memory");
                                break;
                            case CacheEntryRemovedReason.Expired:
                                await _repo.EndSession(Session.Convert(args.CacheItem.Key), true);
                                break;
                        }
                    }
                });
            }
            else
            {
                _repo.EndSession(session.SessionId, true);
            }
        }
    }
}
