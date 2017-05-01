using System.Web.Http;
using ServiceLibrary.Helpers;
using ServiceLibrary.Models;

namespace ServiceLibrary.Helpers
{
    public class BaseApiController : ApiController
    {
        protected int SessionUserId
        {
            get
            {
                var session = Request.Properties[ServiceConstants.SessionProperty] as Session;
                return session.UserId;
            }
        }
    }
}