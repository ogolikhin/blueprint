﻿using System.Net;
using System.Text;
using System.Web;
using System.Web.Http;
using System.Linq;
using System.Net.Http.Formatting;

namespace ServiceLibrary.Helpers
{
    public static class ServerHelper
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1308: Normalize strings to uppercase", Justification = "No need to do a round trip conversion.")]
        public static void UpdateResponseWithError(HttpRequest request, HttpResponse response,
            JsonMediaTypeFormatter jsonFormatter,
            XmlMediaTypeFormatter xmlFormatter,
            string errorMessage)
        {
            response.Clear();
            response.StatusCode = (int) HttpStatusCode.BadRequest;
            var error = new HttpError(errorMessage);
            var acceptTypes = request.AcceptTypes?.Select(at => at.ToLowerInvariant()) ?? new string[] { };

            if (xmlFormatter != null &&
                xmlFormatter.SupportedMediaTypes.Select(mt => mt.MediaType).Intersect(acceptTypes).Any())
            {
                xmlFormatter.CreateXmlSerializer(error.GetType()).Serialize(response.OutputStream, error);
                response.AddHeader("Content-Type", "application/xml");
            }
            // JSON is the default format
            else if (jsonFormatter != null)
            {
                jsonFormatter.WriteToStream(error.GetType(), error,response.OutputStream, Encoding.UTF8);
                response.AddHeader("Content-Type", "application/json");
            }
            else
            {
                response.Write(errorMessage);
                response.AddHeader("Content-Type", "text/plain");
            }
        }

    }
}