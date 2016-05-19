using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;

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
            string thisClassName = nameof(StatusController);
            string thisMethodName = nameof(GetStatusUpcheck);

            return await ProxyGetRequest(thisClassName, thisMethodName);
        }

        /// <summary>
        /// Method to return current status of AccessControl Web Service.
        /// </summary>
        /// <returns>JSON object with service details.</returns>
        [HttpGet]
        [Route("")]
        [ResponseType(typeof(HttpResponseMessage))]
        public async Task<IHttpActionResult> GetStatus(string preAuthorizedKey = null)
        {
            string thisClassName = nameof(StatusController);
            string thisMethodName = nameof(GetStatus);

            List<string> args = null;

            if (preAuthorizedKey != null)
            {
                args = new List<string> { preAuthorizedKey };
            }

            return await ProxyGetRequest(thisClassName, thisMethodName, args);
        }
    }
}
