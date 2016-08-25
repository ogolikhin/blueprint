using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using AdminStore.Models;
using AdminStore.Repositories;
using ServiceLibrary.Attributes;
using ServiceLibrary.Helpers;
using ServiceLibrary.Models;
using ServiceLibrary.Repositories.ConfigControl;

namespace AdminStore.Controllers
{
    [ApiControllerJsonConfig]
    [RoutePrefix("instance")]
    [BaseExceptionFilter]
    public class InstanceController : LoggableApiController
    {
        internal readonly ISqlInstanceRepository _instanceRepository;

        public override string LogSource { get; } = "AdminStore.Instance";

        public InstanceController() : this(new SqlInstanceRepository())
        {
        }

        public InstanceController(ISqlInstanceRepository instanceRepository) : base()
        {
            _instanceRepository = instanceRepository;
        }

        public InstanceController(ISqlInstanceRepository instanceRepository, IServiceLogRepository log) : base(log)
        {
            _instanceRepository = instanceRepository;
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
            var session = Request.Properties[ServiceConstants.SessionProperty] as Session;
            return await _instanceRepository.GetInstanceFolderAsync(id, session.UserId);
        }

        /// <summary>
        /// Get Instance Folder Children
        /// </summary>
        /// <remarks>
        /// Returns child instance folders and live projects that the user has the read permission.
        /// If an instance folder for the specified id is not found, the empty collection is returned.
        /// </remarks>
        /// <response code="200">OK.</response>
        /// <response code="400">Bad Request. The session token is missing or malformed.</response>
        /// <response code="401">Unauthorized. The session token is invalid.</response>
        /// <response code="500">Internal Server Error. An error occurred.</response>
        [HttpGet, NoCache]
        [Route("folders/{id:int:min(1)}/children"), SessionRequired]
        [ResponseType(typeof(List<InstanceItem>))]
        [ActionName("GetInstanceFolderChildren")]
        public async Task<List<InstanceItem>> GetInstanceFolderChildrenAsync(int id)
        {
            var session = Request.Properties[ServiceConstants.SessionProperty] as Session;
            return await _instanceRepository.GetInstanceFolderChildrenAsync(id, session.UserId);
        }

        /// <summary>
        /// Get Project
        /// </summary>
        /// <remarks>
        /// Returns an instance folder for the specified id.
        /// </remarks>
        /// <response code="200">OK.</response>
        /// <response code="400">Bad Request. The session token is missing or malformed.</response>
        /// <response code="401">Unauthorized. The session token is invalid.</response>
        /// <response code="403">Forbidden. The user does not have permissions for the project.</response>
        /// <response code="404">Not found. A project for the specified id is not found, does not exist or is deleted.</response>
        /// <response code="500">Internal Server Error. An error occurred.</response>
        [HttpGet, NoCache]
        [Route("projects/{id:int:min(1)}"), SessionRequired]
        [ResponseType(typeof(InstanceItem))]
        [ActionName("GetInstanceProject")]
        public async Task<InstanceItem> GetInstanceProjectAsync(int id)
        {
            var session = Request.Properties[ServiceConstants.SessionProperty] as Session;
            return await _instanceRepository.GetInstanceProjectAsync(id, session.UserId);
        }
    }
}
