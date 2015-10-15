using System.Web.Http;

namespace AccessControl.Controllers
{
	[RoutePrefix("status")]
	public class StatusController : StatusControl.Controllers.StatusController
	{
		public StatusController() : base(WebApiConfig.AdminStoreDatabase, "GetStatus")
		{
		}
	}
}
