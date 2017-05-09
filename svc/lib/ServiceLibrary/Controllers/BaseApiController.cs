using System.Web.Http;
using ServiceLibrary.Helpers;
using ServiceLibrary.Models;

namespace ServiceLibrary.Controllers
{
    public class BaseApiController : ApiController
    {
        protected Session Session => Request.Properties[ServiceConstants.SessionProperty] as Session;
    }
}