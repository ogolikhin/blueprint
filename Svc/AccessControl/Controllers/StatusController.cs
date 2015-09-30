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

namespace AccessCotrol.Controllers
{
	[RoutePrefix("status")]
	public class StatusController : ApiController
	{
		private readonly ISessionsRepository _sessionRepo;

		public StatusController() : this(new SqlSessionsRepository())
		{
		}

		internal StatusController(ISessionsRepository sessionRepo)
		{
			_sessionRepo = sessionRepo;
		}

		[HttpGet]
		[Route("")]
		[ResponseType(typeof(HttpResponseMessage))]
		public async Task<IHttpActionResult> GetStatus()
		{
			try
			{
				await _sessionRepo.GetStatus();
				return Ok();
			}
			catch
			{
				return InternalServerError();
			}
		}
	}
}
