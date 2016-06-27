using ArtifactStore.Models;
using ArtifactStore.Repositories;
using ServiceLibrary.Attributes;
using ServiceLibrary.Filters;
using ServiceLibrary.Helpers;
using ServiceLibrary.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;

namespace ArtifactStore.Controllers
{
    [ApiControllerJsonConfig]
    [BaseExceptionFilter]
    public class RelationshipsController : LoggableApiController
    {
        //internal readonly ISqlAttachmentsRepository AttachmentsRepository;
        internal readonly IArtifactPermissionsRepository ArtifactPermissionsRepository;
        public override string LogSource { get; } = "ArtifactStore.Relationships";

        [HttpGet, NoCache]
        [Route("artifacts/{artifactId:int:min(1)}/relationships"), SessionRequired]
        [ActionName("GetRelationships")]
        public async Task<RelationshipResultSet> GetRelationships(int artifactId, int? subArtifactId = null, bool addDrafts = true)
        {
            var session = Request.Properties[ServiceConstants.SessionProperty] as Session;
            if (artifactId < 1 || (subArtifactId.HasValue && subArtifactId.Value < 1))
            {
                throw new HttpResponseException(System.Net.HttpStatusCode.BadRequest);
            }

            if (subArtifactId.HasValue)
            {
                var itemInfo = (await ArtifactPermissionsRepository.GetItemInfo(subArtifactId.Value, session.UserId, addDrafts));
                if (itemInfo == null || itemInfo.ArtifactId != artifactId)
                {
                    throw new HttpResponseException(System.Net.HttpStatusCode.BadRequest);
                }
            }
            var artifactIds = new List<int> { artifactId };

            var permissions = await ArtifactPermissionsRepository.GetArtifactPermissions(artifactIds, session.UserId);

            return await Task.FromResult(new RelationshipResultSet());
        }

    }
}