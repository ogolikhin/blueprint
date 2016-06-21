using System;
using System.Collections.Generic;
using System.Linq;
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

            if (subArtifactId.HasValue)
            {
                var itemInfoDictionary = (await ArtifactPermissionsRepository.GetItemsInfos(subArtifactId.Value, session.UserId, addDrafts)).ToDictionary(a=>a.ItemId);
                ArtifactItemProject subArtifactInfo = null;
                itemInfoDictionary.TryGetValue(subArtifactId.Value, out subArtifactInfo);
                if (subArtifactInfo == null || subArtifactInfo.ArtifactId != artifactId)
                {
                    throw new HttpResponseException(System.Net.HttpStatusCode.BadRequest);
                }
            }

            var result = await AttachmentsRepository.GetAttachmentsAndDocumentReferences(artifactId, session.UserId, subArtifactId, addDrafts);

            var artifactIds = new List<int> { artifactId };
            foreach (var documentReference in result.DocumentReferences)
            {
                artifactIds.Add(documentReference.VersionArtifactId);
            }
            
            var permissions = await ArtifactPermissionsRepository.GetArtifactPermissions(artifactIds, session.UserId);

            CheckReadPermissions(artifactId, permissions, () =>
            {
                throw new HttpResponseException(HttpStatusCode.Forbidden);
            });

            var docRef = result.DocumentReferences.ToList();
            foreach (var documentReference in docRef)
            {
                CheckReadPermissions(documentReference.VersionArtifactId, permissions, () =>
                {
                    result.DocumentReferences.Remove(documentReference);
                });
            }

            return result;
        }

        private static void CheckReadPermissions(int artifactId, Dictionary<int, RolePermissions> permissions, Action action)
        {
            if (!permissions.ContainsKey(artifactId))
            {
                action();
            }
            else
            {
                RolePermissions permission = RolePermissions.None;
                permissions.TryGetValue(artifactId, out permission);

                if (!permission.HasFlag(RolePermissions.Read))
                {
                    action();
                }
            }
        }
    }
}