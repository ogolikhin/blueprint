using ArtifactStore.Models;
using Dapper;
using ServiceLibrary.Helpers;
using ServiceLibrary.Repositories;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using ServiceLibrary.Exceptions;
using ServiceLibrary.Models;
using ArtifactStore.Helpers;

namespace ArtifactStore.Repositories
{
    public class SqlRelationshipsRepository: IRelationshipsRepository
    {
        private readonly ISqlConnectionWrapper _connectionWrapper;
        private readonly ISqlItemInfoRepository _itemInfoRepository;

        public SqlRelationshipsRepository()
            : this(new SqlConnectionWrapper(ServiceConstants.RaptorMain))
        {
        }
        internal SqlRelationshipsRepository(ISqlConnectionWrapper connectionWrapper)
        {
            _connectionWrapper = connectionWrapper;
            _itemInfoRepository = new SqlItemInfoRepository(connectionWrapper);
        }

        internal SqlRelationshipsRepository(ISqlConnectionWrapper connectionWrapper, ISqlItemInfoRepository itemInfoRepository)
        {
            _connectionWrapper = connectionWrapper;
            _itemInfoRepository = itemInfoRepository;
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



        private async Task<string> GetItemDescription (int itemId, int userId, bool? addDrafts = true, int? revisionId = int.MaxValue)
        {
            // SP [GetItemDescription] returns last published version for deleted items when revisionId is NULL.
            var parameters = new DynamicParameters();
            parameters.Add("@itemId", itemId);
            parameters.Add("@userId", userId);
            parameters.Add("@addDrafts", addDrafts);
            parameters.Add("@revisionId", revisionId);
            return (await _connectionWrapper.QueryAsync<string>("GetItemDescription", parameters, commandType: CommandType.StoredProcedure)).SingleOrDefault();
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

        public async Task<RelationshipResultSet> GetRelationships(int artifactId, int userId, int? subArtifactId = null, bool addDrafts = true, int? versionId = null)
        {
            var revisionId = int.MaxValue;
            if (versionId.HasValue)
            {
                revisionId = await _itemInfoRepository.GetRevisionIdByVersionIndex(artifactId, versionId.Value);
            }

            if (revisionId <= 0)
            {
                throw new ResourceNotFoundException($"Version index (Id:{versionId}) is not found.", ErrorCodes.ResourceNotFound);
            }

            var itemId = subArtifactId ?? artifactId;
            var types = new List<int> { (int)LinkType.Manual,
                                        (int)LinkType.Association,
                                        (int)LinkType.ActorInheritsFrom,
                                        (int)LinkType.DocumentReference };

            var results = (await GetLinkInfo(itemId, userId, addDrafts, revisionId, types)).ToList();
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
            var description = (await GetItemDescription(itemId, userId, addDrafts, revisionId));
            return new RelationshipExtendedInfo { ArtifactId = artifactId, PathToProject = pathToProject, Description = description };
        }

        public async Task<ReviewRelationshipsResultSet> GetReviewRelationships(int artifactId, int userId, bool addDrafts = true, int? versionId = null)
        {
            var revisionId = int.MaxValue;
            if (versionId.HasValue)
            {
                revisionId = await _itemInfoRepository.GetRevisionIdByVersionIndex(artifactId, versionId.Value);
            }
            if (revisionId <= 0)
            {
                throw new ResourceNotFoundException($"Version index (Id:{versionId}) is not found.", ErrorCodes.ResourceNotFound);
            }
            var reviewType = new List<int> { (int)LinkType.ReviewPackageReference };
            var reviewLinks = (await GetLinkInfo(artifactId, userId, addDrafts, revisionId, reviewType)).ToList();
            var result = new ReviewRelationshipsResultSet { };
            if (reviewLinks != null)
            {
                var distinctReviewIds = reviewLinks.Select(a => a.SourceItemId).Distinct().ToList();
                var itemDetailsDictionary = (await _itemInfoRepository.GetItemsDetails(userId, distinctReviewIds, true, revisionId))
                    .ToDictionary(a => a.HolderId);
                var itemRawDataDictionary = (await _itemInfoRepository.GetItemsRawDataCreatedDate(userId, distinctReviewIds, true, revisionId))
                    .ToDictionary(a => a.ItemId);

                var referencedReviewArtifacts = new List<ReferencedReviewArtifact>();
                ItemRawDataCreatedDate itemRawDataCreatedDate;
                ItemDetails itemDetails;
                foreach (var reviewId in distinctReviewIds)
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