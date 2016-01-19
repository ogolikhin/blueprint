using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using Common;

namespace AccessControlDouble.Controllers
{
    [RoutePrefix("InjectErrors")]
    public class InjectErrorsController : ApiController
    {
        #region Private functions

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

        // POST: /svc/accesscontrol/InjectErrors/{requestType}/{httpStatusCode}
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        [HttpPost]
        [Route("{requestType}/{httpStatusCode}")]
        [ResponseType(typeof(HttpResponseMessage))]
        public async Task<IHttpActionResult> Post(string requestType, string httpStatusCode)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(requestType))
                {
                    throw new ArgumentNullException(nameof(requestType));
                }

                if (string.IsNullOrWhiteSpace(httpStatusCode))
                {
                    throw new ArgumentNullException(nameof(httpStatusCode));
                }

                if (!WebApiConfig.StatusCodeToReturn.Keys.Contains(requestType))
                {
                    string msg = I18NHelper.FormatInvariant("The requestType must be one of the following: {0}",
                        WebApiConfig.StatusCodeToReturn.Keys.ToString());
                    throw new ArgumentException(msg);
                }

                HttpStatusCode statusCode;

                if (HttpStatusCode.TryParse(httpStatusCode, out statusCode))
                {
                    await Task.Run(() =>
                    {
                        WriteLine("Setting StatusCodeToReturn to:  {0}", statusCode);
                    });

                    WebApiConfig.StatusCodeToReturn[requestType] = statusCode;
                    var response = Request.CreateResponse(HttpStatusCode.OK);
                    return ResponseMessage(response);
                }
                else
                {
                    string msg = I18NHelper.FormatInvariant("Cannot parse '{0}' to a valid HttpStatusCode!", httpStatusCode);
                    var response = Request.CreateErrorResponse(HttpStatusCode.BadRequest, msg);
                    return ResponseMessage(response);
                }
            }
            catch (Exception e)
            {
                await Task.Run(() =>
                {
                    WriteLine("Caught an exception!  {0}", e.Message);
                });

                var response = Request.CreateErrorResponse(HttpStatusCode.InternalServerError, e);
                return ResponseMessage(response);
            }
        }

        // DELETE: /svc/accesscontrol/InjectErrors/{requestType}
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        [HttpDelete]
        [Route("{requestType}")]
        [ResponseType(typeof(HttpResponseMessage))]
        public async Task<IHttpActionResult> Delete(string requestType)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(requestType))
                {
                    throw new ArgumentNullException(nameof(requestType));
                }

                await Task.Run(() =>
                {
                    WriteLine("Resetting StatusCodeToReturn to default");
                });

                WebApiConfig.StatusCodeToReturn[requestType] = null;
                var response = Request.CreateResponse(HttpStatusCode.OK);
                return ResponseMessage(response);
            }
            catch (Exception e)
            {
                await Task.Run(() =>
                {
                    WriteLine("Caught an exception!  {0}", e.Message);
                });

                var response = Request.CreateErrorResponse(HttpStatusCode.InternalServerError, e);
                return ResponseMessage(response);
            }
        }

/*
        // GET: /svc/accesscontrol/InjectErrors
        [HttpGet]
        [Route("")]
        [ResponseType(typeof(HttpResponseMessage))]
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET: /svc/accesscontrol/InjectErrors/5
        [HttpGet]
        [Route("{uid}")]
        [ResponseType(typeof(HttpResponseMessage))]
        public string Get(int id)
        {
            return "value";
        }

        // PUT: /svc/accesscontrol/InjectErrors/5
        [HttpPut]
        [Route("")]
        [ResponseType(typeof(HttpResponseMessage))]
        public void Put(int id, [FromBody]string value)
        {
        }
*/
    }
}
