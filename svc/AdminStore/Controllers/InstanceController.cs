using System.Web.Http;
using AdminStore.Filters;
using AdminStore.Models;
using AdminStore.Repositories;
using ServiceLibrary.Attributes;
using ServiceLibrary.Repositories.ConfigControl;
using System.Collections.Generic;
using System.Web.Http.Description;
using System.Threading.Tasks;

namespace AdminStore.Controllers
{
    [RoutePrefix("instance")]
    [BaseExceptionFilter]
    public class InstanceController : ApiController, ILoggable 
    {
        internal readonly ISqlInstanceRepository _instanceRepository;
        public IServiceLogRepository Log { get; }

        public string LogSource { get; } = "AdminStore.Instance";

        public InstanceController() : this(new SqlInstanceRepository(), new ServiceLogRepository())
        {
        }

        public InstanceController(ISqlInstanceRepository instanceRepository, IServiceLogRepository log)
        {
            _instanceRepository = instanceRepository;
            Log = log;
        }

        /// <summary>
        /// Get Instance Folder
        /// </summary>
        /// <remarks>
        /// Returns an instance folder for the specified id.
        /// </remarks>
        /// <response code="200">OK.</response>
        /// <response code="400">Bad Request. The session token is missing or malformed.</response>
        /// <response code="401">Unauthorized. The session token is invalid.</response>
        /// <response code="404">Not found. An instance folder for the specified id is not found, does not exist or is deleted.</response>
        /// <response code="500">Internal Server Error. An error occurred.</response>
        [HttpGet, NoCache]
        [Route("folders/{id:int:min(1)}"), SessionRequired]
        [ResponseType(typeof(InstanceItem))]
        [ActionName("GetInstanceFolder")]
        public async Task<InstanceItem> GetInstanceFolderAsync(int id)
        {
            return await _instanceRepository.GetInstanceFolderAsync(id);
        }

        /// <summary>
        /// Get Instance Folder
        /// </summary>
        /// <remarks>
        /// Returns an instance folder for the specified id.
        /// </remarks>
        /// <response code="200">OK.</response>
        /// <response code="400">Bad Request. The session token is missing or malformed.</response>
        /// <response code="401">Unauthorized. The session token is invalid.</response>
        /// <response code="404">Not found. An instance folder for the specified id is not found, does not exist or is deleted.</response>
        /// <response code="500">Internal Server Error. An error occurred.</response>
        [HttpGet, NoCache]
        [Route("folders/{id:int:min(1)}/children"), SessionRequired]
        [ResponseType(typeof(List<InstanceItem>))]
        [ActionName("GetInstanceFolderChildren")]
        public async Task<List<InstanceItem>> GetInstanceFolderChildrenAsync(int id)
        {
            return await _instanceRepository.GetInstanceFolderChildrenAsync(id);
        }
    }
}