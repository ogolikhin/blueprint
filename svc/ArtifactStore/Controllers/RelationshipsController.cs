using ArtifactStore.Models;
using ArtifactStore.Repositories;
using ServiceLibrary.Attributes;
using ServiceLibrary.Filters;
using ServiceLibrary.Helpers;
using ServiceLibrary.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
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
            var result = await RelationshipsRepository.GetRelationships(itemId, session.UserId, addDrafts);
            var itemIds = new List<int> { itemId };
            itemIds = itemIds.Union(result.ManualTraces.Select(a=>a.ArtifactId)).Union(result.OtherTraces.Select(a => a.ArtifactId)).Distinct().ToList();
            var permissions = await ArtifactPermissionsRepository.GetArtifactPermissionsInChunks(itemIds, session.UserId);
            if (!HasReadPermissions(itemId, permissions))
            {
                throw new HttpResponseException(HttpStatusCode.Forbidden);
            }

            foreach (var relationship in result.ManualTraces)
            {
                if (!HasReadPermissions(relationship.ArtifactId, permissions))
                {
                    relationship.HasAccess = false;
                }
            }
            foreach (var relationship in result.OtherTraces)
            {
                if (!HasReadPermissions(relationship.ArtifactId, permissions))
                {
                    relationship.HasAccess = false;
                }
            }

            return result;
        }

        [HttpGet, NoCache]
        [Route("artifacts/{artifactId:int:min(1)}/relationshipdetails"), SessionRequired]
        [ActionName("GetRelationshipDetails")]
        public async Task<RelationshipExtendedInfo> GetRelationshipDetails(int artifactId, bool addDrafts = true)
        {
            var session = Request.Properties[ServiceConstants.SessionProperty] as Session;
            if (artifactId < 1 )
            {
                throw new HttpResponseException(System.Net.HttpStatusCode.BadRequest);
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