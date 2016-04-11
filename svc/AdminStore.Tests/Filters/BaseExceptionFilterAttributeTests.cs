using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Net.Http;
using System.Web.Http.Hosting;
using System.Web.Http;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using AdminStore.Helpers;
using AdminStore.Controllers;

namespace AdminStore.Filters
{
    [TestClass]
    public class BaseExceptionFilterAttributeTests
    {
        private async Task TestOnExceptionAsync(Exception ex, HttpStatusCode expectedStatusCode)
        {
            //Arrange
            var filter = new BaseExceptionFilterAttribute();

            var request = new HttpRequestMessage(new HttpMethod("GET"), "http://someurl");
            request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
            var contextAction = ContextUtil.GetActionExecutedContext(request, null, "Controller", typeof(InstanceController));


            var controller = new InstanceController();
            contextAction.ActionContext.ControllerContext.Controller = controller;
            contextAction.Exception = ex;

            //Act
            await filter.OnExceptionAsync(contextAction, CancellationToken.None);

            //Assert
            Assert.IsTrue(contextAction.Response.StatusCode == expectedStatusCode);
        }

        [TestMethod]
        public async Task OnExceptionAsync_NotImplementedException()
        {

            await TestOnExceptionAsync(new NotImplementedException(), HttpStatusCode.NotImplemented);
        }

        [TestMethod]
        public async Task OnExceptionAsync_AuthenticationException()
        {
            await TestOnExceptionAsync(new AuthenticationException(string.Empty), HttpStatusCode.Unauthorized);
        }

        [TestMethod]
        public async Task OnExceptionAsync_ResourceNotFoundException()
        {
            await TestOnExceptionAsync(new ResourceNotFoundException(), HttpStatusCode.NotFound);
        }

        [TestMethod]
        public async Task OnExceptionAsync_BadRequestException()
        {
            await TestOnExceptionAsync(new BadRequestException(), HttpStatusCode.BadRequest);
        }

        [TestMethod]
        public async Task OnExceptionAsync_UnknownException()
        {
            await TestOnExceptionAsync(new Exception(), HttpStatusCode.InternalServerError);
        }
    }
}
