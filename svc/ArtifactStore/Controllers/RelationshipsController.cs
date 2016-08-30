using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;
using ArtifactStore.Models;
using ArtifactStore.Repositories;
using ServiceLibrary.Attributes;
using ServiceLibrary.Helpers;
using ServiceLibrary.Models;

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
                throw new HttpResponseException(System.Net.HttpStatusCode.NotFound);
            }
            var itemId = subArtifactId.HasValue ? subArtifactId.Value : artifactId;
            var itemInfo = (await ArtifactPermissionsRepository.GetItemInfo(itemId, session.UserId, addDrafts));
            if (itemInfo == null || (subArtifactId.HasValue && itemInfo.ArtifactId != artifactId))
            {
                throw new HttpResponseException(System.Net.HttpStatusCode.NotFound);
            }

            var result = await RelationshipsRepository.GetRelationships(itemId, session.UserId, addDrafts);
            var itemIds = new List<int> { itemId };
            itemIds = itemIds.Union(result.ManualTraces.Select(a=>a.ArtifactId)).Union(result.OtherTraces.Select(a => a.ArtifactId)).Distinct().ToList();
            var permissions = await ArtifactPermissionsRepository.GetArtifactPermissionsInChunks(itemIds, session.UserId);
            if (!HasReadPermissions(itemId, permissions))
            {
                throw new HttpResponseException(HttpStatusCode.Forbidden);
            }

            ApplyRelationshipPermissions(permissions, result.ManualTraces);
            ApplyRelationshipPermissions(permissions, result.OtherTraces);            

            return result;
        }

        private static void ApplyRelationshipPermissions(Dictionary<int, RolePermissions> permissions, List<Relationship> relationships)
        {
            foreach (var relationship in relationships)
            {
                if (!HasReadPermissions(relationship.ArtifactId, permissions))
                {
                    MakeRelationshipUnauthorized(relationship);
                }
            }
        }

        private static void MakeRelationshipUnauthorized(Relationship relationship)
        {            
            relationship.HasAccess = false;
            relationship.ArtifactName = null;
            relationship.ItemLabel = null;
            relationship.ItemName = null;
        }

        [HttpGet, NoCache]
        [Route("artifacts/{artifactId:int:min(1)}/relationshipdetails"), SessionRequired]
        [ActionName("GetRelationshipDetails")]
        public async Task<RelationshipExtendedInfo> GetRelationshipDetails(int artifactId, bool addDrafts = true)
        {
            var session = Request.Properties[ServiceConstants.SessionProperty] as Session;
            if (artifactId < 1 )
            {
                throw new HttpResponseException(System.Net.HttpStatusCode.NotFound);
            }
            var artifactInfo = (await ArtifactPermissionsRepository.GetItemInfo(artifactId, session.UserId, addDrafts));
            if (artifactInfo == null)
            {
                throw new HttpResponseException(System.Net.HttpStatusCode.NotFound);
            }

            var itemIds = new List<int> { artifactId };
            var permissions = await ArtifactPermissionsRepository.GetArtifactPermissions(itemIds, session.UserId);
            if (!HasReadPermissions(artifactId, permissions))
            {
                throw new HttpResponseException(HttpStatusCode.Forbidden);
            }
            return await RelationshipsRepository.GetRelationshipExtendedInfo(artifactId, session.UserId, addDrafts);
        }

        private static bool HasReadPermissions(int itemId, Dictionary<int, RolePermissions> permissions)
        {
            RolePermissions permission = RolePermissions.None;
            if (permissions.TryGetValue(itemId, out permission) && permission.HasFlag(RolePermissions.Read))
            {
                return true;
            }
            return false;
        }
    }
}
