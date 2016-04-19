using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using Common;
using CommonUtilities;

namespace AccessControlDouble.Controllers
{
    [RoutePrefix("sessions")]
    public class SessionsController : ApiController
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
        /// Writes a line into the log file.
        /// </summary>
        /// <param name="line">The line to write.</param>
        private static void WriteLine(string line)
        {
            using (LogFile logFile = new LogFile(WebApiConfig.LogFile))
            {
                logFile.WriteLine(line);
            }
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
        /// <param name="uid">The ID of the User whose session you are retrieving.</param>
        /// <returns>The session token for the specified user.</returns>
        [HttpGet]
        [Route("{uid}")]
        [ResponseType(typeof(HttpResponseMessage))]
        public async Task<IHttpActionResult> Get(int uid)
        {
            string thisNamespace = nameof(AccessControlDouble);
            string thisClassName = nameof(SessionsController);
            string thisMethodName = nameof(Get);

            using (HttpClient http = new HttpClient())
            {
                await Task.Run(() =>
                {
                    WriteLine("Called {0}.{1}.{2}({3})", thisNamespace, thisClassName, thisMethodName, uid);
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
                    WriteLine("Calling http.GetAsync({0})", uid);
                });
                var result = await http.GetAsync(uri);
                await Task.Run(() =>
                {
                    WebUtils.LogRestResponse(WebApiConfig.LogFile, result);
                });

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
        [HttpGet]
        [Route("select")]
        [ResponseType(typeof(HttpResponseMessage))]
        public async Task<IHttpActionResult> Get(string ps = "100", string pn = "1")
        {
            string thisNamespace = nameof(AccessControlDouble);
            string thisClassName = nameof(SessionsController);
            string thisMethodName = nameof(Get);

            using (HttpClient http = new HttpClient())
            {
                await Task.Run(() =>
                {
                    WriteLine("Called {0}.{1}.{2}('{3}', '{4}')", thisNamespace, thisClassName, thisMethodName, ps, pn);
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
                    WriteLine("Calling http.GetAsync('{0}', '{1}')", ps, pn);
                });
                var result = await http.GetAsync(uri);
                await Task.Run(() =>
                {
                    WebUtils.LogRestResponse(WebApiConfig.LogFile, result);
                });

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
        [HttpPost]
        [Route("{uid}")]
        [ResponseType(typeof(HttpResponseMessage))]
        public async Task<IHttpActionResult> Post(int uid, string userName, int licenseLevel, bool isSso = false)
        {
            string thisNamespace = nameof(AccessControlDouble);
            string thisClassName = nameof(SessionsController);
            string thisMethodName = nameof(Post);

            using (HttpClient http = new HttpClient())
            {
                await Task.Run(() =>
                {
                    WriteLine("Called {0}.{1}.{2}({3}, '{4}', {5}, {6})",
                        thisNamespace, thisClassName, thisMethodName, uid, userName, licenseLevel, isSso);
                });

                // If the test wants to inject a custom status code, return that instead of the real value.
                if (WebApiConfig.StatusCodeToReturn["POST"].HasValue)
                {
                    return ResponseMessage(Request.CreateResponse(WebApiConfig.StatusCodeToReturn["POST"].Value));
                }

                WebUtils.ConfigureHttpClient(http, Request, WebApiConfig.AccessControl);
                var uri = CreateUri();

                await Task.Run(() =>
                {
                    WriteLine("Calling http.PostAsJsonAsync('{0}', {1})", uri.ToString(), uid);
                });
                var result = await http.PostAsJsonAsync(uri, uid);
                await Task.Run(() =>
                {
                    WebUtils.LogRestResponse(WebApiConfig.LogFile, result);
                });

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
        [HttpPut]
        [Route("")]
        [ResponseType(typeof(HttpResponseMessage))]
        public async Task<IHttpActionResult> Put(string op = "", int aid = 0)
        {
            string thisNamespace = nameof(AccessControlDouble);
            string thisClassName = nameof(SessionsController);
            string thisMethodName = nameof(Put);

            using (HttpClient http = new HttpClient())
            {
                await Task.Run(() =>
                {
                    WriteLine("Called {0}.{1}.{2}('{3}', {4})", thisNamespace, thisClassName, thisMethodName, op, aid);
                });

                // If the test wants to inject a custom status code, return that instead of the real value.
                if (WebApiConfig.StatusCodeToReturn["PUT"].HasValue)
                {
                    return ResponseMessage(Request.CreateResponse(WebApiConfig.StatusCodeToReturn["PUT"].Value));
                }

                WebUtils.ConfigureHttpClient(http, Request, WebApiConfig.AccessControl);
                var uri = CreateUri();

                await Task.Run(() =>
                {
                    WriteLine("Calling http.PutAsync('{0}', {1})", op, aid);
                });
                var result = await http.PutAsync(uri, Request.Content);
                await Task.Run(() =>
                {
                    WebUtils.LogRestResponse(WebApiConfig.LogFile, result);
                });

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
            string thisNamespace = nameof(AccessControlDouble);
            string thisClassName = nameof(SessionsController);
            string thisMethodName = nameof(Delete);

            using (HttpClient http = new HttpClient())
            {
                await Task.Run(() =>
                {
                    WriteLine("Called {0}.{1}.{2}()", thisNamespace, thisClassName, thisMethodName);
                });

                // If the test wants to inject a custom status code, return that instead of the real value.
                if (WebApiConfig.StatusCodeToReturn["DELETE"].HasValue)
                {
                    return ResponseMessage(Request.CreateResponse(WebApiConfig.StatusCodeToReturn["DELETE"].Value));
                }

                WebUtils.ConfigureHttpClient(http, Request, WebApiConfig.AccessControl);
                var uri = CreateUri();

                await Task.Run(() =>
                {
                    WriteLine("Calling http.DeleteAsync()");
                });
                var result = await http.DeleteAsync(uri);
                await Task.Run(() =>
                {
                    WebUtils.LogRestResponse(WebApiConfig.LogFile, result);
                });

                return ResponseMessage(result);
            }
        }
    }
}
