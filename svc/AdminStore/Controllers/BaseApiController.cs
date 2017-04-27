using System.Web.Http;
using ServiceLibrary.Helpers;
using ServiceLibrary.Models;

namespace AdminStore.Controllers
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