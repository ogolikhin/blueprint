using System;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using ServiceLibrary.Helpers;

namespace ServiceLibrary.Attributes
{
    public class ValidateTokenAttribute : ActionFilterAttribute
    {
        private const string BlueprintSessionToken = "Session-Token";
        private const string BlueprintSessionCookie = "BLUEPRINT_SESSION_TOKEN";
        private const string AccessControl = "AccessControl";
        private const string UnauthorizedExceptionMessage = "You are not authorized. Please provide token.";
        private const string InternalServerError = "An error occured.";
        private const string BlueprintSessionTokenIgnore = "e51d8f58-0c62-46ad-a6fc-7e7994670f34";

        internal readonly IHttpClientProvider _httpClientProvider;

        public ValidateTokenAttribute() : this(new HttpClientProvider())
        {
        }

        internal ValidateTokenAttribute(IHttpClientProvider httpClientProvider)
        {
            _httpClientProvider = httpClientProvider;
        }

        public override async Task OnActionExecutingAsync(HttpActionContext actionContext, CancellationToken cancellationToken)
        {
            if (actionContext.Request.Headers.Contains(BlueprintSessionTokenIgnore))
            {
                return;
            }

            try
            {
                await GetAccessAsync(actionContext.Request);
            }
            catch (ArgumentNullException)
            {
                actionContext.Response = actionContext.Request.CreateErrorResponse(HttpStatusCode.Unauthorized,
                    UnauthorizedExceptionMessage);
            }
            catch (HttpRequestException)
            {
                actionContext.Response = actionContext.Request.CreateErrorResponse(HttpStatusCode.Unauthorized,
                    UnauthorizedExceptionMessage);
            }
            catch 
            {
                actionContext.Response = actionContext.Request.CreateErrorResponse(HttpStatusCode.InternalServerError,
                    InternalServerError);
            }

            await base.OnActionExecutingAsync(actionContext, cancellationToken);
        }

        private async Task GetAccessAsync(HttpRequestMessage request)
        {
            var uri = ConfigurationManager.AppSettings[AccessControl];
            using (var http = _httpClientProvider.Create())
            {
                http.BaseAddress = new Uri(uri);
                http.DefaultRequestHeaders.Accept.Clear();
                http.DefaultRequestHeaders.Add(BlueprintSessionToken, GetHeaderSessionToken(request));
                var result = await http.PutAsync("sessions", null);
                result.EnsureSuccessStatusCode();
            }
        }

        private string GetHeaderSessionToken(HttpRequestMessage request)
        {
            if (request.Headers.Contains(BlueprintSessionToken))
            {
                return request.Headers.GetValues(BlueprintSessionToken).FirstOrDefault();
            }
            if (request.Method != HttpMethod.Get)
            {
                throw new ArgumentNullException();
            }
            var sessionTokenCookie = request.Headers.GetCookies(BlueprintSessionCookie).FirstOrDefault();
            if (sessionTokenCookie == null)
            {
                throw new ArgumentNullException();
            }
            return sessionTokenCookie[BlueprintSessionCookie].Value;
        }
    }
}
