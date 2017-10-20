using AccessControl.Helpers;
using AccessControl.Repositories;
using ServiceLibrary.Attributes;
using ServiceLibrary.Helpers;
using ServiceLibrary.Models;
using ServiceLibrary.Repositories.ConfigControl;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.Caching;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;

namespace AccessControl.Controllers
{
    [ApiControllerJsonConfig]
    [RoutePrefix("sessions")]
    public class SessionsController : ApiController
    {
        private static readonly ITimeoutManager<Guid> Sessions = new TimeoutManager<Guid>();
        private static readonly ObjectCache SessionsCache = new MemoryCache("USER_SESSIONS");

        private readonly ITimeoutManager<Guid> _sessions;
        private readonly ObjectCache _sessionsCache;
        private readonly ISessionsRepository _repo;
        private readonly IServiceLogRepository _log;

        public SessionsController() : this(Sessions, SessionsCache, new SqlSessionsRepository(), new ServiceLogRepository())
        {
        }

        internal SessionsController(ITimeoutManager<Guid> sessions, ObjectCache sessionsCache, ISessionsRepository repo, IServiceLogRepository log)
        {
            _sessions = sessions;
            _sessionsCache = sessionsCache;
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

                    var ps = 1000;
                    var pn = 1;
                    int count;
                    var expiredSessions = new List<Session>();

                    do
                    {
                        count = 0;
                        var sessions = await _repo.SelectSessions(ps, pn);

                        foreach (var session in sessions)
                        {
                            ++count;

                            if (session.IsExpired())
                            {
                                expiredSessions.Add(session);
                            }
                            else
                            {
                                InsertSession(session);
                            }
                        }

                        ++pn;
                    }
                    while (count == ps);

                    // Mark expired sessions as trully expired in the DB
                    foreach (var session in expiredSessions)
                    {
                        await _repo.EndSession(session.SessionId, session.EndTime);
                    }
                    var logExpiredSessionsCount = expiredSessions.Count > 0 ? $" {expiredSessions.Count} sessions expired" : string.Empty;

                    await _log.LogInformation(WebApiConfig.LogSourceSessions, $"Service started.{logExpiredSessionsCount}");
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
            {
                throw new ArgumentNullException();
            }

            return Request.Headers.GetValues("Session-Token").FirstOrDefault();
        }

        [HttpGet, NoCache]
        [Route("{uid}")]
        [ResponseType(typeof(Session))]
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
        [ResponseType(typeof(IEnumerable<Session>))]
        public async Task<IHttpActionResult> SelectSessions(string ps = "100", string pn = "1")
        {
            try
            {
                int psIntValue, pnIntValue;
                if (int.TryParse(ps, out psIntValue) == false ||
                    int.TryParse(pn, out pnIntValue) == false ||
                    psIntValue <= 0 ||
                    pnIntValue <= 0)
                {
                    throw new FormatException("Specified parameter is not valid.");
                }

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
                var session = await _repo.BeginSession(uid, userName, licenseLevel, isSso, id => _sessions.Remove(id));
                if (session == null)
                {
                    throw new KeyNotFoundException();
                }

                var token = Session.Convert(session.SessionId);
                _sessionsCache.Add(token, session, DateTimeOffset.UtcNow.Add(SessionsCacheSettings.SessionCacheExpiration));
                InsertSession(session);
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
                var session = SessionsCacheSettings.IsSessionCacheEnabled
                    ? _sessionsCache.Get(token) as Session
                    : null;

                if (session == null)
                {
                    session = await _repo.ExtendSession(guid);

                    if (session == null)
                    {
                        throw new KeyNotFoundException();
                    }

                    _sessionsCache.Add(token, session, DateTimeOffset.UtcNow.Add(SessionsCacheSettings.SessionCacheExpiration));
                    InsertSession(session);
                }

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

                if (await _repo.EndSession(guid) == null)
                {
                    throw new KeyNotFoundException();
                }

                RemoveCachedSession(guid);

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

        [HttpDelete]
        [Route("{uid}")]
        [ResponseType(typeof(HttpResponseMessage))]
        public async Task<IHttpActionResult> DeleteSession(int uid)
        {
            try
            {
                var session = await _repo.GetUserSession(uid);
                if (session == null || session.IsExpired())
                {
                    throw new KeyNotFoundException();
                }

                if (await _repo.EndSession(session.SessionId) == null)
                {
                    throw new KeyNotFoundException();
                }

                RemoveCachedSession(session.SessionId);

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

        private void InsertSession(Session session)
        {
            _sessions.Insert(session.SessionId, session.EndTime, async () =>
            {
                RemoveCachedSession(session.SessionId);
                await _repo.EndSession(session.SessionId, session.EndTime);
            });
        }

        private void RemoveCachedSession(Guid sessionId)
        {
            _sessions.Remove(sessionId);
            _sessionsCache.Remove(sessionId.ToString());
        }
    }
}
