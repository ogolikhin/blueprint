using System.Net;

namespace ServiceLibrary.Helpers
{
    public static class HttpsSecurity
    {
        /// <summary>
        /// Please keep in sync with blueprint-current\Source\BluePrintSys.RC.Common\Helpers\HttpsSecurity.cs
        /// </summary>
        public static void Configure()
        {
            // Modern site/servers should avoid using old/insecure SSL3 and TLS 1.0 - that are only enabled for .NET 4.5 by default
            // We are keeping them for now but also enabling Tls 1.1/2 that should be used instead.
            ServicePointManager.SecurityProtocol |= SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
        }
    }
}
