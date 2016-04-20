using System;
using System.Web.Http;
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

        #endregion Protected functions
    }
}
