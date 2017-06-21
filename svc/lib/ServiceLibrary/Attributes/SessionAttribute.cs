using System;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using Newtonsoft.Json;
using ServiceLibrary.Helpers;
using ServiceLibrary.Models;
using ServiceLibrary.Helpers.Cache;
using System.Runtime.Caching;

namespace ServiceLibrary.Attributes
{
    public class NoSessionRequiredAttribute : Attribute
    {
    }

    public class SessionRequiredAttribute : SessionAttribute
    {
        public SessionRequiredAttribute(bool allowCookie = false) : base(allowCookie) { }
    }

    public class SessionOptionalAttribute : SessionAttribute
    {
        public SessionOptionalAttribute(bool allowCookie = false) : base(allowCookie, true) { }
    }

    public class SessionAttribute : ActionFilterAttribute
    {
        protected const string BlueprintSessionToken = "Session-Token";
        protected const string BlueprintSessionCookie = "BLUEPRINT_SESSION_TOKEN";
        protected const string AccessControl = "AccessControl";
        protected const string TokenMissingMalformedMessage = "Token is missing or malformed.";
        protected const string TokenInvalidMessage = "Token is invalid.";
        protected const string InternalServerErrorMessage = "An error occurred.";
        protected const string BlueprintSessionTokenIgnore = "e51d8f58-0c62-46ad-a6fc-7e7994670f34";
        protected const int DefaultSessionCacheExpiration = 30;

        private readonly bool _allowCookie;
        private readonly bool _ignoreBadToken;
        internal readonly IHttpClientProvider _httpClientProvider;
        internal readonly IAsyncCache _cache;

        protected internal SessionAttribute(bool allowCookie = false, bool ignoreBadToken = false) :
            this(allowCookie, ignoreBadToken, new HttpClientProvider(), SessionsCacheSettings.IsSessionCacheEnabled ? AsyncCache.Default : AsyncCache.NoCache)
        {
        }

        internal SessionAttribute(bool allowCookie, bool ignoreBadToken, IHttpClientProvider httpClientProvider, IAsyncCache cache)
        {
            _allowCookie = allowCookie;
            _ignoreBadToken = ignoreBadToken;
            _httpClientProvider = httpClientProvider;
            _cache = cache;
        }

        public override async Task OnActionExecutingAsync(HttpActionContext actionContext, CancellationToken cancellationToken)
        {
            var request = actionContext.Request;
            if (!request.Headers.Contains(BlueprintSessionTokenIgnore))
            {
                try
                {
                    request.Properties[ServiceConstants.SessionProperty] = await GetAccessAsync(request);
                }
                catch (ArgumentNullException)
                {
                    if (!_ignoreBadToken)
                    {
                        actionContext.Response = request.CreateErrorResponse(HttpStatusCode.Unauthorized,
                            TokenMissingMalformedMessage);
                    }
                }
                catch (HttpRequestException)
                {
                    if (!_ignoreBadToken)
                    {
                        actionContext.Response = request.CreateErrorResponse(HttpStatusCode.Unauthorized,
                            TokenInvalidMessage);
                    }
                }
                catch
                {
                    actionContext.Response = request.CreateErrorResponse(HttpStatusCode.InternalServerError,
                        InternalServerErrorMessage);
                }
            }
            await base.OnActionExecutingAsync(actionContext, cancellationToken);
        }

        protected async Task<Session> GetAccessAsync(HttpRequestMessage request)
        {
            var sessionToken = GetHeaderSessionToken(request);

            if (string.IsNullOrWhiteSpace(sessionToken))
            {
                throw new ArgumentNullException(nameof(sessionToken));
            }

            var session = await _cache.AddOrGetExistingAsync(sessionToken, () => GetSessionFromAccessControl(sessionToken), DateTimeOffset.UtcNow.Add(SessionsCacheSettings.SessionCacheExpiration));
            return session;
        }

        private async Task<Session> GetSessionFromAccessControl(string sessionToken)
        {
            var uri = new Uri(ConfigurationManager.AppSettings[AccessControl]);
            var http = _httpClientProvider.Create(uri);
            var request2 = new HttpRequestMessage { RequestUri = new Uri(uri, "sessions"), Method = HttpMethod.Put };
            request2.Headers.Add(BlueprintSessionToken, sessionToken);
            var result = await http.SendAsync(request2);
            result.EnsureSuccessStatusCode();
            var content = await result.Content.ReadAsStringAsync();

            var session = JsonConvert.DeserializeObject<Session>(content);
            session.SessionId = Session.Convert(sessionToken);

            return session;
        }

        private string GetHeaderSessionToken(HttpRequestMessage request)
        {
            if (request.Headers.Contains(BlueprintSessionToken))
            {
                return request.Headers.GetValues(BlueprintSessionToken).FirstOrDefault();
            }
            if (!_allowCookie)
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
