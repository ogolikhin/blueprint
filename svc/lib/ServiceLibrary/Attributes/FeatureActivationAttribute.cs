using System.Net;
using System.Net.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using ServiceLibrary.Helpers;
using ServiceLibrary.Models.Enums;

namespace ServiceLibrary.Attributes
{
    public class FeatureActivationAttribute : ActionFilterAttribute
    {
        private readonly FeatureTypes _requiredFeatureTypes;
        private readonly IFeatureLicenseHelper _featureLicenseHelper;

        public FeatureActivationAttribute(FeatureTypes requiredFeatureTypes) : this(requiredFeatureTypes, FeatureLicenseHelper.Instance)
        {
        }

        internal FeatureActivationAttribute(FeatureTypes requiredFeatureTypes, IFeatureLicenseHelper featureLicenseHelper)
        {
            _requiredFeatureTypes = requiredFeatureTypes;
            _featureLicenseHelper = featureLicenseHelper;
        }

        public override void OnActionExecuting(HttpActionContext actionContext)
        {
            var licenses = _featureLicenseHelper.GetValidBlueprintLicenseFeatures();
            if ((licenses & _requiredFeatureTypes) != _requiredFeatureTypes)
            {
                //required license not found
                actionContext.Response = new HttpResponseMessage(HttpStatusCode.Forbidden);
            }
        }
    }
}
