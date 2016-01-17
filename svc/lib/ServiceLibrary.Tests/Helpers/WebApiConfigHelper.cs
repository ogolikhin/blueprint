using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Dispatcher;
using System.Web.Http.Hosting;
using System.Web.Http.Routing;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ServiceLibrary.Helpers
{
    public static class WebApiConfigHelper
    {
        private static readonly Uri BaseUri = new Uri("http://localhost");

        private static IEnumerable<T> SelectLeaves<T>(this IEnumerable<T> enumeration,
            Func<T, IEnumerable<T>> childSelector)
        {
            return enumeration.SelectMany(item => SelectLeaves(childSelector(item) ?? Enumerable.Empty<T>(), childSelector)
                .DefaultIfEmpty(item));
        }

        public static void AssertTotalRoutes(this HttpConfiguration config, int expectedCount, string message)
        {
            Assert.AreEqual(expectedCount, config.Routes.SelectLeaves(r => r as IEnumerable<IHttpRoute>).Count(), message);
        }

        public static void AssertAction<T>(this HttpConfiguration config, string expectedAction, HttpMethod method, string requestUri)
            where T : ApiController
        {
            // Check controller
            var request = new HttpRequestMessage(method, new Uri(BaseUri, requestUri));
            IHttpRouteData routeData = config.Routes.GetRouteData(request);
            request.Properties[HttpPropertyKeys.HttpRouteDataKey] = routeData;
            HttpControllerDescriptor controller;
            Type controllerType = null;
            try
            {
                controller = new DefaultHttpControllerSelector(config).SelectController(request);
                controllerType = controller.ControllerType;
            }
            catch (HttpResponseException)
            {
                controller = null;
            }
            Assert.AreEqual(typeof(T), controllerType);

            // Check action
            var context = new HttpControllerContext(config, routeData, request) { ControllerDescriptor = controller };
            string actionName;
            try
            {
                actionName = new ApiControllerActionSelector().SelectAction(context).ActionName;
            }
            catch (HttpResponseException)
            {
                actionName = null;
            }

            Assert.AreEqual(expectedAction, actionName);
        }

        public static void AssertMethodAttributes(this HttpConfiguration config, Predicate<object[]> predicate, string messageFormat)
        {
            foreach (var route in config.Routes.SelectLeaves(r => r as IEnumerable<IHttpRoute>))
            {
                Assert.IsNotNull(route.DataTokens, "Missing DataTokens");
                var actions = route.DataTokens["actions"] as object[];
                Assert.IsNotNull(actions, "Missing actions in DataTokens");
                var actionDescriptor = actions.OfType<ReflectedHttpActionDescriptor>().FirstOrDefault();
                Assert.IsNotNull(actionDescriptor, "No ReflectedHttpActionDescriptor in DataTokens");
                var method = actionDescriptor.MethodInfo;

                Assert.IsTrue(predicate(method.GetCustomAttributes(false)), I18NHelper.FormatInvariant(messageFormat, method));
            }
        }
    }
}
