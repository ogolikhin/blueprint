//http://aspnetwebstack.codeplex.com/SourceControl/changeset/view/98d041ae352f#test/System.Web.Http.Test/Util/ContextUtil.cs

using System.Net.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using System.Web.Http.Hosting;
using System.Web.Http.Routing;
using Moq;

namespace System.Web.Http
{
    internal static class ContextUtil
    {
        public static HttpControllerContext CreateControllerContext(HttpConfiguration configuration = null, IHttpController instance = null, IHttpRouteData routeData = null, HttpRequestMessage request = null, string controllerName = null, Type controllerType = null, IHttpController controller = null)
        {
            HttpConfiguration config = configuration ?? new HttpConfiguration();
            IHttpRouteData route = routeData ?? new HttpRouteData(new HttpRoute());
            HttpRequestMessage req = request ?? new HttpRequestMessage();
            req.Properties[HttpPropertyKeys.HttpConfigurationKey] = config;
            req.Properties[HttpPropertyKeys.HttpRouteDataKey] = route;

            HttpControllerContext context = new HttpControllerContext(config, route, req);
            if (instance != null)
            {
                context.Controller = instance;
            }
            if (controller != null)
            {
                context.Controller = controller;
            }
            context.ControllerDescriptor = CreateControllerDescriptor(config, controllerName, controllerType);
            return context;
        }

        public static HttpActionContext CreateActionContext(HttpControllerContext controllerContext = null, HttpActionDescriptor actionDescriptor = null, string controllerName = null, Type controllerType = null, HttpRequestMessage request = null, IHttpController controller = null)
        {
            HttpControllerContext context = controllerContext ?? ContextUtil.CreateControllerContext(controllerName: controllerName, controllerType: controllerType, request: request, controller: controller);
            HttpActionDescriptor descriptor = actionDescriptor ?? new Mock<HttpActionDescriptor>() { CallBase = true }.Object;
            return new HttpActionContext(context, descriptor);
        }

        public static HttpActionContext GetHttpActionContext(HttpRequestMessage request)
        {
            HttpActionContext actionContext = CreateActionContext();
            actionContext.ControllerContext.Request = request;
            return actionContext;
        }

        public static HttpActionExecutedContext GetActionExecutedContext(HttpRequestMessage request, HttpResponseMessage response, string controllerName, Type controllerType)
        {
            HttpActionContext actionContext = CreateActionContext(controllerName: controllerName, controllerType: controllerType);
            actionContext.ControllerContext.Request = request;
            HttpActionExecutedContext actionExecutedContext = new HttpActionExecutedContext(actionContext, null) { Response = response };
            return actionExecutedContext;
        }

        public static HttpActionContext GetActionExecutingContext(HttpRequestMessage request, HttpResponseMessage response, string controllerName, Type controllerType, IHttpController controller = null)
        {
            HttpActionContext actionContext = CreateActionContext(controllerName: controllerName, controllerType: controllerType, request: request, controller: controller);
            return actionContext;
        }

        public static HttpControllerDescriptor CreateControllerDescriptor(HttpConfiguration config = null, string controllerName = null, Type controllerType = null)
        {
            if (config == null)
            {
                config = new HttpConfiguration();
            }
            return new HttpControllerDescriptor(config, controllerName, controllerType);
        }
    }
}

