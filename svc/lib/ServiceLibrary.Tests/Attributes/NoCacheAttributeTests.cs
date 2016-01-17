using System.Linq;
using System.Net.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ServiceLibrary.Attributes
{
    [TestClass]
    public class NoCacheAttributeTests
    {
        #region OnActionExecuted

        [TestMethod]
        public void OnActionExecuted_Always_AddsResponseHeaders()
        {
            // Arrange
            var attribute = new NoCacheAttribute();
            var response = new HttpResponseMessage();
            var context = new HttpActionExecutedContext { ActionContext = new HttpActionContext(), Response = response };

            // Act
            attribute.OnActionExecuted(context);

            // Assert
            Assert.IsTrue(response.Headers.Contains("Cache-Control"));
            Assert.IsTrue(response.Headers.GetValues("Cache-Control").FirstOrDefault() == "no-store, must-revalidate, no-cache");
            Assert.IsTrue(response.Headers.Contains("Pragma"));
            Assert.IsTrue(response.Headers.GetValues("Pragma").FirstOrDefault() == "no-cache");
        }

        #endregion OnActionExecuted
    }
}
