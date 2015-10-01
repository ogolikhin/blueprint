using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Net;
using System.Text;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web.Http;
using System.Web.Http.Description;
using StatusControl.Repositories;


namespace StatusControl.Controllers
{
	[RoutePrefix("status")]
	public class StatusController : ApiController
	{
		private readonly IStatusRepository _statusRepo;

		public StatusController(string cxn, string cmd) : this(new SqlStatusRepository(cxn, cmd))
		{
		}

		internal StatusController(IStatusRepository statusRepo)
		{
			_statusRepo = statusRepo;
		}

		[HttpGet]
		[Route("")]
		[ResponseType(typeof(HttpResponseMessage))]
		public async Task<IHttpActionResult> GetStatus()
		{
			try
			{
				await _statusRepo.GetStatus();
				return Ok();
			}
			catch
			{
				return InternalServerError();
			}
		}
	}
}
