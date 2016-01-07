using System;
using System.Configuration;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
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
            HttpClient http = new HttpClient();
            string accessControl = ConfigurationManager.AppSettings["AccessControl"];
            http.BaseAddress = new Uri(accessControl);
            WebUtils.CopyHttpRequestHeaders(Request.Headers, http.DefaultRequestHeaders);

            string path = Request.RequestUri.LocalPath.Replace("/svc/accesscontrol/", string.Empty);
            Uri uri = new Uri(accessControl + path);

            var result = await http.GetAsync(uri);

            return ResponseMessage(result);
        }
    }
}
