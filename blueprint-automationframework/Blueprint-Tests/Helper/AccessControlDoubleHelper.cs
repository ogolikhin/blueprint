using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using Common;
using CustomAttributes;
using TestConfig;
using Utilities.Facades;

namespace Helper
{
    public class AccessControlDoubleHelper : IDisposable
    {
        private const string SvcPath = "svc/accesscontrol";

        private static TestConfiguration _testConfig = TestConfiguration.GetInstance();

        private string _address;
        private ISet<RestRequestMethod> _injectedErrorMethods = new HashSet<RestRequestMethod>();

        /// <summary>
        /// Creates an AccessControlDoubleHelper object with the settings defined in the TestConfiguration.
        /// </summary>
        /// <returns>The AccessControlDoubleHelper object.</returns>
        /// <exception cref="DataException">If there was an error reading required information from the TestConfiguration.</exception>
        public static AccessControlDoubleHelper GetAccessControlDoubleFromTestConfig()
        {
            if (!_testConfig.Services.ContainsKey(Categories.AccessControl))
            {
                string msg = I18NHelper.FormatInvariant("No <Service> tag named '{0}' was found in the TestConfiguration.xml file!  Please update it.", Categories.AccessControl);
                Logger.WriteError(msg);
                throw new DataException(msg);
            }

            return new AccessControlDoubleHelper(_testConfig.Services[Categories.AccessControl].Address);
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="address">The address of the AccessControlDouble.</param>
        public AccessControlDoubleHelper(string address)
        {
            _address = address;
        }

        /// <summary>
        /// Destructor.
        /// </summary>
        ~AccessControlDoubleHelper()
        {
            Dispose(false);
        }

        #region IDisposable members

        private bool _isDisposed = false;

        /// <summary>
        /// Disposes this object explicitly.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Disposes all the members of this object and stops injecting errors into AccessControl.
        /// </summary>
        /// <param name="disposing">Pass true if explicitly disposing or false if called in the destructor.</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")] // We don't want to throw exceptions from Dispose()
        protected virtual void Dispose(bool disposing)
        {
            if (_isDisposed)
            {
                return;
            }

            if (disposing)
            {
                if (_injectedErrorMethods != null)
                {
                    foreach (var method in _injectedErrorMethods.ToArray())
                    {
                        try
                        {
                            StopInjectingErrors(method);
                        }
                        catch (Exception e)
                        {
                            // Don't allow exceptions to be thrown from the Dispose() method.
                            Logger.WriteError("AccessControlDoubleHelper.Dispose() caught an exception while attempting to unset errors for method: {0}!  {1}",
                                method.ToString(), e.Message);
                        }
                    }
                }
            }

            _injectedErrorMethods = null;

            _isDisposed = true;
        }

        #endregion IDisposable members

        /// <summary>
        /// Starts injecting errors for the specified request method that will be returned from the AccessControlDouble.
        /// This means the AccessControlDouble will immediately return the specified status code instead of contacting the real AccessControl.
        /// </summary>
        /// <param name="method">The request method to inject errors into (ex. GET, POST, DELETE...).</param>
        /// <param name="statusCode">The status code to be injected.</param>
        public void StartInjectingErrors(RestRequestMethod method, HttpStatusCode statusCode)
        {
            var restApi = new RestApiFacade(_address);
            string path = I18NHelper.FormatInvariant("{0}/InjectErrors/{1}/{2}", SvcPath, method.ToString(), (int)statusCode);

            Logger.WriteInfo("Injecting error into AccessControl {0}...", method.ToString());
            restApi.SendRequestAndGetResponse(path, RestRequestMethod.POST);
            _injectedErrorMethods.Add(method);
        }

        /// <summary>
        /// Stops injecting errors for the specified request method and returns to acting as a proxy for the real AccessControl.
        /// </summary>
        /// <param name="method">The request method to stop injecting errors into (ex. GET, POST, DELETE...).</param>
        public void StopInjectingErrors(RestRequestMethod method)
        {
            var restApi = new RestApiFacade(_address);
            string path = I18NHelper.FormatInvariant("{0}/InjectErrors/{1}", SvcPath, method.ToString());

            Logger.WriteInfo("Removing injected error from AccessControl {0}...", method.ToString());
            restApi.SendRequestAndGetResponse(path, RestRequestMethod.DELETE);
            _injectedErrorMethods.Remove(method);
        }
    }
}
