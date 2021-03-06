﻿using ArtifactStore.Helpers;
using ArtifactStore.Models;
using Dapper;
using ServiceLibrary.Exceptions;
using ServiceLibrary.Helpers;
using ServiceLibrary.Models;
using ServiceLibrary.Repositories;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace ArtifactStore.Repositories
{
    public class SqlRelationshipsRepository : IRelationshipsRepository
    {
        private readonly ISqlConnectionWrapper _connectionWrapper;
        private readonly IItemInfoRepository _itemInfoRepository;
        private readonly IArtifactPermissionsRepository _artifactPermissionsRepository;

        public SqlRelationshipsRepository()
            : this(new SqlConnectionWrapper(ServiceConstants.RaptorMain))
        {
        }

        internal SqlRelationshipsRepository(ISqlConnectionWrapper connectionWrapper)
        {
            _connectionWrapper = connectionWrapper;
            _itemInfoRepository = new SqlItemInfoRepository(connectionWrapper);
            _artifactPermissionsRepository = new SqlArtifactPermissionsRepository(connectionWrapper);
        }

        internal SqlRelationshipsRepository(ISqlConnectionWrapper connectionWrapper, IItemInfoRepository itemInfoRepository, IArtifactPermissionsRepository artifactPermissionsRepository)
        {
            _connectionWrapper = connectionWrapper;
            _itemInfoRepository = itemInfoRepository;
            _artifactPermissionsRepository = artifactPermissionsRepository;
        }

        private async Task<IEnumerable<LinkInfo>> GetLinkInfo(int itemId, int userId, bool addDrafts, int revisionId = int.MaxValue, IEnumerable<int> linkTypes = null)
        {

            var parameters = new DynamicParameters();
            parameters.Add("@itemId", itemId);
            parameters.Add("@userId", userId);
            parameters.Add("@addDrafts", addDrafts);
            parameters.Add("@revisionId", revisionId);
            parameters.Add("@types", SqlConnectionWrapper.ToDataTable(linkTypes));
            return await _connectionWrapper.QueryAsync<LinkInfo>("GetRelationshipLinkInfo", parameters, commandType: CommandType.StoredProcedure);
        }

        private async Task<IEnumerable<ItemIdItemNameParentId>> GetPathInfoToRoute(int artifactId, int userId, bool? addDrafts, int? revisionId)
        {
            // SP [GetArtifactNavigationPath] returns last published version for deleted items only when addDrafts and revisionId are NULL.
            var parameters = new DynamicParameters();
            parameters.Add("@artifactId", artifactId);
            parameters.Add("@userId", userId);
            parameters.Add("@addDrafts", addDrafts);
            parameters.Add("@revisionId", revisionId);
            return await _connectionWrapper.QueryAsync<ItemIdItemNameParentId>("GetArtifactNavigationPath", parameters, commandType: CommandType.StoredProcedure);
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
        private Relationship ComposeRelationship(LinkInfo link, TraceDirection traceDirection)
        {
            int artifactId;
            int itemId;
            int projectId;
            if (traceDirection == TraceDirection.From)
            {
                artifactId = link.SourceArtifactId;
                itemId = link.SourceItemId;
                projectId = link.SourceProjectId;
            }
            else
            {
                artifactId = link.DestinationArtifactId;
                itemId = link.DestinationItemId;
                projectId = link.DestinationProjectId;
            }
            return new Relationship
            {
                ArtifactId = artifactId,
                ItemId = itemId,
                TraceDirection = traceDirection,
                Suspect = link.IsSuspect,
                TraceType = link.LinkType,
                ProjectId = projectId
            };
        }

        private List<Relationship> GetManualTraceRelationships(List<LinkInfo> manualLinks, int itemId)
        {
            var fromManualLinks = manualLinks.Where(a => a.SourceItemId == itemId).ToList();
            var toManualLinks = manualLinks.Where(a => a.DestinationItemId == itemId).ToList();
            var result = new List<Relationship>();

            foreach (var fromManualLink in fromManualLinks)
            {
                result.Add(ComposeRelationship(fromManualLink, TraceDirection.To));
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
                    result.Add(ComposeRelationship(toManualLink, TraceDirection.From));
                }
            }
            return result;
        }

        private List<Relationship> GetReuseTraceRelationships(List<LinkInfo> links, int itemId)
        {
            var result = new List<Relationship>();
            foreach (var link in links)
            {
                if (result.All(i => i.ItemId != link.SourceItemId))
                {
                    result.Add(ComposeRelationship(link, TraceDirection.TwoWay));
                }
            }
            return result;
        }

        private IEnumerable<LinkInfo> UpdateReuseLinks(IEnumerable<LinkInfo> links, int itemId)
        {
            var result = links.Where(link => link.SourceArtifactId == itemId);
            return result;
        }


        public async Task<RelationshipResultSet> GetRelationships(
            int artifactId,
            int userId,
            int? subArtifactId = null,
            bool addDrafts = true,
            bool allLinks = false,
            int? versionId = null,
            int? baselineId = null)
        {
            var revisionId = await _itemInfoRepository.GetRevisionId(artifactId, userId, versionId, baselineId);
            var itemId = subArtifactId ?? artifactId;
            var types = new List<int> { (int)LinkType.Manual,
                                        (int)LinkType.Association,
                                        (int)LinkType.ActorInheritsFrom,
                                        (int)LinkType.DocumentReference };
            if (allLinks) {
                types.AddRange(new[] { (int)LinkType.ParentChild, (int)LinkType.Reuse });
            }

            if (baselineId != null)
            {
                addDrafts = false;
            }

            var results = (await GetLinkInfo(itemId, userId, addDrafts, revisionId, types)).ToList();
            var manualLinks = results.Where(a => a.LinkType == LinkType.Manual).ToList();
            // filter out Parent/Child links between artifact and its subartifact if exist
            var excludeParentChildLinks = results.Where(link =>
                link.LinkType == LinkType.ParentChild &&
                                ((link.SourceArtifactId != link.SourceItemId || link.DestinationArtifactId != link.DestinationItemId)) || ////internal links
                                (link.SourceItemId == link.SourceProjectId)) ////to artifact's project
                                .ToList();
            // get reuse links to to modify them separaratly.
            var reuseLinks = results.Where(a => a.LinkType == LinkType.Reuse).ToList();
            // get collection of other links exept exclude parent/child links and reuse links
            var otherLinks = results.Except(excludeParentChildLinks).Except(reuseLinks).Where(link => link.LinkType != LinkType.Manual).ToList();
            // modify reuse links by combining matching pais (source match destination on other) and add them back to coolection of otherlinks
            otherLinks.AddRange(UpdateReuseLinks(reuseLinks, itemId));

            var manualTraceRelationships = GetManualTraceRelationships(manualLinks, itemId);
            var otherTraceRelationships = new List<Relationship>();

            foreach (var otherLink in otherLinks)
            {
                var traceDirection = otherLink.SourceItemId == itemId ? TraceDirection.To : TraceDirection.From;
                Relationship relationship = null;
                if (otherLink.LinkType == LinkType.ActorInheritsFrom)
                {
                    var itemInfo = await _artifactPermissionsRepository.GetItemInfo(otherLink.DestinationArtifactId, userId, addDrafts, revisionId);
                    if (itemInfo != null)
                    {
                        relationship = ComposeRelationship(otherLink, traceDirection);
                    }
                }
                else if (otherLink.LinkType == LinkType.Reuse)
                {
                    traceDirection = TraceDirection.TwoWay;
                    var itemInfo = await _artifactPermissionsRepository.GetItemInfo(otherLink.DestinationArtifactId, userId, addDrafts, revisionId);
                    if (itemInfo != null)
                    {
                        relationship = ComposeRelationship(otherLink, traceDirection);
                    }
                }
                else
                {
                    relationship = ComposeRelationship(otherLink, traceDirection);
                }
                if (relationship != null)
                {
                    otherTraceRelationships.Add(relationship);
                }
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

            var itemDetailsDictionary = (await _itemInfoRepository.GetItemsDetails(userId, distinctItemIds, true, revisionId)).ToDictionary(a => a.HolderId);
            var itemLabelsDictionary = (await _itemInfoRepository.GetItemsLabels(userId, distinctItemIds, true, revisionId)).ToDictionary(a => a.ItemId);
            PopulateRelationshipInfos(manualTraceRelationships, itemDetailsDictionary, itemLabelsDictionary);
            PopulateRelationshipInfos(otherTraceRelationships, itemDetailsDictionary, itemLabelsDictionary);
            return new RelationshipResultSet
            {
                RevisionId = revisionId,
                ManualTraces = manualTraceRelationships,
                OtherTraces = otherTraceRelationships
            };
        }

        private static IEnumerable<ItemIdItemNameParentId> GetPathToProject(int artifactId, IDictionary<int, ItemIdItemNameParentId> pathInfoDictionary)
        {
            var pathToProject = new List<ItemIdItemNameParentId>();
            int? itemId = artifactId;
            ItemIdItemNameParentId item;
            // We return the project, as well, removed check for the ParentId.
            while (itemId.HasValue && pathInfoDictionary.TryGetValue(itemId.Value, out item))
            {
                pathToProject.Add(item);
                itemId = item.ParentId;
            }
            pathToProject.Reverse();
            return pathToProject;
        }
        public async Task<RelationshipExtendedInfo> GetRelationshipExtendedInfo(int artifactId, int userId, int? subArtifactId = null, bool isDeleted = false)
        {
            bool? addDrafts = null;
            int? revisionId = null;
            if (!isDeleted)
            {
                addDrafts = true;
                revisionId = int.MaxValue;
            }
            var itemId = subArtifactId.HasValue ? subArtifactId.Value : artifactId;
            var pathInfoDictionary = (await GetPathInfoToRoute(artifactId, userId, addDrafts, revisionId)).ToDictionary(a => a.ItemId);
            if (pathInfoDictionary.Keys.Count == 0)
                throw new ResourceNotFoundException($"Artifact in revision {revisionId} does not exist.", ErrorCodes.ResourceNotFound);
            var pathToProject = GetPathToProject(artifactId, pathInfoDictionary);
            var description = (await _itemInfoRepository.GetItemDescription(itemId, userId, addDrafts, revisionId));
            return new RelationshipExtendedInfo { ArtifactId = artifactId, PathToProject = pathToProject, Description = description };
        }

        public async Task<ReviewRelationshipsResultSet> GetReviewRelationships(int artifactId, int userId, bool addDrafts = true, int? versionId = null)
        {
            var revisionId = await _itemInfoRepository.GetRevisionId(artifactId, userId, versionId);
            var reviewType = new List<int> { (int)LinkType.ReviewPackageReference };
            var reviewLinks = (await GetLinkInfo(artifactId, userId, addDrafts, revisionId, reviewType)).ToList();
            var result = new ReviewRelationshipsResultSet { };
            if (reviewLinks != null)
            {
                var distinctReviewIds = reviewLinks.Select(a => a.SourceItemId).Distinct().ToList();
                var reviewIdsWithAccess = new List<int>();
                var reviewPermissions = await _artifactPermissionsRepository.GetArtifactPermissions(distinctReviewIds, userId);
                foreach (var reviewId in distinctReviewIds)
                {
                    if (SqlArtifactPermissionsRepository.HasPermissions(reviewId, reviewPermissions, RolePermissions.Read))
                    {
                        reviewIdsWithAccess.Add(reviewId);
                    }
                }

                var itemDetailsDictionary = (await _itemInfoRepository.GetItemsDetails(userId, reviewIdsWithAccess, true, revisionId))
                    .ToDictionary(a => a.HolderId);
                var itemRawDataDictionary = (await _itemInfoRepository.GetItemsRawDataCreatedDate(userId, reviewIdsWithAccess, true, revisionId))
                    .ToDictionary(a => a.ItemId);
                var referencedReviewArtifacts = new List<ReferencedReviewArtifact>();
                ItemRawDataCreatedDate itemRawDataCreatedDate;
                ItemDetails itemDetails;
                foreach (var reviewId in reviewIdsWithAccess)
                {
                    if ((itemRawDataDictionary.TryGetValue(reviewId, out itemRawDataCreatedDate)) && (itemDetailsDictionary.TryGetValue(reviewId, out itemDetails)))
                    {
                        var status = ReviewRawDataHelper.ExtractReviewStatus(itemRawDataCreatedDate.RawData);
                        referencedReviewArtifacts.Add(new ReferencedReviewArtifact
                        {
                            ItemId = reviewId,
                            Status = status,
                            CreatedDate = itemRawDataCreatedDate.CreatedDateTime,
                            ItemName = itemDetails.Name,
                            ItemTypePrefix = itemDetails.Prefix
                        });
                    }
                }
                result.ReviewArtifacts = referencedReviewArtifacts;
            }
            return result;
        }
    }
}