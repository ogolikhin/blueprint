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

        /// <summary>
        /// This will turn on error injection for the specified request method.
        /// </summary>
        /// <param name="requestMethodType">The method you want to start returning errors for (i.e. DELETE, GET, HEAD, POST, PUT).</param>
        /// <param name="httpStatusCode">The status code that you want the method to start returning.</param>
        /// <returns>200 OK if the error was successfully injected, 400 if an invalid httpStatusCode was passed,
        /// 404 if an invalid Request Method was passed, or 500 for any other errors.</returns>
        /// <example>POST: /svc/accesscontrol/InjectErrors/{requestMethodType}/{httpStatusCode}</example>
        [HttpPost]
        [Route("{requestMethodType}/{httpStatusCode}")]
        [ResponseType(typeof(HttpResponseMessage))]
        public async Task<IHttpActionResult> StartInjectingErrors(string requestMethodType, string httpStatusCode)
        {
            string thisClassName = nameof(InjectErrorsController);
            string thisMethodName = nameof(StartInjectingErrors);

            try
            {
                if (string.IsNullOrWhiteSpace(requestMethodType))
                {
                    throw new ArgumentNullException(nameof(requestMethodType));
                }

                if (string.IsNullOrWhiteSpace(httpStatusCode))
                {
                    throw new ArgumentNullException(nameof(httpStatusCode));
                }

                if (!WebApiConfig.StatusCodeToReturn.Keys.Contains(requestMethodType))
                {
                    var keys = WebApiConfig.StatusCodeToReturn.Keys;
                    string msg = I18NHelper.FormatInvariant("The requestType must be one of the following: {0}",
                        string.Join(", ", keys));

                    throw new ArgumentException(msg);
                }

                HttpStatusCode statusCode;

                if (HttpStatusCode.TryParse(httpStatusCode, out statusCode))
                {
                    await Task.Run(() =>
                    {
                        WriteLine("Setting StatusCodeToReturn to:  {0}", statusCode);
                    });

                    WebApiConfig.StatusCodeToReturn[requestMethodType] = statusCode;
                    var response = Request.CreateResponse(HttpStatusCode.OK);

                    return ResponseMessage(response);
                }
                else
                {
                    string msg = I18NHelper.FormatInvariant("Cannot parse '{0}' to a valid HttpStatusCode!",
                        httpStatusCode);
                    await Task.Run(() =>
                    {
                        WriteLine(msg);
                    });
                    var response = Request.CreateErrorResponse(HttpStatusCode.BadRequest, msg);

                    return ResponseMessage(response);
                }
            }
            catch (ArgumentException e)
            {
                await Task.Run(() =>
                {
                    WriteLine("{0}.{1}() caught an exception!  {2}", thisClassName, thisMethodName, e.Message);
                });

                var response = Request.CreateErrorResponse(HttpStatusCode.NotFound, e);

                throw new HttpResponseException(response);
            }
            catch (Exception e)
            {
                await Task.Run(() =>
                {
                    WriteLine("{0}.{1}() caught an exception!  {2}", thisClassName, thisMethodName, e.Message);
                });

                var response = Request.CreateErrorResponse(HttpStatusCode.InternalServerError, e);

                throw new HttpResponseException(response);
            }
        }

        /// <summary>
        /// This will turn off error injection for the specified request method.
        /// </summary>
        /// <param name="requestMethodType">The method you want to start returning errors for (i.e. DELETE, GET, HEAD, POST, PUT).</param>
        /// <returns>200 OK if the error was successfully injected, 404 if an invalid Request Method was passed, or 500 for any other errors.</returns>
        /// <example>DELETE: /svc/accesscontrol/InjectErrors/{requestMethodType}</example>
        [HttpDelete]
        [Route("{requestMethodType}")]
        [ResponseType(typeof(HttpResponseMessage))]
        public async Task<IHttpActionResult> StopInjectingErrors(string requestMethodType)
        {
            string thisClassName = nameof(InjectErrorsController);
            string thisMethodName = nameof(StopInjectingErrors);

            try
            {
                if (string.IsNullOrWhiteSpace(requestMethodType))
                {
                    throw new ArgumentNullException(nameof(requestMethodType));
                }

                if (!WebApiConfig.StatusCodeToReturn.Keys.Contains(requestMethodType))
                {
                    var keys = WebApiConfig.StatusCodeToReturn.Keys;
                    string msg = I18NHelper.FormatInvariant("The requestType must be one of the following: {0}",
                        string.Join(", ", keys));

                    throw new ArgumentException(msg);
                }

                await Task.Run(() =>
                {
                    WriteLine("Resetting StatusCodeToReturn to default");
                });

                WebApiConfig.StatusCodeToReturn[requestMethodType] = null;
                var response = Request.CreateResponse(HttpStatusCode.OK);

                return ResponseMessage(response);
            }
            catch (ArgumentException e)
            {
                await Task.Run(() =>
                {
                    WriteLine("{0}.{1}() caught an exception!  {2}", thisClassName, thisMethodName, e.Message);
                });

                var response = Request.CreateErrorResponse(HttpStatusCode.NotFound, e);

                throw new HttpResponseException(response);
            }
            catch (Exception e)
            {
                await Task.Run(() =>
                {
                    WriteLine("{0}.{1}() caught an exception!  {2}", thisClassName, thisMethodName, e.Message);
                });

                var response = Request.CreateErrorResponse(HttpStatusCode.InternalServerError, e);

                throw new HttpResponseException(response);
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
