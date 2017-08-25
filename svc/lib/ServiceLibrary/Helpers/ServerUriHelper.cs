using System;
using System.Collections.Specialized;
using System.Web;
using ServiceLibrary.Models;

namespace ServiceLibrary.Helpers
{
    public static class ServerUriHelper
    {
        public const string KeyArtifactId = "ArtifactId";
        public const string KeyProjectId = "ProjectId";
        public const string KeyVersionId = "VersionId";
        public const string KeyBaselineId = "BaselineId";
        public const string KeyCommentId = "CommentId";
        public const string KeySubArtifactId = "SubArtifactId";
        public const string KeySharedViewId = "SharedViewId";
        public const string KeyAttachmentId = "AttachmentId";
        public const string KeyStorytellerDefaultDoc = "/index.html";
        public const string KeyStorytellerRouter = "#/main";

        private const string KeyValuePairSubsequentFormat = "${0}={1}";

        public static Uri BaseHostUri
        {
            get
            {
                var hostUri = HttpContext.Current.Request.Url;
                var builder = new UriBuilder();
                builder.Scheme = hostUri.Scheme;
                builder.Host = hostUri.Host;
                builder.Port = hostUri.Port;
                return builder.Uri;
            }
        }

        public static Uri GetBaseHostUri()
        {
            if (HttpContext.Current?.Request?.Url == null)
            {
                return null;
            }
            return BaseHostUri;
        }

        public static string GetArtifactUrl(int artifactId)
        {
            Uri hostUri = GetCurrentHostUri();
            if (hostUri == null)
            {
                return null;
            }
            string baseUrl = CreateUrlString(hostUri, "", null);
            return GetArtifactPartUrl(baseUrl, artifactId, null, null, null, null);
        }

        private static string CreateUrlString(Uri baseUri, string appendPath, string query)
        {
            var uri = CreateUrl(baseUri, appendPath, query);

            return uri.ToString();
        }

        private static Uri CreateUrl(Uri baseUri, string appendPath, string query, string hash = null)
        {
            var uriBuilder = new UriBuilder(baseUri);

            var applicationPath = string.IsNullOrEmpty(uriBuilder.Path) || string.Equals(uriBuilder.Path, @"/")
                                      ? string.Empty
                                      : uriBuilder.Path;

            uriBuilder.Path = string.Format(applicationPath.EndsWith(@"/") ? "{0}{1}" : "{0}/{1}", applicationPath, appendPath);
            if (query != null)
                uriBuilder.Query = query;

            if (hash != null)
                uriBuilder.Fragment = hash;

            return uriBuilder.Uri;
        }

        private static string GetArtifactPartUrl(string baseUrl, int artifactId, int? versionId, int? baselineId, int? sharedViewId, int? subArtifactId, ItemTypePredefined? baseItemType = null)
        {
            return GetArtifactUrl(baseUrl, artifactId, versionId, baselineId, sharedViewId, baseItemType)
                // Storyteller does not support URLs to sub-artifacts
                + (IsStoryTeller(baseUrl, baseItemType)
                    ? string.Empty
                : (subArtifactId.HasValue ? string.Format(KeyValuePairSubsequentFormat, KeySubArtifactId, subArtifactId.Value) : string.Empty));
        }

        private static string GetArtifactUrl(string baseUrl, int artifactId, int? versionId, int? baselineId, int? sharedViewId, ItemTypePredefined? baseItemType = null)
        {
            if (IsStoryTeller(baseUrl, baseItemType))
            {
                var nBaseUrl = baseUrl ?? String.Empty;
                var slashPosition = nBaseUrl.LastIndexOf("/", StringComparison.Ordinal);
                if (slashPosition > 8)
                {
                    nBaseUrl = nBaseUrl.Substring(0, slashPosition);
                }
                var versionFormat = "?version={0}";

                return string.Format
                    (
                        "{0}{1}{2}/{3}{4}",
                        nBaseUrl,
                        KeyStorytellerDefaultDoc,
                        KeyStorytellerRouter,
                        artifactId,
                        (versionId.HasValue ? string.Format(versionFormat, versionId.Value) : string.Empty)
                    );
            }

            return string.Format
            (
                "{0}?{1}={2}{3}{4}{5}",
                baseUrl,
                KeyArtifactId,
                artifactId,
                (versionId.HasValue ? string.Format(KeyValuePairSubsequentFormat, KeyVersionId, versionId.Value) : string.Empty),
                (baselineId.HasValue ? string.Format(KeyValuePairSubsequentFormat, KeyBaselineId, baselineId.Value) : string.Empty),
                (sharedViewId.HasValue ? string.Format(KeyValuePairSubsequentFormat, KeySharedViewId, sharedViewId.Value) : string.Empty)
            );
        }

        private static Boolean IsStoryTeller(string baseUrl, ItemTypePredefined? baseItemType = null)
        {
            return (!string.IsNullOrWhiteSpace(baseUrl) && baseUrl.EndsWith(KeyStorytellerDefaultDoc)) ||
                (baseItemType.HasValue && baseItemType.Value == ItemTypePredefined.Process) ||
                (baseItemType.HasValue && baseItemType.Value == ItemTypePredefined.PROShape);
        }

        private static Uri GetCurrentHostUri()
        {
            // Bug 160754 - unnecessary port number in the artifact url, use the existing Uri helper that handles the issue
            if ((HttpContext.Current == null) || (HttpContext.Current.Request == null))
            {
                return null;
            }
            return GetHostUriFromRequest(HttpContext.Current.Request) ?? BaseHostUri;
        }

        private static Uri GetHostUriFromRequest(HttpRequest httpRequest)
        {
            if (httpRequest == null)
                throw new ArgumentNullException("httpRequest");

            var requestUrl = GetPublicFacingUrl(httpRequest, httpRequest.ServerVariables);

            var uriBuilder = new UriBuilder(requestUrl.Scheme, requestUrl.Host);

            if (!requestUrl.IsDefaultPort)
            {
                uriBuilder.Port = requestUrl.Port;
            }

            if (!string.IsNullOrWhiteSpace(httpRequest.ApplicationPath))
            {
                uriBuilder.Path = httpRequest.ApplicationPath;
            }

            return uriBuilder.Uri;
        }

        //http://stackoverflow.com/questions/7795910/how-do-i-get-url-action-to-use-the-right-port-number
        /// <summary>
        /// Gets the public facing URL for the given incoming HTTP request.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <param name="serverVariables">The server variables to consider part of the request.</param>
        /// <returns>
        /// The URI that the outside world used to create this request.
        /// </returns>
        /// <remarks>
        /// Although the <paramref name="serverVariables"/> value can be obtained from
        /// <see cref="HttpRequest.ServerVariables"/>, it's useful to be able to pass them
        /// in so we can simulate injected values from our unit tests since the actual property
        /// is a read-only kind of <see cref="NameValueCollection"/>.
        /// </remarks>
        private static Uri GetPublicFacingUrl(HttpRequest request, NameValueCollection serverVariables)
        {
            // Due to URL rewriting, cloud computing (i.e. Azure)
            // and web farms, etc., we have to be VERY careful about what
            // we consider the incoming URL.  We want to see the URL as it would
            // appear on the public-facing side of the hosting web site.
            // HttpRequest.Url gives us the internal URL in a cloud environment,
            // So we use a variable that (at least from what I can tell) gives us
            // the public URL:
            if (serverVariables["HTTP_HOST"] != null)
            {
                string scheme = serverVariables["HTTP_X_FORWARDED_PROTO"] ?? request.Url.Scheme;
                Uri hostAndPort = new Uri(scheme + Uri.SchemeDelimiter + serverVariables["HTTP_HOST"]);
                UriBuilder publicRequestUri = new UriBuilder(request.Url);
                publicRequestUri.Scheme = scheme;
                publicRequestUri.Host = hostAndPort.Host;
                publicRequestUri.Port = hostAndPort.Port; // CC missing Uri.Port contract that's on UriBuilder.Port
                return publicRequestUri.Uri;
            }
            else
            {
                // Failover to the method that works for non-web farm enviroments.
                // We use Request.Url for the full path to the server, and modify it
                // with Request.RawUrl to capture both the cookieless session "directory" if it exists
                // and the original path in case URL rewriting is going on.  We don't want to be
                // fooled by URL rewriting because we're comparing the actual URL with what's in
                // the return_to parameter in some cases.
                // Response.ApplyAppPathModifier(builder.Path) would have worked for the cookieless
                // session, but not the URL rewriting problem.
                return new Uri(request.Url, request.RawUrl);
            }
        }
    }
}
