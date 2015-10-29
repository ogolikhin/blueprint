using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using System.Net;
using System.Linq;
using System.Net.Http.Headers;
using ConfigControl.Repositories;
using System;
using ConfigControl.Models;
using System.Text;
using System.Collections.Generic;

namespace ConfigControl.Controllers
{
	[RoutePrefix("settings")]
	public class ConfigController : ApiController
	{
		private readonly IConfigRepository _configRepo;

		public ConfigController() : this(new SqlConfigRepository())
      {
		}

		internal ConfigController(IConfigRepository configRepo)
		{
			_configRepo = configRepo;
		}

		[HttpGet]
		[Route("{restricted}")]
		[ResponseType(typeof(HttpResponseMessage))]
		public async Task<IHttpActionResult> GetConfig(bool restricted)
		{
			try
			{
				var settings = await _configRepo.GetSettings(restricted);
				var response = Request.CreateResponse(HttpStatusCode.OK, settings);
				response.Headers.Add("Cache-Control", "no-cache, no-store, must-revalidate"); // HTTP 1.1.
				response.Headers.Add("Pragma", "no-cache"); // HTTP 1.0.
				return ResponseMessage(response);
			}
			catch
			{
				return InternalServerError();
			}
		}
	}
}
