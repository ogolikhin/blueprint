using System.Net;
using System.Net.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using ServiceLibrary.Helpers;

namespace ServiceLibrary.Attributes
{
    public class FeatureActivationAttribute : ActionFilterAttribute
    {
        private readonly FeatureTypes _required;

        public FeatureActivationAttribute(FeatureTypes required)
        {
            _required = required;
        }

        public override void OnActionExecuting(HttpActionContext actionContext)
        {
            var licenses = FeatureLicenseHelper.GetValidBlueprintLicenseFeatures();
            if ((licenses & _required) != _required)
            {
                //required license not found
                actionContext.Response = new HttpResponseMessage(HttpStatusCode.Forbidden);
            }
        }
    }
}
