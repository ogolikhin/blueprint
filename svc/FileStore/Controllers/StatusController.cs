using System.Web.Http;

namespace FileStore.Controllers
{
	[RoutePrefix("status")]
	public class StatusController : StatusControl.Controllers.StatusController
	{
		public StatusController() : base(WebApiConfig.FileStoreDatabase, "GetStatus")
		{
		}
	}
}
