using System.Web.Http;
using FileStore.Repositories;

namespace FileStore.Controllers
{
	[RoutePrefix("status")]
	public class StatusController : StatusControl.Controllers.StatusController
	{
        public StatusController() : this(new ConfigRepository())
        {

        }

        internal StatusController(IConfigRepository configRepository) : base(configRepository.FileStoreDatabase, "GetStatus")
        {
        }
	}
}
