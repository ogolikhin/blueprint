using System;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using CommonUtilities;

namespace AccessControlDouble.Controllers
{
    [RoutePrefix("sessions")]
    public class SessionsController : ApiController
    {
        private const string SVC_PATH = "/svc/accesscontrol/";
        private const string TOKEN_HEADER = "Session-Token";

        #region Private functions

        /// <summary>
        /// If present, copies the Session-Token from the source headers to dest.
        /// </summary>
        /// <param name="source">The HTTP headers to copy from.</param>
        /// <param name="dest">The HTTP headers to copy to.</param>
        private static void CopySessionTokenHeader(HttpRequestHeaders source, HttpRequestHeaders dest)
        {
            if (source.Contains(TOKEN_HEADER))
            {
                var tokens = source.GetValues(TOKEN_HEADER);
                dest.Add(TOKEN_HEADER, tokens.First());
            }
        }

        /// <summary>
        /// Configures the specified HttpClient with the right BaseAddress and copies the Headers into it.
        /// </summary>
        /// <param name="http">The HttpClient to be configured.</param>
        private void ConfigureHttpClient(HttpClient http)
        {
            http.BaseAddress = new Uri(WebApiConfig.AccessControl);
            WebUtils.CopyHttpRequestHeaders(Request.Headers, http.DefaultRequestHeaders);
            CopySessionTokenHeader(Request.Headers, http.DefaultRequestHeaders);
        }

        /// <summary>
        /// Creates a copy of the request Uri that points to the real AccessControl.
        /// </summary>
        /// <returns>The new Uri.</returns>
        private Uri CreateUri()
        {
            string path = Request.RequestUri.LocalPath.Replace(SVC_PATH, string.Empty);
            Uri uri = new Uri(string.Format("{0}/{1}/{2}", WebApiConfig.AccessControl, path.TrimEnd('/'), Request.RequestUri.Query).TrimEnd('/'));

            return uri;
        }

        #endregion Private functions

        /// <summary>
        /// Method to query if session exists, expect to receive session token in header Session-Token to identify user session.
        /// Method will not extend lifetime of the session by SESSION_TIMEOUT.
        /// This method to be used for sign in sequence only.  Session information is returned back.
        /// </summary>
        /// <param name="uid">The ID of the User whose session you are retrieving.</param>
        /// <returns>The session token for the specified user.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "uid")]    // It's used implicitly by the path variable.
        [HttpGet]
        [Route("{uid}")]
        [ResponseType(typeof(HttpResponseMessage))]
        public async Task<IHttpActionResult> Get(int uid)
        {
            using (HttpClient http = new HttpClient())
            {
                ConfigureHttpClient(http);
                var uri = CreateUri();
                var result = await http.GetAsync(uri);

                return ResponseMessage(result);
            }
        }

        /// <summary>
        /// Gets a paged list of existing sessions.
        /// Method expects to receive session token in header Session-Token to identify admin user session.
        /// </summary>
        /// <param name="ps">(optional) Page Size.  The size of each page to return.</param>
        /// <param name="pn">(optional) Page Number.  Max number of pages to return.</param>
        /// <returns>A paged list of existing sessions.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters")] // They're used implicitly by the path variable.
        [HttpGet]
        [Route("select")]
        [ResponseType(typeof(HttpResponseMessage))]
        public async Task<IHttpActionResult> Get(string ps = "100", string pn = "1")
        {
            using (HttpClient http = new HttpClient())
            {
                ConfigureHttpClient(http);
                var uri = CreateUri();
                var result = await http.GetAsync(uri);

                return ResponseMessage(result);
            }
        }

        /// <summary>
        /// Method for initiating user session.  Session Token is returned thru Session-Token header as the string containing 32 alphanumerical characters of unique identifier (GUID).
        /// </summary>
        /// <param name="uid">Parameter to identify user for whom session needs to be created.</param>
        /// <param name="userName">The username for the user session.</param>
        /// <param name="licenseLevel"></param>
        /// <param name="isSso"></param>
        /// <returns>The session token.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters")]  // They're used implicitly by the path variable.
        [HttpPost]
        [Route("{uid}")]
        [ResponseType(typeof(HttpResponseMessage))]
        public async Task<IHttpActionResult> Post(int uid, string userName, int licenseLevel, bool isSso = false)
        {
            using (HttpClient http = new HttpClient())
            {
                ConfigureHttpClient(http);
                var uri = CreateUri();
                var result = await http.PostAsJsonAsync(uri, uid);

                return ResponseMessage(result);
            }
        }

        /// <summary>
        /// Method will extend lifetime of the session by SESSION_TIMEOUT.  This method to be used by all service methods to authorize user session.
        /// Method expects to receive session token in header Session-Token to identify user session.
        /// Session token value is returned back via Session-Token header.
        /// </summary>
        /// <param name="op">Optional parameter to identify operation user intends to perform.</param>
        /// <param name="aid">Optional parameter to identify artifact operation is requested to be performed on.</param>
        /// <returns>The session token.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters")]    // They're used implicitly by the path variable.
        [HttpPut]
        [Route("{op}/{aid}")]
        [ResponseType(typeof(HttpResponseMessage))]
        public async Task<IHttpActionResult> Put(string op, int aid)
        {
            using (HttpClient http = new HttpClient())
            {
                ConfigureHttpClient(http);
                var uri = CreateUri();
                var result = await http.PutAsync(uri, Request.Content);

                return ResponseMessage(result);
            }
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
            using (HttpClient http = new HttpClient())
            {
                ConfigureHttpClient(http);
                var uri = CreateUri();
                var result = await http.DeleteAsync(uri);

                return ResponseMessage(result);
            }
        }
    }
}
