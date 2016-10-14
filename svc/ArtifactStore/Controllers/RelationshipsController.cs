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
        private readonly IRelationshipsRepository _relationshipsRepository;
        private readonly IArtifactPermissionsRepository _artifactPermissionsRepository;
        private readonly IArtifactVersionsRepository _artifactVersionsRepository;
        public override string LogSource { get; } = "ArtifactStore.Relationships";

        public RelationshipsController() : this(new SqlRelationshipsRepository(), new SqlArtifactPermissionsRepository(), new SqlArtifactVersionsRepository())
        {
        }
        public RelationshipsController(IRelationshipsRepository relationshipsRepository,
            IArtifactPermissionsRepository artifactPermissionsRepository,
            IArtifactVersionsRepository artifactVersionsRepository) : base()
        {
            _relationshipsRepository = relationshipsRepository;
            _artifactPermissionsRepository = artifactPermissionsRepository;
            _artifactVersionsRepository = artifactVersionsRepository;
        }

        [HttpGet, NoCache]
        [Route("artifacts/{artifactId:int:min(1)}/relationships"), SessionRequired]
        [ActionName("GetRelationships")]
        public async Task<RelationshipResultSet> GetRelationships(int artifactId, int? subArtifactId = null, bool addDrafts = true, int? versionId = null)
        {
            var session = Request.Properties[ServiceConstants.SessionProperty] as Session;
            if (artifactId < 1 || (subArtifactId.HasValue && subArtifactId.Value < 1))
            {
                throw new HttpResponseException(HttpStatusCode.NotFound);
            }
            var itemId = subArtifactId ?? artifactId;

            var isDeleted = await _artifactVersionsRepository.IsItemDeleted(itemId);
            var itemInfo = isDeleted && versionId != null ?
                (await _artifactVersionsRepository.GetDeletedItemInfo(itemId)) :
                (await _artifactPermissionsRepository.GetItemInfo(itemId, session.UserId, addDrafts));
            if (itemInfo == null)
                throw new HttpResponseException(HttpStatusCode.NotFound);
            if (subArtifactId.HasValue && itemInfo.ArtifactId != artifactId)
                throw new HttpResponseException(HttpStatusCode.BadRequest);

            // We do not need drafts for historical artifacts 
            var effectiveAddDraft = !versionId.HasValue && addDrafts;

            var result = await _relationshipsRepository.GetRelationships(artifactId, session.UserId, subArtifactId, effectiveAddDraft, versionId);
            var artifactIds = new List<int> { artifactId };
            artifactIds = artifactIds.Union(result.ManualTraces.Select(a=>a.ArtifactId)).Union(result.OtherTraces.Select(a => a.ArtifactId)).Distinct().ToList();
            var permissions = await _artifactPermissionsRepository.GetArtifactPermissionsInChunks(artifactIds, session.UserId);
            if (!HasPermissions(artifactId, permissions, RolePermissions.Read))
            {
                throw new HttpResponseException(HttpStatusCode.Forbidden);
            }

            ApplyRelationshipPermissions(permissions, result.ManualTraces);
            ApplyRelationshipPermissions(permissions, result.OtherTraces);

            result.CanEdit = HasPermissions(artifactId, permissions, RolePermissions.Trace) && HasPermissions(artifactId, permissions, RolePermissions.Edit);

            return result;
        }

        private static void ApplyRelationshipPermissions(Dictionary<int, RolePermissions> permissions, List<Relationship> relationships)
        {
            foreach (var relationship in relationships)
            {
                if (!HasPermissions(relationship.ArtifactId, permissions, RolePermissions.Read))
                {
                    MakeRelationshipUnauthorized(relationship);
                }

                if ((HasPermissions(relationship.ArtifactId, permissions, RolePermissions.Trace) &&
                     HasPermissions(relationship.ArtifactId, permissions, RolePermissions.Edit)) == false)
                {
                    relationship.ReadOnly = true;
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
        public async Task<RelationshipExtendedInfo> GetRelationshipDetails(int artifactId, bool addDrafts = true, int? revisionId = null)
        {
            var session = Request.Properties[ServiceConstants.SessionProperty] as Session;
            if (artifactId < 1 )
            {
                throw new HttpResponseException(HttpStatusCode.NotFound);
            }

            var isDeleted = await _artifactVersionsRepository.IsItemDeleted(artifactId);
            var artifactInfo = isDeleted && revisionId != null ?
                (await _artifactVersionsRepository.GetDeletedItemInfo(artifactId)) :
                (await _artifactPermissionsRepository.GetItemInfo(artifactId, session.UserId, addDrafts));
            if (artifactInfo == null)
                throw new HttpResponseException(HttpStatusCode.NotFound);

            var itemIds = new List<int> { artifactId };
            var permissions = await _artifactPermissionsRepository.GetArtifactPermissions(itemIds, session.UserId);
            if (!HasPermissions(artifactId, permissions, RolePermissions.Read))
            {
                throw new HttpResponseException(HttpStatusCode.Forbidden);
            }

            // We do not need drafts for historical artifacts 
            var effectiveAddDraft = !revisionId.HasValue && addDrafts;

            return await _relationshipsRepository.GetRelationshipExtendedInfo(artifactId, session.UserId, effectiveAddDraft, revisionId ?? int.MaxValue);
        }

        private static bool HasPermissions(int itemId, Dictionary<int, RolePermissions> permissions, RolePermissions permissionType)
        {
            RolePermissions permission = RolePermissions.None;
            if (permissions.TryGetValue(itemId, out permission) && permission.HasFlag(permissionType))
            {
                return true;
            }
            return false;
        }
    }
}
