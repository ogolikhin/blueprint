using System;
using System.Collections.Generic;
using System.Net;
using Logging;
using Model.Facades;
using RestSharp;
using RestSharp.Authenticators;

namespace Model.Impl
{
    public class FileStore : IFileStore
    {
        private const string SVC_PATH = "svc/filestore";

        private string _address = null;
        private List<IFileMetadata> _Files = new List<IFileMetadata>();

        #region Inherited from IFileStore

        public List<IFileMetadata> Files { get { return _Files; } }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="address">The URI address of the FileStore.</param>
        public FileStore(string address)
        {
            if (address == null) { throw new ArgumentNullException("address"); }

            _address = address;
        }

        /// <summary>
        /// Adds the specified file to the FileStore.
        /// </summary>
        /// <param name="file">The file to add.</param>
        /// <param name="user">The user to authenticate to the FileStore.</param>
        /// <param name="expectedStatusCodes">A list of expected status codes.  By default, only '200 OK' is expected.</param>
        /// <returns>The file that was added (including the file ID that FileStore gave it).</returns>
        /// <exception cref="WebException">A WebException sub-class if FileStore returned an unexpected HTTP status code.</exception>
        public IFile AddFile(IFile file, IUser user, List<HttpStatusCode> expectedStatusCodes = null)
        {
            if (file == null) { throw new ArgumentNullException("file"); }
            if (user == null) { throw new ArgumentNullException("user"); }

            var client = new RestClient(new Uri(_address));
            client.Authenticator = new HttpBasicAuthenticator(user.Username, user.Password);

            string path = string.Format("{0}/files", SVC_PATH);
            var request = new RestRequest(path, Method.POST);
            request.AddFile(file.FileName, file.Content, file.FileName);

            IRestResponse response = client.Execute(request);

            if (expectedStatusCodes == null) { expectedStatusCodes = new List<HttpStatusCode>() { HttpStatusCode.OK }; }

            if (!expectedStatusCodes.Contains(response.StatusCode))
            {
                throw WebExceptionFactory.Create(response.StatusCode.ToString());
            }

            string fileGuid = response.Content;

            Logger.WriteDebug(string.Format("POST {0} returned: {1}", path, fileGuid));
            file.Id = fileGuid;
            _Files.Add(file);

            return file;
        }

        public void DeleteFile(Guid id)
        {
            throw new NotImplementedException();
        }

        public void DeleteFile(IFile file)
        {
            throw new NotImplementedException();
        }

        public IFile GetFile(string id)
        {
            throw new NotImplementedException();
        }

        public IFile GetFile(IFile file)
        {
            throw new NotImplementedException();
        }

        public IFileMetadata GetFileMetadata(Guid id)
        {
            throw new NotImplementedException();
        }

        public short GetStatus()
        {
            throw new NotImplementedException();
        }

        #endregion Inherited from IFileStore
    }
}
