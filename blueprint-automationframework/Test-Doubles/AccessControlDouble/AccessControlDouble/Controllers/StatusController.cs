using System;
using System.Configuration;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using Common;
using CommonUtilities;

namespace AccessControlDouble.Controllers
{
    [RoutePrefix("status")]
    public class StatusController : ApiController
    {
        /// <summary>
        /// Method to return current status of AccessControl Web Service.
        /// </summary>
        /// <returns>200 OK if no issue is detected.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]   // Ignore this warning.
        [HttpGet]
        [Route("")]
        [ResponseType(typeof(HttpResponseMessage))]
        public async Task<IHttpActionResult> GetStatus()
        {
            using (LogFile logFile = new LogFile(WebApiConfig.LogFile))
            using (HttpClient http = new HttpClient())
            {
                logFile.WriteLine("Called AccessControlDouble.StatusController.GetStatus()");

                WebUtils.ConfigureHttpClient(http, Request, WebApiConfig.AccessControl);

                var uri = WebUtils.CreateUri(Request.RequestUri, WebApiConfig.AccessControl, WebApiConfig.SVC_PATH);

                logFile.WriteLine("Calling http.GetAsync()");
                var result = await http.GetAsync(uri);
                WebUtils.LogRestResponse(logFile, result);

                return ResponseMessage(result);
            }
        }
    }
}
