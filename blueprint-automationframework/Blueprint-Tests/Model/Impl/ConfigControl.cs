using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mime;
using Common;
using Utilities;
using Utilities.Facades;

namespace Model.Impl
{
    public class ConfigControl : NovaServiceBase, IConfigControl
    {
        private const string SVC_PATH = "/svc/configcontrol";

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="address">The base URL of the ConfigControl service.</param>
        public ConfigControl(string address)
        {
            ThrowIf.ArgumentNull(address, nameof(address));

            Address = address;
        }

        #region Inherited from IConfigControl

        /// <see cref="IConfigControl.GetLog(List{HttpStatusCode})"/>
        public IFile GetLog(List<HttpStatusCode> expectedStatusCodes = null)
        {
            var restApi = new RestApiFacade(Address, token: string.Empty);
            var path = I18NHelper.FormatInvariant("{0}/log/getlog", SVC_PATH);

            var response = restApi.SendRequestAndGetResponse(
                path,
                RestRequestMethod.GET,
                expectedStatusCodes: expectedStatusCodes);

            File file = null;

            if (response.StatusCode == HttpStatusCode.OK)
            {
                file = new File
                {
                    Content = response.RawBytes.ToArray(),
                    FileType = response.ContentType,
                    FileName =
                        new ContentDisposition(
                            response.Headers.First(h => h.Key == "Content-Disposition").Value.ToString()).FileName
                };
            }

            return file;
        }

        /// <see cref="IConfigControl.GetStatus(List{HttpStatusCode})"/>
        public string GetStatus(List<HttpStatusCode> expectedStatusCodes = null)    // GET /status
        {
            return GetStatus(SVC_PATH, preAuthorizedKey: null, expectedStatusCodes: expectedStatusCodes);
        }

        /// <seealso cref="IConfigControl.GetStatusUpcheck"/>
        public HttpStatusCode GetStatusUpcheck(List<HttpStatusCode> expectedStatusCodes = null)
        {
            return GetStatusUpcheck(SVC_PATH, expectedStatusCodes);
        }

        #endregion Inherited from IConfigControl

        #region Inherited from IDisposable

        private bool _isDisposed = false;

        /// <summary>
        /// Disposes this object by releasing any resources that were created.
        /// </summary>
        /// <param name="disposing">Pass true if explicitly disposing or false if called from the destructor.</param>
        protected virtual void Dispose(bool disposing)
        {
            Logger.WriteTrace("{0}.{1} called.", nameof(ConfigControl), nameof(Dispose));

            if (_isDisposed)
            {
                return;
            }

            // TODO: If ConfigControl starts using resources that need to be released, remember to update this Dispose() to release them.

            _isDisposed = true;
        }

        /// <summary>
        /// Disposes this object by releasing any resources that were created.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion Inherited from IDisposable
    }
}
