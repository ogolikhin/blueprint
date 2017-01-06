using System;
using System.Globalization;
using System.Net;
using System.Net.Http.Headers;

namespace ServiceLibrary.Helpers
{
    public static class WebHeaderCollectionExtensions
    {
        public static T GetValue<T>(this WebHeaderCollection webHeaderCollection, string headerName)
        {
            string headerValue = webHeaderCollection.Get(headerName);

            if (string.IsNullOrWhiteSpace(headerValue))
            {
                throw new Exception(string.Format(CultureInfo.InvariantCulture, "Web header '{0}' is not found.", headerName));
            }

            try
            {
                if (typeof(T) == typeof(DateTime))
                {
                    return (T)((object)DateTime.ParseExact(headerValue, "o", CultureInfo.InvariantCulture));
                }

                if (typeof(T) == typeof(ContentDispositionHeaderValue))
                {
                    return (T)((object)ContentDispositionHeaderValue.Parse(headerValue));
                }

                if (typeof(T) == typeof(MediaTypeHeaderValue))
                {
                    return (T)((object)MediaTypeHeaderValue.Parse(headerValue));
                }

                return (T)Convert.ChangeType(headerValue, typeof(T), CultureInfo.InvariantCulture);
            }
            catch (Exception ex)
            {
                throw new Exception(string.Format(CultureInfo.InvariantCulture, "Web header '{0}' value '{1}' is invalid", headerName, headerValue), ex);
            }
        }
    }
}
