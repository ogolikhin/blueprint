using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Http;
using ServiceLibrary.Attributes;
using ServiceLibrary.Filters;
using ArtifactStore.Models;
using ArtifactStore.Repositories;
using ServiceLibrary.Helpers;
using ServiceLibrary.Models;
using ServiceLibrary.Repositories.ConfigControl;
using System.Net.Http;
using System.Net;

namespace ArtifactStore.Controllers
{
    [ApiControllerJsonConfig]
    [BaseExceptionFilter]
    public class AttachmentsController : LoggableApiController
    {       
        internal readonly ISqlAttachmentsRepository AttachmentsRepository;
        internal readonly IArtifactPermissionsRepository ArtifactPermissionsRepository;
        public override string LogSource { get; } = "ArtifactStore.Attachments";

        public AttachmentsController() : this(new SqlAttachmentsRepository(), new SqlArtifactPermissionsRepository())
        {
        }
        public AttachmentsController(ISqlAttachmentsRepository attachmentsRepository, IArtifactPermissionsRepository artifactPermissionsRepository) : base()
        {
            AttachmentsRepository = attachmentsRepository;
            ArtifactPermissionsRepository = artifactPermissionsRepository;
        }

        /// <summary>
        /// Get artifact history
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <response code="200">OK.</response>
        /// <response code="400">Bad Request. The session token or parameters are missing or malformed</response>
        /// <response code="401">Unauthorized. The session token is invalid.</response>
        /// <response code="403">Forbidden. The user does not have permissions for the project.</response>
        /// <response code="500">Internal Server Error. An error occurred.</response>
        [HttpGet, NoCache]
        [Route("artifacts/{artifactId:int:min(1)}/attachment"), SessionRequired]
        [ActionName("GetAttachmentsAndDocumentReferences")]
        public async Task<FilesInfo> GetAttachmentsAndDocumentReferences(int artifactId, int? subArtifactId = null, bool addDrafts = true)
        {
            var session = Request.Properties[ServiceConstants.SessionProperty] as Session;
            if (artifactId < 1 || (subArtifactId.HasValue && subArtifactId.Value < 1))
            {
                throw new HttpResponseException(System.Net.HttpStatusCode.BadRequest);
            }            

            var artifactIds = new List<int> { artifactId };
            var permissions = await ArtifactPermissionsRepository.GetArtifactPermissions(artifactIds, session.UserId);

            if (!permissions.ContainsKey(artifactId))
            {
                throw new HttpResponseException(HttpStatusCode.Forbidden);
            }
            else
            {
                RolePermissions permission = RolePermissions.None;
                permissions.TryGetValue(artifactId, out permission);

                if (!permission.HasFlag(RolePermissions.Read))
                {
                    throw new HttpResponseException(HttpStatusCode.Forbidden);
                }
            }
            var result = await AttachmentsRepository.GetAttachmentsAndDocumentReferences(artifactId, session.UserId, subArtifactId, addDrafts);
            return result;
        }

    }
}