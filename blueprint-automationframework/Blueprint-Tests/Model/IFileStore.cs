using System;
using System.Collections.Generic;
using System.Net;

namespace Model
{
    public interface IFileStore
    {
        List<IFileMetadata> Files { get; }


        /// <summary>
        /// Adds the specified file to the FileStore.
        /// </summary>
        /// <param name="file">The file to add.</param>
        /// <param name="user">The user to authenticate to the FileStore.</param>
        /// <param name="expectedStatusCodes">A list of expected status codes.  By default, only '200 OK' is expected.</param>
        /// <returns>The file that was added (including the file ID that FileStore gave it).</returns>
        /// <exception cref="WebException">A WebException sub-class if FileStore returned an unexpected HTTP status code.</exception>
        IFile AddFile(IFile file, IUser user, List<HttpStatusCode> expectedStatusCodes = null);

        void DeleteFile(Guid id);
        void DeleteFile(IFile file);

        IFile GetFile(string id);
        IFile GetFile(IFile file);

        IFileMetadata GetFileMetadata(Guid id);

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")] // Ignore this warning.
        short GetStatus();
    }
}
