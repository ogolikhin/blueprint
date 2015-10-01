using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FileStore.Repositories;
using System.Net;
using System.Text;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web.Http;
using System.Web.Http.Description;

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
