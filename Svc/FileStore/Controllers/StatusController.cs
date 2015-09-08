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
	public class StatusController : ApiController
	{
		private readonly IFilesRepository _fr;

		public StatusController() : this(new SqlFilesRepository())
		{
		}

		internal StatusController(IFilesRepository fr)
		{
			_fr = fr;
		}

		[HttpGet]
		[Route("")]
		[ResponseType(typeof(HttpResponseMessage))]
		public async Task<IHttpActionResult> GetStatus()
		{
			try
			{
				await _fr.GetStatus();
				return Ok();
			}
			catch
			{
				return InternalServerError();
			}
		}
	}
}
