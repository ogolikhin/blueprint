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
    public class StatusController : BaseController
    {
        /// <summary>
        /// Method to return current upcheck status of AccessControl Web Service.
        /// </summary>
        /// <returns>200 OK if no issue is detected.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]   // Ignore this warning.
        [HttpGet]
        [Route("upcheck")]
        [ResponseType(typeof (HttpResponseMessage))]
        public async Task<IHttpActionResult> GetStatusUpcheck()
        {
            return await GetStatus();
        }

        /// <summary>
        /// Method to return current status of AccessControl Web Service.
        /// </summary>
        /// <returns>200 OK if no issue is detected.</returns>
        [HttpGet]
        [Route("")]
        [ResponseType(typeof(HttpResponseMessage))]
        public async Task<IHttpActionResult> GetStatus(string preAuthorizedKey = null)
        {
            string thisNamespace = nameof(AccessControlDouble);
            string thisClassName = nameof(StatusController);
            string thisMethodName = nameof(GetStatus);

            using (HttpClient http = new HttpClient())
            {
                await Task.Run(() =>
                {
                    WriteLine("Called {0}.{1}.{2}()", thisNamespace, thisClassName, thisMethodName);
                });

                // If the test wants to inject a custom status code, return that instead of the real value.
                if (WebApiConfig.StatusCodeToReturn["GET"].HasValue)
                {
                    return ResponseMessage(Request.CreateResponse(WebApiConfig.StatusCodeToReturn["GET"].Value));
                }

                WebUtils.ConfigureHttpClient(http, Request, WebApiConfig.AccessControl);

                var uri = WebUtils.CreateUri(Request.RequestUri, WebApiConfig.AccessControl, WebApiConfig.SVC_PATH);

                await Task.Run(() =>
                {
                    WriteLine("Calling http.GetAsync()");
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
