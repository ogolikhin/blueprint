using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using Common;
using Model.ArtifactModel.Impl;
using Utilities;
using Utilities.Facades;

namespace Model.Impl
{
    public class SvcComponents : NovaServiceBase, ISvcComponents
    {
        #region Members inherited from IDisposable

        private bool _isDisposed = false;

        /// <summary>
        /// Disposes this object by deleting all objects that were created.
        /// </summary>
        /// <param name="disposing">Pass true if explicitly disposing or false if called from the destructor.</param>
        protected virtual void Dispose(bool disposing)
        {
            Logger.WriteTrace("{0}.{1} called.", nameof(SvcComponents), nameof(Dispose));

            if (_isDisposed)
            {
                return;
            }

            if (disposing)
            {
                // TODO: Delete anything created by this class.
            }

            _isDisposed = true;
        }

        /// <summary>
        /// Disposes this object by deleting all objects that were created.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion Members inherited from IDisposable

        #region Members inherited from ISvcComponents

        /// <seealso cref="ISvcComponents.UploadFile(IUser, IFile, DateTime?, List{HttpStatusCode})"/>
        public string UploadFile(IUser user, IFile file, DateTime? expireDate = null, List<HttpStatusCode> expectedStatusCodes = null)
        {
            return UploadFile(Address, user, file, expireDate, expectedStatusCodes);
        }

        /// <seealso cref="ISvcComponents.UploadFile(IUser, IFile, DateTime?, List{HttpStatusCode})"/>
        public static string UploadFile(string address, IUser user, IFile file, DateTime? expireDate = null, List<HttpStatusCode> expectedStatusCodes = null)
        {
            Logger.WriteTrace("{0}.{1}", nameof(SvcComponents), nameof(UploadFile));

            ThrowIf.ArgumentNull(user, nameof(user));
            ThrowIf.ArgumentNull(file, nameof(file));

            if (expectedStatusCodes == null)
            {
                expectedStatusCodes = new List<HttpStatusCode> { HttpStatusCode.Created };
            }

            var additionalHeaders = new Dictionary<string, string>();
            string path = I18NHelper.FormatInvariant(RestPaths.Svc.Components.FileStore.FILES_filename_, file.FileName);

            if (expireDate != null)
            {
                DateTime time = (DateTime)expireDate;
                path = I18NHelper.FormatInvariant("{0}/?expired={1}", path, time.ToUniversalTime().ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss'.'fff'Z'", CultureInfo.InvariantCulture));
            }

            byte[] bytes = file.Content.ToArray();
            var restApi = new RestApiFacade(address, user.Token?.AccessControlToken);

            Logger.WriteInfo("{0} Uploading a file named: {1}, size: {2}", nameof(SvcComponents), file.FileName, bytes.Length);

            var artifactResult = restApi.SendRequestAndGetResponse(
                path,
                RestRequestMethod.POST,
                fileName: file.FileName,
                fileContent: bytes,
                contentType: "application/json;charset=utf8",
                additionalHeaders: additionalHeaders,
                expectedStatusCodes: expectedStatusCodes);

            return artifactResult.Content;

            #endregion Members inherited from ISvcComponents
        }
    }
}
