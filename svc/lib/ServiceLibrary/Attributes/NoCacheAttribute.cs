using System.Web.Http.Filters;

namespace ServiceLibrary.Attributes
{
    public class NoCacheAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuted(HttpActionExecutedContext actionExecutedContext)
        {
            base.OnActionExecuted(actionExecutedContext);
            actionExecutedContext.Response?.Headers?.Add("Cache-Control", "no-cache, no-store, must-revalidate"); // HTTP 1.1.
            actionExecutedContext.Response?.Headers?.Add("Pragma", "no-cache"); // HTTP 1.0.
        }
    }
}
