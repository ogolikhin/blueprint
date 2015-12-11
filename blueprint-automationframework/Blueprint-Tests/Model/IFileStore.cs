using System;
using System.Collections.Generic;
using System.Net;
using RestSharp;

namespace Model
{
    public interface IFileStore
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="file"></param>
        /// <param name="useMultiPartMime"></param>
        /// <param name="chunkSize"></param>
        /// <param name="expireTime"></param>
        /// <param name="expectedStatusCodes"></param>
        /// <param name="user"></param>
        /// <returns></returns>
        IFile AddFile(IFile file, IUser user, DateTime? expireTime = null, bool useMultiPartMime = false,
            int chunkSize = 0, List<HttpStatusCode> expectedStatusCodes = null);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="fileId"></param>
        /// <param name="user"></param>
        /// <param name="expireTime"></param>
        /// <param name="expectedStatusCode"></param>
        void DeleteFile(string fileId, IUser user, DateTime? expireTime = null, HttpStatusCode? expectedStatusCode = null);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="fileId"></param>
        /// <param name="user"></param>
        /// <param name="expectedStatusCode"></param>
        /// <returns></returns>
        IFile GetFile(string fileId, IUser user, HttpStatusCode? expectedStatusCode = null);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="fileId"></param>
        /// <param name="user"></param>
        /// <param name="expectedStatusCode"></param>
        /// <returns></returns>
        IFileMetadata GetFileMetadata(string fileId, IUser user, HttpStatusCode? expectedStatusCode = null);

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")] // Ignore this warning.
        HttpStatusCode GetStatus();
    }
}
