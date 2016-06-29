using ArtifactStore.Models;
using Dapper;
using ServiceLibrary.Helpers;
using ServiceLibrary.Repositories;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace ArtifactStore.Repositories
{
    public class SqlRelationshipsRepository: IRelationshipsRepository
    {
        internal readonly ISqlConnectionWrapper ConnectionWrapper;
        public SqlRelationshipsRepository()
            : this(new SqlConnectionWrapper(ServiceConstants.RaptorMain))
        {
        }
        internal SqlRelationshipsRepository(ISqlConnectionWrapper connectionWrapper)
        {
            ConnectionWrapper = connectionWrapper;
        }

        private async Task<IEnumerable<LinkInfo>> GetLinkInfo(int itemId, int userId, bool addDrafts)
        {
            var parameters = new DynamicParameters();
            parameters.Add("@itemId", itemId);
            parameters.Add("@userId", userId);
            parameters.Add("@addDrafts", addDrafts);
            return await ConnectionWrapper.QueryAsync<LinkInfo>("GetRelationshipLinkInfo", parameters, commandType: CommandType.StoredProcedure);
        }

        public async Task<RelationshipResultSet> GetRelationships(int itemId, int userId, bool addDrafts = true)
        {
            var result = (await GetLinkInfo(itemId, userId, addDrafts)).ToList();
            var manualLinks = result.Where(a => a.LinkType == LinkType.Manual).ToList();
            var otherLinks = result.Where(a => a.LinkType != LinkType.Manual).ToList();
            var manualTraceRelationships = GetManualTraceRelationships(manualLinks, itemId);
            var otherTraceRelationships = new List<Relationship>();

            foreach (var otherLink in otherLinks)
            {
                otherTraceRelationships.Add(new Relationship
                {
                    ArtifactId = otherLink.DestinationArtifactId,
                    itemId = otherLink.DestinationItemId,
                    TraceDirection = TraceDirection.To,
                    Suspect = otherLink.IsSuspect,
                    TraceType = TraceType.Manual
                });
            }
            return new RelationshipResultSet { ManualTraces = manualTraceRelationships, OtherTraces = otherTraceRelationships };
        }

        public List<Relationship> GetManualTraceRelationships(List<LinkInfo> manualLinks, int itemId)
        {
            var fromManualLinks = manualLinks.Where(a => a.SourceItemId == itemId).ToList();
            var toManualLinks = manualLinks.Where(a => a.DestinationItemId == itemId).ToList();

            var result = new List<Relationship>();

            foreach (var fromManualLink in fromManualLinks)
            {
                result.Add(new Relationship
                {
                    ArtifactId = fromManualLink.DestinationArtifactId,
                    itemId = fromManualLink.DestinationItemId,
                    TraceDirection = TraceDirection.To,
                    Suspect = fromManualLink.IsSuspect,
                    TraceType = TraceType.Manual
                });
            }
            foreach (var toManualLink in toManualLinks)
            {
                if (fromManualLinks.Any(a => a.DestinationItemId == toManualLink.SourceItemId))
                {
                    var BidirectionalRelationship = result.SingleOrDefault(a => a.itemId == toManualLink.SourceItemId);
                    BidirectionalRelationship.TraceDirection = TraceDirection.TwoWay;
                }
                else
                {
                    result.Add(new Relationship
                    {
                        ArtifactId = toManualLink.SourceArtifactId,
                        itemId = toManualLink.SourceItemId,
                        TraceDirection = TraceDirection.From,
                        Suspect = toManualLink.IsSuspect,
                        TraceType = TraceType.Manual
                    });
                }
            }
            return result;
        }
    }
}