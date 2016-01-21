﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Description;
using AccessControl.Helpers;
using AccessControl.Repositories;
using ServiceLibrary.Attributes;
using ServiceLibrary.Models;
using ServiceLibrary.Repositories.ConfigControl;

namespace AccessControl.Controllers
{
    [RoutePrefix("sessions")]
    public class SessionsController : ApiController
    {
        private static readonly ITimeoutManager<Guid> Sessions = new TimeoutManager<Guid>();

        internal readonly ITimeoutManager<Guid> _sessions;
        internal readonly ISessionsRepository _repo;
        internal readonly IServiceLogRepository _log;

        public SessionsController() : this(Sessions, new SqlSessionsRepository(), new ServiceLogRepository())
        {
        }

        internal SessionsController(ITimeoutManager<Guid> sessions, ISessionsRepository repo, IServiceLogRepository log)
        {
            _sessions = sessions;
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
                            InsertSession(session);
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
                var session = await _repo.BeginSession(uid, userName, licenseLevel, isSso, id => _sessions.Remove(id));
                if (session == null)
                {
                    throw new KeyNotFoundException();
                }
                var token = Session.Convert(session.SessionId);
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
                var session = await _repo.ExtendSession(guid);
                if (session == null)
                {
                    throw new KeyNotFoundException();
                }
                InsertSession(session);
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
                _sessions.Remove(guid);
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
                _sessions.Remove(session.SessionId);
                await _repo.EndSession(session.SessionId, true);
            });
        }
    }
}
