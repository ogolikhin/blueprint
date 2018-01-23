using System;
using System.Net.Http.Headers;

namespace ServiceLibrary.Attributes
{

    public class ResponseCacheAttribute : BaseCacheAttribute
    {
        private int? _duration;

        // <summary>
        // Specifies the maximum amount of time in seconds a resource will be considered fresh.
        // </summary>
        public int Duration
        {
            get { return _duration ?? 0; }
            set { _duration = value; }
        }

        protected override void CustomizeHttpResponseHeaders(HttpResponseHeaders responseHeaders)
        {
            if (_duration.HasValue)
            {
                responseHeaders.CacheControl.MaxAge = TimeSpan.FromSeconds(Duration);
            }
        }

    }
}