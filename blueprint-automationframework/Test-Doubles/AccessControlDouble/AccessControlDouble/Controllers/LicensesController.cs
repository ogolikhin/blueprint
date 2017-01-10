using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;

namespace AccessControlDouble.Controllers
{
    [RoutePrefix("licenses")]
    public class LicensesController : BaseController
    {
        /// <summary>
        /// Method to query if session exists, expect to receive session token in header Session-Token to identify user session.
        /// Method will not extend lifetime of the session by SESSION_TIMEOUT.
        /// This method to be used for sign in sequence only.  Session information is returned back.
        /// </summary>
        /// <param name="days">The number of past days for which to return transactions.</param>
        /// <returns>The session token for the specified user.</returns>
        [HttpGet]
        [Route("transactions")]
        [ResponseType(typeof(HttpResponseMessage))]
        public async Task<IHttpActionResult> GetLicenseTransactions(int days)
        {
            string thisClassName = nameof(LicensesController);
            string thisMethodName = nameof(GetLicenseTransactions);

            var args = new List<string> { days.ToString() };

            return await ProxyGetRequest(thisClassName, thisMethodName, args);
        }

        /// <summary>
        /// Returns information about the number of active licenses for each license level.
        /// </summary>
        /// <returns>JSON object like:  [{"LicenseLevel": 0, "Count": 0}]</returns>
        [HttpGet]
        [Route("active")]
        [ResponseType(typeof(HttpResponseMessage))]
        public async Task<IHttpActionResult> GetLicenseTransactions()
        {
            string thisClassName = nameof(LicensesController);
            string thisMethodName = nameof(GetLicenseTransactions);

            return await ProxyGetRequest(thisClassName, thisMethodName);
        }

        /// <summary>
        /// Returns information about the number of active licenses for the current user's license level,
        /// excluding any active license for the current user.
        /// </summary>
        /// <returns>JSON object like:  [{"LicenseLevel": 0, "Count": 0}]</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]   // Ignore this warning.
        [HttpGet]
        [Route("locked")]
        [ResponseType(typeof(HttpResponseMessage))]
        public async Task<IHttpActionResult> GetLockedLicenses()
        {
            string thisClassName = nameof(LicensesController);
            string thisMethodName = nameof(GetLockedLicenses);

            return await ProxyGetRequest(thisClassName, thisMethodName);
        }

        /// <summary>
        /// Returns license usage for the given {month} and {year}.
        /// </summary>
        /// <returns>JSON object like:  [{"LicenseLevel": 0, "Count": 0}]</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]   // Ignore this warning.
        [HttpGet]
        [Route("usage")]
        [ResponseType(typeof(HttpResponseMessage))]
        public async Task<IHttpActionResult> GetLicenseUsage(int? month = null, int? year = null)
        {
            string thisClassName = nameof(LicensesController);
            string thisMethodName = nameof(GetLicenseUsage);

            var args = new List<string>();

            if (month != null)
            {
                args.Add(month.ToString());
            }

            if (year != null)
            {
                args.Add(year.ToString());
            }

            return await ProxyGetRequest(thisClassName, thisMethodName, args);
        }
    }
}
