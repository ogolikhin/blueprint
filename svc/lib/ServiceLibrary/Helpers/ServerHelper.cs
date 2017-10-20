using System.Net;
using System.Text;
using System.Web;
using System.Web.Http;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Formatting;
using ServiceLibrary.Exceptions;
using ServiceLibrary.Models;

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
            response.StatusCode = (int)HttpStatusCode.BadRequest;
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
                // Use UTF8Encoding contractor that does not provide Unicode byte order mark (BOM)
                // that in turn causes a deserialization error performed by Newtonsoft json formatter in integration tests.
                jsonFormatter.WriteToStream(error.GetType(), error, response.OutputStream, new UTF8Encoding());
                response.AddHeader("Content-Type", "application/json");
            }
            else
            {
                response.Write(errorMessage);
                response.AddHeader("Content-Type", "text/plain");
            }
        }

        public static Session GetSession(HttpRequestMessage request)
        {
            object sessionValue;
            if (!request.Properties.TryGetValue(ServiceConstants.SessionProperty, out sessionValue))
            {
                throw new AuthenticationException("Authorization is required", ErrorCodes.UnauthorizedAccess);
            }

            return (Session)sessionValue;
        }

    }
}