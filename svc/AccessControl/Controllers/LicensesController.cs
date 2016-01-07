using AccessControl.Repositories;
using ServiceLibrary.Models;
using ServiceLibrary.Repositories.ConfigControl;
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using ServiceLibrary.Attributes;

namespace AccessControl.Controllers
{
    [RoutePrefix("licenses")]
    public class LicensesController : ApiController
    {
        internal readonly ILicensesRepository _repo;
        internal readonly ISessionsRepository _sessions;
        private static IServiceLogRepository _log;

        public LicensesController() : this(new SqlLicensesRepository(), new SqlSessionsRepository(), new ServiceLogRepository())
        {
        }

        internal LicensesController(ILicensesRepository repo, ISessionsRepository sessions, IServiceLogRepository log)
        {
            _repo = repo;
            _sessions = sessions;
            _log = log;
        }

        private string GetHeaderSessionToken()
        {
            if (Request.Headers.Contains("Session-Token") == false)
                throw new ArgumentNullException();
            return Request.Headers.GetValues("Session-Token").FirstOrDefault();
        }

        [HttpGet, NoCache]
        [Route("active")]
        [ResponseType(typeof(HttpResponseMessage))]
        public async Task<IHttpActionResult> GetActiveLicenses()
        {
            try
            {
                GetHeaderSessionToken();
                var licenses = await _repo.GetActiveLicenses(DateTime.UtcNow, WebApiConfig.LicenseHoldTime);

                var response = Request.CreateResponse(HttpStatusCode.OK, licenses);
                return ResponseMessage(response);
            }
            catch (ArgumentNullException)
            {
                return Unauthorized();
            }
            catch (Exception ex)
            {
                await _log.LogError(WebApiConfig.LogSource_Licenses, ex);
                return InternalServerError();
            }
        }

        [HttpGet, NoCache]
        [Route("locked")]
        [ResponseType(typeof(HttpResponseMessage))]
        public async Task<IHttpActionResult> GetLockedLicenses()
        {
            try
            {
                var token = GetHeaderSessionToken();
                var session = await _sessions.GetSession(Session.Convert(token));
                var licenses = await _repo.GetLockedLicenses(session.UserId, session.LicenseLevel, WebApiConfig.LicenseHoldTime);

                var response = Request.CreateResponse(HttpStatusCode.OK,
                    new LicenseInfo { LicenseLevel = session.LicenseLevel, Count = licenses });
                return ResponseMessage(response);
            }
            catch (ArgumentNullException)
            {
                return Unauthorized();
            }
            catch (Exception ex)
            {
                await _log.LogError(WebApiConfig.LogSource_Licenses, ex);
                return InternalServerError();
            }
        }

        [HttpGet, NoCache]
        [Route("transactions")]
        [ResponseType(typeof(HttpResponseMessage))]
        public async Task<IHttpActionResult> GetLicenseTransactions(int days, int consumerType)
        {
            try
            {
                GetHeaderSessionToken();
                var licenses = await _repo.GetLicenseTransactions(DateTime.UtcNow.AddDays(-days), consumerType);

                var response = Request.CreateResponse(HttpStatusCode.OK, licenses);
                return ResponseMessage(response);
            }
            catch (ArgumentNullException)
            {
                return Unauthorized();
            }
            catch (Exception ex)
            {
                await _log.LogError(WebApiConfig.LogSource_Licenses, ex);
                return InternalServerError();
            }
        }
    }
}
