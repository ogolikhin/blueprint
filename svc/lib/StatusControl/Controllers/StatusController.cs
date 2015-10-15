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
using System.Threading;

namespace StatusControl.Controllers
{
	[RoutePrefix("status")]
	public class StatusController : ApiController
	{
		public static ManualResetEventSlim Ready { get; private set; }

		static StatusController()
		{
			Ready = new ManualResetEventSlim(false);
		}

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
				return Ready.IsSet ?
					(IHttpActionResult)Ok() :
					new System.Web.Http.Results.StatusCodeResult(System.Net.HttpStatusCode.ServiceUnavailable, Request);
			}
			catch
			{
				return InternalServerError();
			}
		}
	}
}
