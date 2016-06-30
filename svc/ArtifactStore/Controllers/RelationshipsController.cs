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
            CheckReadPermissions(itemId, permissions, () =>
            {
                throw new HttpResponseException(HttpStatusCode.Forbidden);
            });

            foreach (var relationship in result.ManualTraces)
            {
                CheckReadPermissions(relationship.ArtifactId, permissions, () =>
                {
                    relationship.HasAccess = false;
                });
            }
            foreach (var relationship in result.OtherTraces)
            {
                CheckReadPermissions(relationship.ArtifactId, permissions, () =>
                {
                    relationship.HasAccess = false;
                });
            }

            return result;
        }
        private static void CheckReadPermissions(int itemId, Dictionary<int, RolePermissions> permissions, Action action)
        {
            if (!permissions.ContainsKey(itemId))
            {
                action();
            }
            else
            {
                RolePermissions permission = RolePermissions.None;
                permissions.TryGetValue(itemId, out permission);

                if (!permission.HasFlag(RolePermissions.Read))
                {
                    action();
                }
            }
        }
    }
}