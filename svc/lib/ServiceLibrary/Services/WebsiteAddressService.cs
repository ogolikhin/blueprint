using System;
using System.Web;

namespace ServiceLibrary.Services
{
    public class WebsiteAddressService : IWebsiteAddressService
    {
        public string GetWebsiteAddress()
        {
            return HttpContext.Current.Request.Url.GetLeftPart(UriPartial.Authority);
        }
    }
}
