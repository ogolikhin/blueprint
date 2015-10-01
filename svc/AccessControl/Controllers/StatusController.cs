using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AccessControl.Repositories;
using System.Net;
using System.Text;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web.Http;
using System.Web.Http.Description;

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
