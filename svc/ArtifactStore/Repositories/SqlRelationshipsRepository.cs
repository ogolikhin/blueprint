﻿using ArtifactStore.Models;
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

        private void PopulateRelationshipInfos(List<Relationship> relationships, Dictionary<int, ItemDetails> itemDetailsDictionary)
        {
            foreach (var relationship in relationships)
            {
                ItemDetails item, project, artifact;
                if (itemDetailsDictionary.TryGetValue(relationship.ItemId, out item))
                {
                    if (itemDetailsDictionary.TryGetValue(relationship.ProjectId, out project))
                    {
                        relationship.ProjectId = project.HolderId;
                        relationship.ProjectName = project.Name;
                    }
                    relationship.ItemName = item.Name;
                    relationship.ItemTypePrefix = item.Prefix;
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
            if (traceDirection == TraceDirection.To || traceDirection == TraceDirection.TwoWay)
            {
                artifactId = link.DestinationArtifactId;
                itemId = link.DestinationItemId;
            }
            else
            {
                artifactId = link.SourceArtifactId;
                itemId = link.SourceItemId;
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
            var description = (await GetItemDescription(artifactId, userId, addDraft, revisionId));
            return new RelationshipExtendedInfo { ArtifactId = artifactId, PathToProject = pathToProject, Description = description };
        }
    }
}