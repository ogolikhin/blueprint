using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using Common;
using CommonUtilities;

namespace AccessControlDouble.Controllers
{
    [RoutePrefix("licenses")]
    public class LicensesController : ApiController
    {
        #region Private functions

        /// <summary>
        /// Creates a copy of the request Uri that points to the real AccessControl.
        /// </summary>
        /// <returns>The new Uri.</returns>
        private Uri CreateUri()
        {
            return WebUtils.CreateUri(Request.RequestUri, WebApiConfig.AccessControl, WebApiConfig.SVC_PATH);
        }


        /// <summary>
        /// Writes a formatted line into the log file.
        /// </summary>
        /// <param name="format">The format string to write.</param>
        /// <param name="args">The format arguments.</param>
        private static void WriteLine(string format, params Object[] args)
        {
            using (LogFile logFile = new LogFile(WebApiConfig.LogFile))
            {
                logFile.WriteLine(format, args);
            }
        }

        #endregion Private functions

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
            string thisNamespace = nameof(AccessControlDouble);
            string thisClassName = nameof(LicensesController);
            string thisMethodName = nameof(GetLicenseTransactions);

            using (HttpClient http = new HttpClient())
            {
                await Task.Run(() =>
                {
                    WriteLine("Called {0}.{1}.{2}({3})", thisNamespace, thisClassName, thisMethodName, days);
                });

                // If the test wants to inject a custom status code, return that instead of the real value.
                if (WebApiConfig.StatusCodeToReturn["GET"].HasValue)
                {
                    return ResponseMessage(Request.CreateResponse(WebApiConfig.StatusCodeToReturn["GET"].Value));
                }

                WebUtils.ConfigureHttpClient(http, Request, WebApiConfig.AccessControl);
                var uri = CreateUri();

                await Task.Run(() =>
                {
                    WriteLine("Calling http.GetAsync({0})", days);
                });
                var result = await http.GetAsync(uri);
                await Task.Run(() =>
                {
                    WebUtils.LogRestResponse(WebApiConfig.LogFile, result);
                });

                return ResponseMessage(result);
            }
        }

    }
}
