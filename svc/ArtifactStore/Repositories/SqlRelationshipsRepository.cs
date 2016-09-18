using ArtifactStore.Helpers;
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
        private readonly SqlItemInfoRepository _itemInfoRepository;
        public SqlRelationshipsRepository()
            : this(new SqlConnectionWrapper(ServiceConstants.RaptorMain))
        {
        }
        internal SqlRelationshipsRepository(ISqlConnectionWrapper connectionWrapper)
        {
            ConnectionWrapper = connectionWrapper;
            _itemInfoRepository = new SqlItemInfoRepository(connectionWrapper);
        }

        private async Task<IEnumerable<LinkInfo>> GetLinkInfo(int itemId, int userId, bool addDrafts)
        {
            var parameters = new DynamicParameters();
            parameters.Add("@itemId", itemId);
            parameters.Add("@userId", userId);
            parameters.Add("@addDrafts", addDrafts);
            return await ConnectionWrapper.QueryAsync<LinkInfo>("GetRelationshipLinkInfo", parameters, commandType: CommandType.StoredProcedure);
        }

        private async Task<IEnumerable<ItemIdItemNameParentId>> GetPathInfoToRoute(int artifactId, int userId, bool addDrafts = true, int revisionId = int.MaxValue)
        {
            var parameters = new DynamicParameters();
            parameters.Add("@artifactId", artifactId);
            parameters.Add("@userId", userId);
            parameters.Add("@addDrafts", addDrafts);
            parameters.Add("@revisionId", revisionId);
            return await ConnectionWrapper.QueryAsync<ItemIdItemNameParentId>("GetPathIdsNamesToProject", parameters, commandType: CommandType.StoredProcedure);
        }



        private async Task<string> GetItemDescription (int itemId, int userId, bool addDrafts = true, int revisionId = int.MaxValue)
        {
            var parameters = new DynamicParameters();
            parameters.Add("@itemId", itemId);
            parameters.Add("@userId", userId);
            parameters.Add("@addDrafts", addDrafts);
            parameters.Add("@revisionId", revisionId);
            return (await ConnectionWrapper.QueryAsync<string>("GetItemDescription", parameters, commandType: CommandType.StoredProcedure)).SingleOrDefault();
        }

        private void PopulateRelationshipInfos(List<Relationship> relationships, IDictionary<int, ItemDetails> itemDetailsDictionary, IDictionary<int, ItemLabel> itemLabelsDictionary)
        {
            ItemDetails item, project, artifact;
            ItemLabel itemLabel;
            foreach (var relationship in relationships)
            {
                if (itemDetailsDictionary.TryGetValue(relationship.ItemId, out item))
                {
                    if (itemDetailsDictionary.TryGetValue(relationship.ProjectId, out project))
                    {
                        relationship.ProjectId = project.HolderId;
                        relationship.ProjectName = project.Name;
                    }
                    relationship.ItemName = item.Name;
                    relationship.ItemTypePrefix = item.Prefix;
                    if (itemLabelsDictionary.TryGetValue(relationship.ItemId, out itemLabel))
                    {
                        relationship.ItemLabel = itemLabel.Label;
                    }
                    relationship.PrimitiveItemTypePredefined = item.PrimitiveItemTypePredefined;
                    relationship.ArtifactName = relationship.ItemName;
                    relationship.ArtifactTypePrefix = relationship.ItemTypePrefix;
                }
                if (relationship.ItemId != relationship.ArtifactId && itemDetailsDictionary.TryGetValue(relationship.ArtifactId, out artifact)) // Sub-artifacts
                {
                    relationship.ArtifactName = artifact.Name;
                    relationship.ArtifactTypePrefix = artifact.Prefix;
                }
            }
        }
        private Relationship NewRelationship(LinkInfo link, TraceDirection traceDirection)
        {
            int artifactId = 0;
            int itemId = 0;
            if (traceDirection == TraceDirection.From)
            {
                artifactId = link.SourceArtifactId;
                itemId = link.SourceItemId;
            }
            else
            {
                artifactId = link.DestinationArtifactId;
                itemId = link.DestinationItemId;
            }
            return new Relationship
            {
                ArtifactId = artifactId,
                ItemId = itemId,
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
                var traceDirection = otherLink.SourceItemId == itemId ? TraceDirection.To : TraceDirection.From;
                otherTraceRelationships.Add(NewRelationship(otherLink, traceDirection));
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

            var itemDetailsDictionary = (await _itemInfoRepository.GetItemsDetails(userId, distinctItemIds, true, int.MaxValue)).ToDictionary(a => a.HolderId);
            var itemLabelsDictionary = (await _itemInfoRepository.GetItemsLabels(userId, distinctItemIds, true, int.MaxValue)).ToDictionary(a => a.ItemId);
            PopulateRelationshipInfos(manualTraceRelationships, itemDetailsDictionary, itemLabelsDictionary);
            PopulateRelationshipInfos(otherTraceRelationships, itemDetailsDictionary, itemLabelsDictionary);
            return new RelationshipResultSet { ManualTraces = manualTraceRelationships, OtherTraces = otherTraceRelationships };
        }

        private IEnumerable<ItemIdItemNameParentId> GetPathToProject(int artifactId, IDictionary<int, ItemIdItemNameParentId> pathInfoDictionary)
        {
            var pathToProject = new List<ItemIdItemNameParentId>();
            var itemId = artifactId;
            ItemIdItemNameParentId item;
            while (pathInfoDictionary.TryGetValue(itemId, out item) && (item != null && item.ParentId != 0))
            {
                pathToProject.Add(item);
                itemId = item.ParentId;
            }
            pathToProject.Reverse();
            return pathToProject;
        }

        public async Task<RelationshipExtendedInfo> GetRelationshipExtendedInfo(int artifactId, int userId, bool addDraft = true, int revisionId = int.MaxValue)
        {
            var pathInfoDictionary = (await GetPathInfoToRoute(artifactId, userId, addDraft, revisionId)).ToDictionary(a => a.ItemId);
            var pathToProject = GetPathToProject(artifactId, pathInfoDictionary);
            var description = (await GetItemDescription(artifactId, userId, addDraft, revisionId));
            return new RelationshipExtendedInfo { ArtifactId = artifactId, PathToProject = pathToProject, Description = description };
        }
    }
}