using System.Threading.Tasks;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Description;
using AccessControl.Repositories;

namespace AccessControl.Controllers
{
    [RoutePrefix("licenses")]
    public class LicensesController : ApiController
    {
	    private readonly ILicensesRepository _repo;

	    public LicensesController(): this(new SqlLicensesRepository(WebApiConfig.AdminStorage))
        {
        }

	    internal LicensesController(ILicensesRepository repo)
	    {
		    _repo = repo;
	    }

	    [HttpGet]
        [Route("status")]
        [ResponseType(typeof(HttpResponseMessage))]
        public async Task<IHttpActionResult> GetActiveLicenses()
        {
	        try
	        {
		        var licenses = await _repo.GetLicensesStatus(WebApiConfig.LicenseHoldTime);

		        var response = Request.CreateResponse(HttpStatusCode.OK, licenses);
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
