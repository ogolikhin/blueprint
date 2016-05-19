using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using Common;

namespace CommonUtilities
{
    public static class WebUtils
    {
        /// <summary>
        /// Clears the destination collection and copies everything from source to dest.
        /// </summary>
        /// <typeparam name="T">The type in the collection.</typeparam>
        /// <param name="source">The collection to copy from.</param>
        /// <param name="dest">The collection to copy to.</param>
        private static void ClearAndCopyAllHttpHeaderCollectionValues<T>(HttpHeaderValueCollection<T> source,
            HttpHeaderValueCollection<T> dest) where T : class
        {
            dest.Clear();

            foreach (var value in source)
            {
                dest.Add(value);
            }
        }

        /// <summary>
        /// Configures the specified HttpClient with the right BaseAddress and copies the Headers into it.
        /// </summary>
        /// <param name="http">The HttpClient to be configured.</param>
        /// <param name="request">The Request to copy settings from.</param>
        /// <param name="uri">The base URI for the service you want to call.</param>
        public static void ConfigureHttpClient(HttpClient http, HttpRequestMessage request, string uri)
        {
            http.BaseAddress = new Uri(uri);
            CopyHttpRequestHeaders(request.Headers, http.DefaultRequestHeaders);
            CopySessionTokenHeader(request.Headers, http.DefaultRequestHeaders);
        }

        /// <summary>
        /// Copies the Request headers from source to dest.
        /// </summary>
        /// <param name="source">The headers to copy from.</param>
        /// <param name="dest">The headers to copy to.</param>
        public static void CopyHttpRequestHeaders(HttpRequestHeaders source, HttpRequestHeaders dest)
        {
            ClearAndCopyAllHttpHeaderCollectionValues(source.Accept, dest.Accept);
            ClearAndCopyAllHttpHeaderCollectionValues(source.AcceptCharset, dest.AcceptCharset);
            ClearAndCopyAllHttpHeaderCollectionValues(source.AcceptEncoding, dest.AcceptEncoding);
            ClearAndCopyAllHttpHeaderCollectionValues(source.AcceptLanguage, dest.AcceptLanguage);
            ClearAndCopyAllHttpHeaderCollectionValues(source.Connection, dest.Connection);
            ClearAndCopyAllHttpHeaderCollectionValues(source.Expect, dest.Expect);
            ClearAndCopyAllHttpHeaderCollectionValues(source.IfMatch, dest.IfMatch);
            ClearAndCopyAllHttpHeaderCollectionValues(source.IfNoneMatch, dest.IfNoneMatch);
            ClearAndCopyAllHttpHeaderCollectionValues(source.Pragma, dest.Pragma);
            ClearAndCopyAllHttpHeaderCollectionValues(source.TE, dest.TE);
            ClearAndCopyAllHttpHeaderCollectionValues(source.Trailer, dest.Trailer);
            ClearAndCopyAllHttpHeaderCollectionValues(source.TransferEncoding, dest.TransferEncoding);
            ClearAndCopyAllHttpHeaderCollectionValues(source.Upgrade, dest.Upgrade);
            ClearAndCopyAllHttpHeaderCollectionValues(source.UserAgent, dest.UserAgent);
            ClearAndCopyAllHttpHeaderCollectionValues(source.Via, dest.Via);
            ClearAndCopyAllHttpHeaderCollectionValues(source.Warning, dest.Warning);

            dest.Authorization = source.Authorization;
            dest.CacheControl = source.CacheControl;
            dest.ConnectionClose = source.ConnectionClose;
            dest.Date = source.Date;
            dest.ExpectContinue = source.ExpectContinue;
            dest.From = source.From;
            dest.Host = source.Host;
            dest.IfModifiedSince = source.IfModifiedSince;
            dest.IfRange = source.IfRange;
            dest.IfUnmodifiedSince = source.IfUnmodifiedSince;
            dest.MaxForwards = source.MaxForwards;
            dest.ProxyAuthorization = source.ProxyAuthorization;
            dest.Range = source.Range;
            dest.Referrer = source.Referrer;
            dest.TransferEncodingChunked = source.TransferEncodingChunked;
        }

        /// <summary>
        /// If present, copies the Session-Token from the source headers to dest.
        /// </summary>
        /// <param name="source">The HTTP headers to copy from.</param>
        /// <param name="dest">The HTTP headers to copy to.</param>
        public static void CopySessionTokenHeader(HttpRequestHeaders source, HttpRequestHeaders dest)
        {
            const string TOKEN_HEADER = "Session-Token";

            if (source.Contains(TOKEN_HEADER))
            {
                var tokens = source.GetValues(TOKEN_HEADER);
                dest.Add(TOKEN_HEADER, tokens.First());
            }
        }

        /// <summary>
        /// Creates a copy of the request Uri that points to the real AccessControl.
        /// </summary>
        /// <param name="requestUri">The Request Uri to copy from.</param>
        /// <param name="svcBaseUri">The base Uri of the service.</param>
        /// <param name="svcPath">The service path for the Uri.</param>
        /// <returns>The new Uri.</returns>
        public static Uri CreateUri(Uri requestUri, string svcBaseUri, string svcPath)
        {
            string path = requestUri.LocalPath.ToLowerInvariant().Replace(svcPath.ToLowerInvariant(), string.Empty);
            Uri uri = new Uri(I18NHelper.FormatInvariant("{0}/{1}/{2}", svcBaseUri, path.TrimEnd('/'), requestUri.Query).TrimEnd('/'));

            return uri;
        }

        /// <summary>
        /// Writes the REST response to the log file.
        /// </summary>
        /// <param name="logFile">The open log file to write to.</param>
        /// <param name="response">The response we got from a REST API call.</param>
        public static void LogRestResponse(LogFile logFile, HttpResponseMessage response)
        {
            logFile.WriteLine("    --> Got back: StatusCode = {0}", response.StatusCode);

            if (response.StatusCode != HttpStatusCode.OK)
            {
                logFile.WriteLine("    --> ReasonPhrase = {0}", response.ReasonPhrase);
                logFile.WriteLine("    --> Content = {0}", response.Content);
            }
        }

        public static void LogRestResponse(string logFileName, HttpResponseMessage response)
        {
            using (LogFile logFile = new LogFile(logFileName))
            {
                LogRestResponse(logFile, response);
            }
        }

    }
}
