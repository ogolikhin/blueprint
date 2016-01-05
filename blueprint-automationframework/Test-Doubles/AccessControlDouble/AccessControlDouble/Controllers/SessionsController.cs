using System;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using CommonUtilities;

namespace AccessControlDouble.Controllers
{
    [RoutePrefix("sessions")]
    public class SessionsController : ApiController
    {
        private const string TOKEN_HEADER = "Session-Token";

        /// <summary>
        /// Method to query if session exists, expect to receive session token in header Session-Token to identify user session.
        /// Method will not extend lifetime of the session by SESSION_TIMEOUT.
        /// This method to be used for sign in sequence only.  Session information is returned back.
        /// </summary>
        /// <param name="uid">The ID of the User whose session you are retrieving.</param>
        /// <returns>The session token for the specified user.</returns>
        [HttpGet]
        [Route("{uid}")]
        [ResponseType(typeof(HttpResponseMessage))]
        public async Task<IHttpActionResult> Get(int uid)
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

        /// <summary>
        /// Gets a paged list of existing sessions.
        /// Method expects to receive session token in header Session-Token to identify admin user session.
        /// </summary>
        /// <param name="ps">(optional) Page Size.  The size of each page to return.</param>
        /// <param name="pn">(optional) Page Number.  Max number of pages to return.</param>
        /// <returns>A paged list of existing sessions.</returns>
        [HttpGet]
        [Route("select")]
        [ResponseType(typeof(HttpResponseMessage))]
        public async Task<IHttpActionResult> Get(string ps = "100", string pn = "1")
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

        /// <summary>
        /// Method for initiating user session.  Session Token is returned thru Session-Token header as the string containing 32 alphanumerical characters of unique identifier (GUID).
        /// </summary>
        /// <param name="uid">Parameter to identify user for whom session needs to be created.</param>
        /// <param name="userName">The username for the user session.</param>
        /// <param name="licenseLevel"></param>
        /// <param name="isSso"></param>
        /// <returns>The session token.</returns>
        [HttpPost]
        [Route("{uid}")]
        [ResponseType(typeof(HttpResponseMessage))]
        public async Task<IHttpActionResult> Post(int uid, string userName, int licenseLevel, bool isSso = false)
        {
            HttpClient http = new HttpClient();
            string accessControl = ConfigurationManager.AppSettings["AccessControl"];
            http.BaseAddress = new Uri(accessControl);
            WebUtils.CopyHttpRequestHeaders(Request.Headers, http.DefaultRequestHeaders);

            string path = Request.RequestUri.LocalPath.Replace("/svc/accesscontrol/", string.Empty);
            Uri uri = new Uri(accessControl + path + Request.RequestUri.Query);

            var result = await http.PostAsJsonAsync(uri, uid);

            return ResponseMessage(result);
        }

        /// <summary>
        /// Method will extend lifetime of the session by SESSION_TIMEOUT.  This method to be used by all service methods to authorize user session.
        /// Method expects to receive session token in header Session-Token to identify user session.
        /// Session token value is returned back via Session-Token header.
        /// </summary>
        /// <param name="op">Optional parameter to identify operation user intends to perform.</param>
        /// <param name="aid">Optional parameter to identify artifact operation is requested to be performed on.</param>
        /// <returns>The session token.</returns>
        [HttpPut]
        [Route("{op}/{aid}")]
        [ResponseType(typeof(HttpResponseMessage))]
        public async Task<IHttpActionResult> Put(string op, int aid)
        {
            HttpClient http = new HttpClient();
            string accessControl = ConfigurationManager.AppSettings["AccessControl"];
            http.BaseAddress = new Uri(accessControl);
            WebUtils.CopyHttpRequestHeaders(Request.Headers, http.DefaultRequestHeaders);

            if (Request.Headers.Contains(TOKEN_HEADER))
            {
                var tokens = Request.Headers.GetValues(TOKEN_HEADER);
                http.DefaultRequestHeaders.Add(TOKEN_HEADER, tokens.First());
            }

            string path = Request.RequestUri.LocalPath.Replace("/svc/accesscontrol/", string.Empty);
            Uri uri = new Uri(accessControl + path + Request.RequestUri.Query);

            var result = await http.PutAsync(uri, Request.Content);

            return ResponseMessage(result);
        }

        /// <summary>
        /// Method for removing user session upon explicit sign out.  Method expects to receive session token in header Session-Token to identify user session.
        /// The session token is returned thru Session-Token header.
        /// </summary>
        /// <returns>The session token.</returns>
        [HttpDelete]
        [Route("")]
        [ResponseType(typeof(HttpResponseMessage))]
        public async Task<IHttpActionResult> Delete()
        {
            HttpClient http = new HttpClient();
            string accessControl = ConfigurationManager.AppSettings["AccessControl"];
            http.BaseAddress = new Uri(accessControl);
            WebUtils.CopyHttpRequestHeaders(Request.Headers, http.DefaultRequestHeaders);

            if (Request.Headers.Contains(TOKEN_HEADER))
            {
                var tokens = Request.Headers.GetValues(TOKEN_HEADER);
                http.DefaultRequestHeaders.Add(TOKEN_HEADER, tokens.First());
            }

            string path = Request.RequestUri.LocalPath.Replace("/svc/accesscontrol/", string.Empty);
            Uri uri = new Uri(accessControl + path);

            var result = await http.DeleteAsync("sessions");

            return ResponseMessage(result);
        }
    }
}
