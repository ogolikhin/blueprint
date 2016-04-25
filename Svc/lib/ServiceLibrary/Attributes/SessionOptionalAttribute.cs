using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.Controllers;
using ServiceLibrary.Helpers;

namespace ServiceLibrary.Attributes
{
    public class SessionOptionalAttribute : SessionAttribute
    {
      
        public override async Task OnActionExecutingAsync(HttpActionContext actionContext, CancellationToken cancellationToken)
        {
            if (actionContext.Request.Headers.Contains(BlueprintSessionTokenIgnore))
            {
                return;
            }

            try
            {
                actionContext.Request.Properties[ServiceConstants.SessionProperty] = await GetAccessAsync(actionContext.Request);
            }
            catch (ArgumentNullException)
            {
            }
            catch (HttpRequestException)
            {
            }
            catch
            {
                actionContext.Response = actionContext.Request.CreateErrorResponse(HttpStatusCode.InternalServerError,
                    InternalServerErrorMessage);
            }

            await base.OnActionExecutingAsync(actionContext, cancellationToken);
        }

    }
}
