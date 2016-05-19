using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;

namespace AccessControlDouble.Controllers
{
    [RoutePrefix("sessions")]
    public class SessionsController : BaseController
    {
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
            string thisClassName = nameof(SessionsController);
            string thisMethodName = nameof(Get);

            List<string> args = new List<string> { uid.ToString() };

            return await ProxyGetRequest(thisClassName, thisMethodName, args);
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
            string thisClassName = nameof(SessionsController);
            string thisMethodName = nameof(Get);

            List<string> args = null;

            if (ps != null)
            {
                args = new List<string> { ps };
            }

            if (pn != null)
            {
                if (args == null)
                {
                    args = new List<string> {pn};
                }
                else
                {
                    args.Add(pn);
                }
            }

            return await ProxyGetRequest(thisClassName, thisMethodName, args);
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
            string thisClassName = nameof(SessionsController);
            string thisMethodName = nameof(Post);

            var args = new List<string> { uid.ToString(), userName, licenseLevel.ToString(), isSso.ToString() };

            return await ProxyPostRequest(thisClassName, thisMethodName, uid, args);
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
            string thisClassName = nameof(SessionsController);
            string thisMethodName = nameof(Put);

            var args = new List<string> { op ?? "null", aid.ToString() };

            return await ProxyPutRequest(thisClassName, thisMethodName, args);
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
            string thisClassName = nameof(SessionsController);
            string thisMethodName = nameof(Delete);

            return await ProxyDeleteRequest(thisClassName, thisMethodName);
        }
    }
}
