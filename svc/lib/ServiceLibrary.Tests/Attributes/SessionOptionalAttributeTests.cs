using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Routing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ServiceLibrary.Helpers;
using ServiceLibrary.Models;

namespace ServiceLibrary.Attributes
{
    [TestClass]
    public class SessionOptionalAttributeTests
    {
        #region Constructor

        [TestMethod]
        public void Constructor_Always_CreatesDefaultDependencies()
        {
            // Arrange

            // Act
            var atrribute = new SessionOptionalAttribute();

            // Assert
            Assert.IsInstanceOfType(atrribute._httpClientProvider, typeof(HttpClientProvider));
        }

        #endregion Constructor

        #region OnActionExecutingAsync
        [TestMethod]
        public async Task OnActionExecutingAsync_BlueprintSessionIgnoreToken_ResponseIsNull()
        {
            // Arrange
            var attribute = new SessionOptionalAttribute();
            var request = new HttpRequestMessage(new HttpMethod("GET"), "http://localhost/");
            request.Headers.Add("e51d8f58-0c62-46ad-a6fc-7e7994670f34", "");
            var actionContext = CreateHttpActionContext(request);

            // Act
            await attribute.OnActionExecutingAsync(actionContext, new CancellationToken());

            // Assert
            Assert.IsNull(actionContext.Response);
        }

        [TestMethod]
        public async Task OnActionExecutingAsync_InvalidSession_ResponseIsNull()
        {
            // Arrange
            string session = Session.Convert(new Guid());
            var attribute = new SessionOptionalAttribute(CreateHttpClientProvider(""));
            var request = new HttpRequestMessage(new HttpMethod("GET"), "http://localhost");
            request.Headers.Add("Session-Token", session);
            var actionContext = CreateHttpActionContext(request);

            // Act
            await attribute.OnActionExecutingAsync(actionContext, new CancellationToken());

            // Assert
            Assert.IsNull(actionContext.Response);
        }

        [TestMethod]
        public async Task OnActionExecutingAsync_PutWithNoSessionToken_BadRequest()
        {
            // Arrange
            var attribute = new SessionOptionalAttribute();
            var request = new HttpRequestMessage(new HttpMethod("PUT"), "http://localhost");
            var actionContext = CreateHttpActionContext(request);

            // Act
            await attribute.OnActionExecutingAsync(actionContext, new CancellationToken());

            // Assert
            Assert.IsNull(actionContext.Response);
        }

        [TestMethod]
        public async Task OnActionExecutingAsync_Exception_InternalServerError()
        {
            // Arrange
            string session = Session.Convert(new Guid());
            var attribute = new SessionOptionalAttribute(new TestHttpClientProvider(r => { throw new Exception(); }));
            var request = new HttpRequestMessage(new HttpMethod("GET"), "http://localhost");
            request.Headers.Add("Session-Token", session);
            var actionContext = CreateHttpActionContext(request);

            // Act
            await attribute.OnActionExecutingAsync(actionContext, new CancellationToken());

            // Assert
            Assert.AreEqual(HttpStatusCode.InternalServerError, actionContext.Response.StatusCode);
        }


        #endregion OnActionExecutingAsync

        private static HttpActionContext CreateHttpActionContext(HttpRequestMessage request)
        {
            var context = new HttpControllerContext(new HttpConfiguration(), new HttpRouteData(new HttpRoute()), request);
            var descriptor = new ReflectedHttpActionDescriptor();
            return new HttpActionContext(context, descriptor);
        }

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
            })
            { UseCookies = useCookies };
        }

    }
}
