using System;
using System.Collections.Generic;
using System.Net;

namespace Model
{
    public interface ISvcComponents : IDisposable
    {
        /// <summary>
        /// Upload a File.
        /// </summary>
        /// <param name="user">The user credentials for the request to upload the file.</param>
        /// <param name="file">The file to upload.</param>
        /// <param name="expireDate">(optional) Expected expire date for the file.</param>
        /// <param name="expectedStatusCodes">(optional) Expected status codes for the request.</param>
        /// <returns>The REST response content of the upload file request.</returns>
        string UploadFile(IUser user, IFile file, DateTime? expireDate = null, List<HttpStatusCode> expectedStatusCodes = null);
    }
}
