using ArtifactStore.Models;
using ArtifactStore.Repositories;
using ServiceLibrary.Attributes;
using ServiceLibrary.Filters;
using ServiceLibrary.Helpers;
using ServiceLibrary.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Http;

namespace ArtifactStore.Controllers
{
    [ApiControllerJsonConfig]
    [BaseExceptionFilter]
    public class RelationshipsController : LoggableApiController
    {
        internal readonly IRelationshipsRepository RelationshipsRepository;
        internal readonly IArtifactPermissionsRepository ArtifactPermissionsRepository;
        public override string LogSource { get; } = "ArtifactStore.Relationships";

        public RelationshipsController() : this(new SqlRelationshipsRepository(), new SqlArtifactPermissionsRepository())
        {
        }
        public RelationshipsController(IRelationshipsRepository relationshipsRepository, IArtifactPermissionsRepository artifactPermissionsRepository) : base()
        {
            RelationshipsRepository = relationshipsRepository;
            ArtifactPermissionsRepository = artifactPermissionsRepository;
        }

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
            var itemId = artifactId;
            if (subArtifactId.HasValue)
            {
                var itemInfo = (await ArtifactPermissionsRepository.GetItemInfo(subArtifactId.Value, session.UserId, addDrafts));
                if (itemInfo == null || itemInfo.ArtifactId != artifactId)
                {
                    throw new HttpResponseException(System.Net.HttpStatusCode.BadRequest);
                }
                itemId = subArtifactId.Value;
            }
            var artifactIds = new List<int> { artifactId };

            var permissions = await ArtifactPermissionsRepository.GetArtifactPermissions(artifactIds, session.UserId);

            var result = await RelationshipsRepository.GetRelationships(itemId, session.UserId, addDrafts);
            return result;
        }

    }
}