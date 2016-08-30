using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ServiceLibrary.Helpers;
using ServiceLibrary.Models;

namespace ServiceLibrary.Attributes
{
    [TestClass]
    public class SessionAttributeTests
    {
        #region Constructor

        [TestMethod]
        public void Constructor_Always_CreatesDefaultDependencies()
        {
            // Arrange

            // Act
            var atrribute = new SessionAttribute();

            // Assert
            Assert.IsInstanceOfType(atrribute._httpClientProvider, typeof(HttpClientProvider));
        }

        #endregion Constructor

        #region OnActionExecutingAsync

        [TestMethod]
        public async Task OnActionExecutingAsync_BlueprintSessionIgnoreToken_ResponseIsNull()
        {
            // Arrange
            var attribute = new SessionAttribute();
            var request = new HttpRequestMessage(HttpMethod.Get, "");
            request.Headers.Add("e51d8f58-0c62-46ad-a6fc-7e7994670f34", "");
            var actionContext = HttpFilterHelper.CreateHttpActionContext(request);

            // Act
            await attribute.OnActionExecutingAsync(actionContext, CancellationToken.None);

            // Assert
            Assert.IsNull(actionContext.Response);
        }

        [TestMethod]
        public async Task OnActionExecutingAsync_ValidSessionToken_ResponseIsNull()
        {
            // Arrange
            string session = Session.Convert(new Guid());
            var attribute = new SessionAttribute(false, false, CreateHttpClientProvider(session));
            var request = new HttpRequestMessage(HttpMethod.Get, "");
            request.Headers.Add("Session-Token", session);
            var actionContext = HttpFilterHelper.CreateHttpActionContext(request);

            // Act
            await attribute.OnActionExecutingAsync(actionContext, CancellationToken.None);

            // Assert
            Assert.IsNull(actionContext.Response);
        }

        [TestMethod]
        public async Task OnActionExecutingAsync_AllowCookieAndValidCookie_ResponseIsNull()
        {
            // Arrange
            string session = Session.Convert(new Guid());
            var attribute = new SessionAttribute(true, false, CreateHttpClientProvider(session, false));
            var request = new HttpRequestMessage(HttpMethod.Get, "");
            request.Headers.Add("Cookie", "BLUEPRINT_SESSION_TOKEN=" + session);
            var actionContext = HttpFilterHelper.CreateHttpActionContext(request);

            // Act
            await attribute.OnActionExecutingAsync(actionContext, CancellationToken.None);

            // Assert
            Assert.IsNull(actionContext.Response);
        }

        [TestMethod]
        public async Task OnActionExecutingAsync_InvalidSessionToken_Unauthorized()
        {
            // Arrange
            string session = Session.Convert(new Guid());
            var attribute = new SessionAttribute(false, false, CreateHttpClientProvider(""));
            var request = new HttpRequestMessage(HttpMethod.Get, "");
            request.Headers.Add("Session-Token", session);
            var actionContext = HttpFilterHelper.CreateHttpActionContext(request);

            // Act
            await attribute.OnActionExecutingAsync(actionContext, CancellationToken.None);

            // Assert
            Assert.AreEqual(HttpStatusCode.Unauthorized, actionContext.Response.StatusCode);
        }

        [TestMethod]
        public async Task OnActionExecutingAsync_IgnoreBadTokenAndInvalidSessionToken_ResponseIsNull()
        {
            // Arrange
            string session = Session.Convert(new Guid());
            var attribute = new SessionAttribute(false, true, CreateHttpClientProvider(""));
            var request = new HttpRequestMessage(HttpMethod.Get, "");
            request.Headers.Add("Session-Token", session);
            var actionContext = HttpFilterHelper.CreateHttpActionContext(request);

            // Act
            await attribute.OnActionExecutingAsync(actionContext, CancellationToken.None);

            // Assert
            Assert.IsNull(actionContext.Response);
        }

        [TestMethod]
        public async Task OnActionExecutingAsync_AllowCookieAndNoSessionTokenOrCookie_BadRequest()
        {
            // Arrange
            var attribute = new SessionAttribute(true);
            var request = new HttpRequestMessage(HttpMethod.Get, "");
            var actionContext = HttpFilterHelper.CreateHttpActionContext(request);

            // Act
            await attribute.OnActionExecutingAsync(actionContext, CancellationToken.None);

            // Assert
            Assert.AreEqual(HttpStatusCode.BadRequest, actionContext.Response.StatusCode);
        }

        [TestMethod]
        public async Task OnActionExecutingAsync_DoNotAllowCookieAndValidCookie_BadRequest()
        {
            // Arrange
            string session = Session.Convert(new Guid());
            var attribute = new SessionAttribute(false, false, CreateHttpClientProvider(session, false));
            var request = new HttpRequestMessage(HttpMethod.Put, "");
            request.Headers.Add("Cookie", "BLUEPRINT_SESSION_TOKEN=" + session);
            var actionContext = HttpFilterHelper.CreateHttpActionContext(request);

            // Act
            await attribute.OnActionExecutingAsync(actionContext, CancellationToken.None);

            // Assert
            Assert.AreEqual(HttpStatusCode.BadRequest, actionContext.Response.StatusCode);
        }

        [TestMethod]
        public async Task OnActionExecutingAsync_IgnoreBadTokenAndNoSessionToken_ResponseIsNull()
        {
            // Arrange
            string session = Session.Convert(new Guid());
            var attribute = new SessionAttribute(false, true, CreateHttpClientProvider(session));
            var request = new HttpRequestMessage(HttpMethod.Put, "");
            var actionContext = HttpFilterHelper.CreateHttpActionContext(request);

            // Act
            await attribute.OnActionExecutingAsync(actionContext, CancellationToken.None);

            // Assert
            Assert.IsNull(actionContext.Response);
        }

        [TestMethod]
        public async Task OnActionExecutingAsync_Exception_InternalServerError()
        {
            // Arrange
            string session = Session.Convert(new Guid());
            var attribute = new SessionAttribute(false, false, new TestHttpClientProvider(r => { throw new Exception(); }));
            var request = new HttpRequestMessage(HttpMethod.Get, "");
            request.Headers.Add("Session-Token", session);
            var actionContext = HttpFilterHelper.CreateHttpActionContext(request);

            // Act
            await attribute.OnActionExecutingAsync(actionContext, CancellationToken.None);

            // Assert
            Assert.AreEqual(HttpStatusCode.InternalServerError, actionContext.Response.StatusCode);
        }

        #endregion OnActionExecutingAsync

        private static IHttpClientProvider CreateHttpClientProvider(string token, bool useCookies = true)
        {
            return new TestHttpClientProvider(request =>
            {
                Assert.AreEqual(HttpMethod.Put, request.Method, I18NHelper.FormatInvariant("Unexpected HttpMethod: {0}", request.Method));
                Assert.IsTrue(request.RequestUri.AbsoluteUri.EndsWithOrdinal("svc/accesscontrol/sessions"), I18NHelper.FormatInvariant("Unexpected RequestUri: {0}", request.RequestUri));
                Assert.IsTrue(request.Headers.Contains("Session-Token"), "Request does not cotain Session-Token header");
                if (request.Headers.GetValues("Session-Token").FirstOrDefault() == token)
                {
                    var session = new Session { SessionId = Session.Convert(token) };
                    return new HttpResponseMessage(HttpStatusCode.OK) { Content = new ObjectContent(typeof(Session), session, new JsonMediaTypeFormatter()) };
                }
                return new HttpResponseMessage(HttpStatusCode.NotFound);
            }) { UseCookies = useCookies };
        }
    }
}
