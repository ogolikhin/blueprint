using System;
using System.Linq;
using System.Threading.Tasks;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Description;
using AccessControl.Repositories;
using ServiceLibrary.Models;

namespace AccessControl.Controllers
{
    [RoutePrefix("licenses")]
    public class LicensesController : ApiController
    {
        internal readonly ILicensesRepository _repo;
        internal readonly ISessionsRepository _sessions;

        public LicensesController(): this(new SqlLicensesRepository(), new SqlSessionsRepository())
        {
        }

        internal LicensesController(ILicensesRepository repo, ISessionsRepository sessions)
        {
            _repo = repo;
            _sessions = sessions;
        }

        private string GetHeaderSessionToken()
        {
            if (Request.Headers.Contains("Session-Token") == false)
                throw new ArgumentNullException();
            return Request.Headers.GetValues("Session-Token").FirstOrDefault();
        }

        [HttpGet]
        [Route("active")]
        [ResponseType(typeof(HttpResponseMessage))]
        public async Task<IHttpActionResult> GetActiveLicenses()
        {
            try
            {
                GetHeaderSessionToken();
                var licenses = await _repo.GetActiveLicenses(DateTime.UtcNow, WebApiConfig.LicenseHoldTime);

                var response = Request.CreateResponse(HttpStatusCode.OK, licenses);
                response.Headers.Add("Cache-Control", "no-cache, no-store, must-revalidate"); // HTTP 1.1.
                response.Headers.Add("Pragma", "no-cache"); // HTTP 1.0.
                return ResponseMessage(response);
            }
            catch (ArgumentNullException)
            {
                return Unauthorized();
            }
            catch
            {
                return InternalServerError();
            }
        }

        [HttpGet]
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
                    new LicenseInfo {LicenseLevel = session.LicenseLevel, Count = licenses});
                response.Headers.Add("Cache-Control", "no-cache, no-store, must-revalidate"); // HTTP 1.1.
                response.Headers.Add("Pragma", "no-cache"); // HTTP 1.0.
                return ResponseMessage(response);
            }
            catch (ArgumentNullException)
            {
                return Unauthorized();
            }
            catch
            {
                return InternalServerError();
            }
        }

        [HttpGet]
        [Route("transactions")]
        [ResponseType(typeof(HttpResponseMessage))]
        public async Task<IHttpActionResult> GetLicenseTransactions(int days, int consumerType)
        {
            try
            {
                GetHeaderSessionToken();
                var licenses = await _repo.GetLicenseTransactions(DateTime.UtcNow.AddDays(-days), consumerType);

                var response = Request.CreateResponse(HttpStatusCode.OK, licenses);
                response.Headers.Add("Cache-Control", "no-cache, no-store, must-revalidate"); // HTTP 1.1.
                response.Headers.Add("Pragma", "no-cache"); // HTTP 1.0.
                return ResponseMessage(response);
            }
            catch (ArgumentNullException)
            {
                return Unauthorized();
            }
            catch
            {
                return InternalServerError();
            }
        }
    }
}
