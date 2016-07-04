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
    public class ItemIdItemNameParentId
    {
        public int ItemId { get; set; }
        public string ItemName { get; set; }
        public int ParentId { get; set; }
    }

    public class RelationshipExtendedInfo
    {
        public int ArtifactId { get; set; }
        public string Description { get; set; }
        public List<ItemIdItemNameParentId> PathToProject { get; set; }
    }

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

        private async Task<IEnumerable<ItemDetails>> GetItemsDetails(int userId, IEnumerable<int> itemIds, bool addDrafts= true, int revisionId = int.MaxValue)
        {
            var parameters = new DynamicParameters();
            parameters.Add("@userId", userId);
            parameters.Add("@itemIds", DapperHelper.GetIntCollectionTableValueParameter(itemIds));
            parameters.Add("@addDrafts", addDrafts);
            parameters.Add("@revisionId", revisionId);
            return await ConnectionWrapper.QueryAsync<ItemDetails>("GetItemsDetails", parameters, commandType: CommandType.StoredProcedure);
        }

        private async Task<IEnumerable<ItemIdItemNameParentId>> GetPathInfoToRoute(int artifactId, int userId, bool addDrafts = true, int revisionId = int.MaxValue)
        {
            var parameters = new DynamicParameters();
            parameters.Add("@artifactId", userId);
            parameters.Add("@userId", userId);
            parameters.Add("@addDrafts", addDrafts);
            parameters.Add("@revisionId", revisionId);
            return await ConnectionWrapper.QueryAsync<ItemIdItemNameParentId>("GetPathIdsToRoute", parameters, commandType: CommandType.StoredProcedure);
        }

        private void PopulateRelationshipInfos(List<Relationship> relationships, Dictionary<int, ItemDetails> itemDetailsDictionary)
        {
            foreach (var relationship in relationships)
            {
                ItemDetails item;
                ItemDetails project;
                if (itemDetailsDictionary.TryGetValue(relationship.ItemId, out item))
                {
                    if (itemDetailsDictionary.TryGetValue(relationship.ProjectId, out project))
                    {
                        relationship.ProjectId = project.HolderId;
                        relationship.ProjectName = project.Name;
                    }
                    relationship.ItemName = item.Name;
                    relationship.ItemTypePrefix = item.Prefix;
                }

                if (relationship.ItemId != relationship.ArtifactId) //Not sub-artifacts
                {
                    ItemDetails artifact;
                    itemDetailsDictionary.TryGetValue(relationship.ArtifactId, out artifact);
                    relationship.ArtifactName = artifact.Name;
                    relationship.ArtifactTypePrefix = artifact.Prefix;
                }
                else
                {
                    relationship.ArtifactName = relationship.ItemName;
                    relationship.ArtifactTypePrefix = relationship.ItemTypePrefix;
                }
            }

        }
        private Relationship NewRelationship(LinkInfo link, TraceDirection traceDirection)
        {
            return new Relationship
            {
                ArtifactId = link.DestinationArtifactId,
                ItemId = link.DestinationItemId,
                TraceDirection = traceDirection,
                Suspect = link.IsSuspect,
                TraceType = link.LinkType,
                ProjectId = link.DestinationProjectId
            };
        }
        private List<Relationship> GetManualTraceRelationships(List<LinkInfo> manualLinks, int itemId)
        {
            var fromManualLinks = manualLinks.Where(a => a.SourceItemId == itemId).ToList();
            var toManualLinks = manualLinks.Where(a => a.DestinationItemId == itemId).ToList();
            var result = new List<Relationship>();

            foreach (var fromManualLink in fromManualLinks)
            {
                result.Add(NewRelationship(fromManualLink, TraceDirection.To));
            }
            foreach (var toManualLink in toManualLinks)
            {
                if (fromManualLinks.Any(a => a.DestinationItemId == toManualLink.SourceItemId))
                {
                    var BidirectionalRelationship = result.SingleOrDefault(a => a.ItemId == toManualLink.SourceItemId);
                    BidirectionalRelationship.TraceDirection = TraceDirection.TwoWay;
                }
                else
                {
                    result.Add(NewRelationship(toManualLink, TraceDirection.From));
                }
            }
            return result;
        }

        public async Task<RelationshipResultSet> GetRelationships(int itemId, int userId, bool addDrafts = true)
        {
            var results = (await GetLinkInfo(itemId, userId, addDrafts)).ToList();
            var manualLinks = results.Where(a => a.LinkType == LinkType.Manual).ToList();
            var otherLinks = results.Where(a => a.LinkType != LinkType.Manual).ToList();
            var manualTraceRelationships = GetManualTraceRelationships(manualLinks, itemId);
            var otherTraceRelationships = new List<Relationship>();

            foreach (var otherLink in otherLinks)
            {
                otherTraceRelationships.Add(NewRelationship(otherLink, TraceDirection.To));
            }

            var distinctItemIds = new HashSet<int>();
            foreach (var result in results)
            {
                distinctItemIds.Add(result.SourceArtifactId);
                distinctItemIds.Add(result.DestinationArtifactId);
                distinctItemIds.Add(result.SourceItemId);
                distinctItemIds.Add(result.DestinationItemId);
                distinctItemIds.Add(result.SourceProjectId);
                distinctItemIds.Add(result.DestinationProjectId);
            }

            var itemDetails = await GetItemsDetails(userId, distinctItemIds, true, int.MaxValue);
            var itemDetailsDictionary = itemDetails.ToDictionary(a => a.HolderId);
            PopulateRelationshipInfos(manualTraceRelationships, itemDetailsDictionary);
            PopulateRelationshipInfos(otherTraceRelationships, itemDetailsDictionary);
            return new RelationshipResultSet { ManualTraces = manualTraceRelationships, OtherTraces = otherTraceRelationships };
        }

        public async Task<RelationshipExtendedInfo> GetRelationshipExtendedInfo(int artifactId, int userId, bool addDraft = true, int revisionId = int.MaxValue)
        {
            var pathToProject = (await GetPathInfoToRoute(artifactId, userId, addDraft, revisionId)).ToList();
            pathToProject.Reverse();
            return new RelationshipExtendedInfo { ArtifactId = artifactId, PathToProject = pathToProject };
        }
    }
}