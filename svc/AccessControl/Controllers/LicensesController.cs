using AccessControl.Repositories;
using ServiceLibrary.Models;
using ServiceLibrary.Repositories.ConfigControl;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using ServiceLibrary.Attributes;

namespace AccessControl.Controllers
{
    [ApiControllerJsonConfig]
    [RoutePrefix("licenses")]
    public class LicensesController : ApiController
    {
        internal readonly ILicensesRepository _repo;
        internal readonly ISessionsRepository _sessions;
        internal readonly IServiceLogRepository _log;

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

        /// <summary>
        /// GetActiveLicenses
        /// </summary>
        /// <remarks>
        /// Returns information about the number of active licenses for each license level.
        /// </remarks>
        /// <response code="200">OK.</response>
        /// <response code="500">Internal Server Error. An error occurred.</response>
        [HttpGet, NoCache]
        [Route("active")]
        [ResponseType(typeof(IEnumerable<LicenseInfo>))]
        public async Task<IHttpActionResult> GetActiveLicenses()
        {
            try
            {
                var licenses = await _repo.GetActiveLicenses(DateTime.UtcNow, WebApiConfig.LicenseHoldTime);

                var response = Request.CreateResponse(HttpStatusCode.OK, licenses);
                return ResponseMessage(response);
            }
            catch (Exception ex)
            {
                await _log.LogError(WebApiConfig.LogSourceLicenses, ex);
                return InternalServerError();
            }
        }

        /// <summary>
        /// GetLockedLicenses
        /// </summary>
        /// <remarks>
        /// Returns information about the number of active licenses for the current user's license level,
        /// excluding any active license for the current user.
        /// </remarks>
        /// <response code="200">OK.</response>
        /// <response code="401">Unauthorized. The session token is invalid.</response>
        /// <response code="500">Internal Server Error. An error occurred.</response>
        [HttpGet, NoCache]
        [Route("locked")]
        [ResponseType(typeof(LicenseInfo))]
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
                await _log.LogError(WebApiConfig.LogSourceLicenses, ex);
                return InternalServerError();
            }
        }

        /// <summary>
        /// GetLicenseTransactions
        /// </summary>
        /// <remarks>
        /// Returns license transactions for the given <paramref name="consumerType" />, for the past <paramref name="days" /> days.
        /// </remarks>
        /// <param name="days">The number of past days for which to return transactions.</param>
        /// <param name="consumerType">TODO</param>
        /// <response code="200">OK.</response>
        /// <response code="500">Internal Server Error. An error occurred.</response>
        [HttpGet, NoCache]
        [Route("transactions")]
        [ResponseType(typeof(IEnumerable<LicenseTransaction>))]
        public async Task<IHttpActionResult> GetLicenseTransactions(int days, int consumerType)
        {
            try
            {
                var licenses = await _repo.GetLicenseTransactions(DateTime.UtcNow.AddDays(-days), consumerType);

                var response = Request.CreateResponse(HttpStatusCode.OK, licenses);
                return ResponseMessage(response);
            }
            catch (Exception ex)
            {
                await _log.LogError(WebApiConfig.LogSourceLicenses, ex);
                return InternalServerError();
            }
        }

        /// <summary>
        /// Provides license usage information 
        /// </summary>
        /// <remarks>
        /// Returns license usage for the given <paramref name="month" /> and <paramref name="year" />.
        /// </remarks>
        /// <param name="month">The number of the month to get usage for.</param>
        /// <param name="year">The number of the year to get usage for</param>
        /// <response code="200">OK.</response>
        /// <response code="500">Internal Server Error. An error occurred.</response>
        [HttpGet, NoCache]
        [Route("usage")]
        [ResponseType(typeof(IEnumerable<LicenseUsage>))]
        public async Task<IHttpActionResult> GetLicenseUsage(int month, int year)
        {
            try
            {
                var usage = await _repo.GetLicenseUsage(month, year);

                var response = Request.CreateResponse(HttpStatusCode.OK, usage);
                return ResponseMessage(response);
            }
            catch (Exception ex)
            {
                await _log.LogError(WebApiConfig.LogSourceLicenses, ex);
                return InternalServerError();
            }
        }
    }
}
