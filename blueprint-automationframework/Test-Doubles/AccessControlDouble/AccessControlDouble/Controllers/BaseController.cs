using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using Common;
using CommonUtilities;

namespace AccessControlDouble.Controllers
{
    public abstract class BaseController : ApiController
    {
        #region Protected functions

        /// <summary>
        /// Creates a copy of the request Uri that points to the real AccessControl.
        /// </summary>
        /// <returns>The new Uri.</returns>
        protected Uri CreateUri()
        {
            return WebUtils.CreateUri(Request.RequestUri, WebApiConfig.AccessControl, WebApiConfig.SVC_PATH);
        }

        /// <summary>
        /// Writes a line into the log file.
        /// </summary>
        /// <param name="line">The line to write.</param>
        protected static void WriteLine(string line)
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
        protected static void WriteLine(string format, params Object[] args)
        {
            using (LogFile logFile = new LogFile(WebApiConfig.LogFile))
            {
                logFile.WriteLine(format, args);
            }
        }

        /// <summary>
        /// Proxies a GET request to the real AccessControl and returns the response.
        /// </summary>
        /// <param name="thisClassName">The name of the class that called this function.</param>
        /// <param name="thisMethodName">The name of the function that called this function.</param>
        /// <param name="args">(optional) A list of arguments passed to the REST call.</param>
        /// <returns>The HTTP response returned by AccessControl.</returns>
        [HttpGet]
        [ResponseType(typeof(HttpResponseMessage))]
        protected async Task<IHttpActionResult> ProxyGetRequest(string thisClassName, string thisMethodName, List<string> args = null)
        {
            string thisNamespace = nameof(AccessControlDouble);

            using (HttpClient http = new HttpClient())
            {
                await Task.Run(() =>
                {
                    string allArgs = string.Empty;

                    if ((args != null) && (args.Count > 0))
                    {
                        allArgs = string.Join(", ", args);
                    }

                    WriteLine("Called {0}.{1}.{2}({3})", thisNamespace, thisClassName, thisMethodName, allArgs);
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
                    WriteLine("Proxying request via: http.GetAsync(\"{0}\")", uri.ToString());
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
        /// Proxies a POST request to the real AccessControl and returns the response.
        /// </summary>
        /// <param name="thisClassName">The name of the class that called this function.</param>
        /// <param name="thisMethodName">The name of the function that called this function.</param>
        /// <param name="value">The value to pass to the HttpClient.PostAsJsonAsync() function.</param>
        /// <param name="args">(optional) A list of arguments passed to the REST call.</param>
        /// <returns>The HTTP response returned by AccessControl.</returns>
        [HttpPost]
        [ResponseType(typeof(HttpResponseMessage))]
        protected async Task<IHttpActionResult> ProxyPostRequest<T>(string thisClassName, string thisMethodName, T value, List<string> args = null)
        {
            string thisNamespace = nameof(AccessControlDouble);

            using (HttpClient http = new HttpClient())
            {
                await Task.Run(() =>
                {
                    string allArgs = string.Empty;

                    if ((args != null) && (args.Count > 0))
                    {
                        allArgs = string.Join(", ", args);
                    }

                    WriteLine("Called {0}.{1}.{2}({3})", thisNamespace, thisClassName, thisMethodName, allArgs);
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
                    WriteLine("Proxying request via: http.PostAsJsonAsync(\"{0}\")", uri.ToString());
                });

                var result = await http.PostAsJsonAsync(uri, value);

                await Task.Run(() =>
                {
                    WebUtils.LogRestResponse(WebApiConfig.LogFile, result);
                });

                return ResponseMessage(result);
            }
        }

        /// <summary>
        /// Proxies a PUT request to the real AccessControl and returns the response.
        /// </summary>
        /// <param name="thisClassName">The name of the class that called this function.</param>
        /// <param name="thisMethodName">The name of the function that called this function.</param>
        /// <param name="args">(optional) A list of arguments passed to the REST call.</param>
        /// <returns>The HTTP response returned by AccessControl.</returns>
        [HttpPut]
        [ResponseType(typeof(HttpResponseMessage))]
        protected async Task<IHttpActionResult> ProxyPutRequest(string thisClassName, string thisMethodName, List<string> args = null)
        {
            string thisNamespace = nameof(AccessControlDouble);

            using (HttpClient http = new HttpClient())
            {
                await Task.Run(() =>
                {
                    string allArgs = string.Empty;

                    if ((args != null) && (args.Count > 0))
                    {
                        allArgs = string.Join(", ", args);
                    }

                    WriteLine("Called {0}.{1}.{2}({3})", thisNamespace, thisClassName, thisMethodName, allArgs);
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
                    WriteLine("Proxying request via: http.PutAsync(\"{0}\")", uri.ToString());
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
        /// Proxies a DELETE request to the real AccessControl and returns the response.
        /// </summary>
        /// <param name="thisClassName">The name of the class that called this function.</param>
        /// <param name="thisMethodName">The name of the function that called this function.</param>
        /// <param name="args">(optional) A list of arguments passed to the REST call.</param>
        /// <returns>The HTTP response returned by AccessControl.</returns>
        [HttpDelete]
        [ResponseType(typeof(HttpResponseMessage))]
        protected async Task<IHttpActionResult> ProxyDeleteRequest(string thisClassName, string thisMethodName, List<string> args = null)
        {
            string thisNamespace = nameof(AccessControlDouble);

            using (HttpClient http = new HttpClient())
            {
                await Task.Run(() =>
                {
                    string allArgs = string.Empty;

                    if ((args != null) && (args.Count > 0))
                    {
                        allArgs = string.Join(", ", args);
                    }

                    WriteLine("Called {0}.{1}.{2}({3})", thisNamespace, thisClassName, thisMethodName, allArgs);
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
                    WriteLine("Proxying request via: http.DeleteAsync()");
                });

                var result = await http.DeleteAsync(uri);

                await Task.Run(() =>
                {
                    WebUtils.LogRestResponse(WebApiConfig.LogFile, result);
                });

                return ResponseMessage(result);
            }
        }

        #endregion Protected functions

    }
}
