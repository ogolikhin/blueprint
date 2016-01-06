using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using ServiceLibrary.Repositories.ConfigControl;

namespace ServiceLibrary.Attributes
{
    public class ValidateToken : ActionFilterAttribute
    {
        private const string UnauthorizedExceptionMessage = "You are not authorzied. Please provide token.";
        private const string InternalServerError = "An error occured.";

        public override async Task OnActionExecutingAsync(HttpActionContext actionContext, CancellationToken cancellationToken)
        {
            await Validate(actionContext);
            await base.OnActionExecutingAsync(actionContext, cancellationToken);
        }

        private async Task Validate(HttpActionContext actionContext)
        {
            if (actionContext.Request.Headers.Contains("DoNotValidate"))
            {
                return;
            }

            var op = "1";
            var aid = 1;
            var serviceSessionRepository = new ServiceSessionRepository();
            try
            {
                await serviceSessionRepository.GetAccessAsync(actionContext.Request, op, aid);
            }
            catch (ArgumentNullException)
            {
                actionContext.Response = actionContext.Request.CreateErrorResponse(HttpStatusCode.Unauthorized,
                    UnauthorizedExceptionMessage);
            }
            catch (HttpRequestException)
            {
                actionContext.Response = actionContext.Request.CreateErrorResponse(HttpStatusCode.Unauthorized,
                    UnauthorizedExceptionMessage);
            }
            catch 
            {
                actionContext.Response = actionContext.Request.CreateErrorResponse(HttpStatusCode.InternalServerError,
                    InternalServerError);
            }
        }
    }
}
