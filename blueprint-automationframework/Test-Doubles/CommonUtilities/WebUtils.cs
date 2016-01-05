using System.Net.Http.Headers;

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
    }
}
