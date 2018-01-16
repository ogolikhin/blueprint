using System.Web.Http.Filters;

namespace ServiceLibrary.Attributes
{
    public class CacheAttribute : ActionFilterAttribute
    {
        private readonly string _maxAge;

        public CacheAttribute(string maxAge)
        {
            _maxAge = $"max-age={maxAge}";
        }
        public override void OnActionExecuted(HttpActionExecutedContext actionExecutedContext)
        {
            base.OnActionExecuted(actionExecutedContext);
            actionExecutedContext.Response?.Headers?.Add("Cache-Control", _maxAge); // HTTP 1.1.
        }
    }
}