using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Net.Http;
using System.Web.Http.Hosting;
using System.Web.Http;
using Moq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using AdminStore.Helpers;
using AdminStore.Controllers;
using ServiceLibrary.Repositories.ConfigControl;

namespace AdminStore.Filters
{
    [TestClass]
    public class BaseExceptionFilterAttributeTests
    {
        private void TestOnExceptionAsync(Exception ex, HttpStatusCode expectedStatusCode)
        {
            //Arrange
            var filter = new BaseExceptionFilterAttribute();

            var request = new HttpRequestMessage(new HttpMethod("GET"), "http://someurl");
            request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
            var contextAction = ContextUtil.GetActionExecutedContext(request, null, "Controller", typeof(InstanceController));

            var controller = new InstanceController();
            contextAction.ActionContext.ControllerContext.Controller = controller;
            contextAction.Exception = ex;

            Task.Run(async () =>
            {
                //Act
                await filter.OnExceptionAsync(contextAction, CancellationToken.None);

                //Assert
                Assert.IsTrue(contextAction.Response.StatusCode == expectedStatusCode);

            }).GetAwaiter().GetResult();
        }

        [TestMethod]
        public void OnExceptionAsync_NotImplementedException()
        {

            TestOnExceptionAsync(new NotImplementedException(), HttpStatusCode.NotImplemented);
        }

        [TestMethod]
        public void OnExceptionAsync_AuthenticationException()
        {
            TestOnExceptionAsync(new AuthenticationException(string.Empty), HttpStatusCode.Unauthorized);
        }

        [TestMethod]
        public void OnExceptionAsync_ResourceNotFoundException()
        {
            TestOnExceptionAsync(new ResourceNotFoundException(), HttpStatusCode.NotFound);
        }

        [TestMethod]
        public void OnExceptionAsync_UnknownException()
        {
            TestOnExceptionAsync(new Exception(), HttpStatusCode.InternalServerError);
        }
    }
}
