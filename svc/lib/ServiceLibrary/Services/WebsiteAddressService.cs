using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
