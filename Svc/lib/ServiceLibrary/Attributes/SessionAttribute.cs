using System;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using Newtonsoft.Json;
using ServiceLibrary.Helpers;
using ServiceLibrary.Models;

namespace ServiceLibrary.Attributes
{
    public class NoSessionRequiredAttribute : Attribute
    {
    }

    public class SessionAttribute : ActionFilterAttribute
    {
        protected const string BlueprintSessionToken = "Session-Token";
        protected const string BlueprintSessionCookie = "BLUEPRINT_SESSION_TOKEN";
        protected const string AccessControl = "AccessControl";
        protected const string BadRequestMessage = "Token is missing or malformed.";
        protected const string UnauthorizedMessage = "Token is invalid.";
        protected const string InternalServerErrorMessage = "An error occurred.";
        protected const string BlueprintSessionTokenIgnore = "e51d8f58-0c62-46ad-a6fc-7e7994670f34";

        internal readonly IHttpClientProvider _httpClientProvider;

        public SessionAttribute() : this(new HttpClientProvider())
        {
        }

        internal SessionAttribute(IHttpClientProvider httpClientProvider)
        {
            _httpClientProvider = httpClientProvider;
        }

        public override async Task OnActionExecutingAsync(HttpActionContext actionContext, CancellationToken cancellationToken)
        {
            await base.OnActionExecutingAsync(actionContext, cancellationToken);
        }

        protected async Task<Session> GetAccessAsync(HttpRequestMessage request)
        {
            var uri = new Uri(ConfigurationManager.AppSettings[AccessControl]);
            var http = _httpClientProvider.Create(uri);
            var request2 = new HttpRequestMessage { RequestUri = new Uri(uri, "sessions"), Method = HttpMethod.Put };
            request2.Headers.Add(BlueprintSessionToken, GetHeaderSessionToken(request));
            var result = await http.SendAsync(request2);
            result.EnsureSuccessStatusCode();
            var content = await result.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<Session>(content);
        }

        private static string GetHeaderSessionToken(HttpRequestMessage request)
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
