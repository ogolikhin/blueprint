using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using System.Web.Http.Routing;

namespace ServiceLibrary.Helpers
{
    public static class HttpFilterHelper
    {
        public static HttpActionContext CreateHttpActionContext(HttpRequestMessage request)
        {
            var context = new HttpControllerContext(new HttpConfiguration(), new HttpRouteData(new HttpRoute()), request);
            var descriptor = new ReflectedHttpActionDescriptor();
            return new HttpActionContext(context, descriptor);
        }

        public static HttpActionExecutedContext CreateActionExecutedContext(HttpRequestMessage request, HttpResponseMessage response)
        {
            HttpActionContext actionContext = CreateHttpActionContext(request);
            actionContext.ControllerContext.Request = request;
            HttpActionExecutedContext actionExecutedContext = new HttpActionExecutedContext(actionContext, null) { Response = response };
            return actionExecutedContext;
        }
    }
}
