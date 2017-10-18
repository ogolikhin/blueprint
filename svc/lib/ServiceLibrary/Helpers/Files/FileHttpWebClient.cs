using System;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;

namespace ServiceLibrary.Helpers.Files
{
    public class FileHttpWebClient : HttpWebClient
    {
        public FileHttpWebClient
        (
            Uri baseUri,
            string sessionToken,
            int timeout = ServiceConstants.DefaultRequestTimeout) : base(baseUri, sessionToken, timeout)
        {
            // Making calls to FileStore with SSL (HTTPS) requires certificate validation.
            ServicePointManager.ServerCertificateValidationCallback = AcceptAllCertifications;
        }

        private static bool AcceptAllCertifications(object sender, X509Certificate certification, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            // This will make it accept all certificates. It's safe because FileStore is an internal service.
            // If it becomes public we must have a valid trusted certificate installed on the server.
            return true;
        }
    }
}
