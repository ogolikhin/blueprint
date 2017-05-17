using System.Web.Http;
using ServiceLibrary.Helpers;
using ServiceLibrary.Models;

namespace ServiceLibrary.Controllers
{
    public abstract class BaseApiController : ApiController
    {
        public Session Session
        {
            get
            {
                object value;
                if (!Request.Properties.TryGetValue(ServiceConstants.SessionProperty, out value))
                {
                    return null;
                }

                return value as Session;
            }
        }
    }
}
