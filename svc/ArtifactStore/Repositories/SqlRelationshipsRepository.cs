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
using System;
using System.Text.RegularExpressions;
using System.Globalization;

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

        private async Task<IEnumerable<LinkInfo>> GetLinkInfo(int itemId, int userId, bool addDrafts, int revisionId = int.MaxValue, List<int> linkTypes = null)
        {

            var parameters = new DynamicParameters();
            parameters.Add("@itemId", itemId);
            parameters.Add("@userId", userId);
            parameters.Add("@addDrafts", addDrafts);
            parameters.Add("@revisionId", revisionId);
            parameters.Add("@types", SqlConnectionWrapper.ToDataTable(linkTypes, "Int32Collection", "Int32Value"));
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
                var itemRawDataDictionary = (await _itemInfoRepository.GetItemsRawDataCreatedDate(userId, distinctReviewIds, true, revisionId))
                    .ToDictionary(a => a.ItemId);

                var referencedReviewArtifacts = new List<ReferencedReviewArtifact>();
                ItemRawDataCreatedDate itemRawDataCreatedDate;
                foreach (var reviewId in distinctReviewIds)
                {
                    if (itemRawDataDictionary.TryGetValue(reviewId, out itemRawDataCreatedDate))
                    {
                        var status = ReviewRawDataHelper.ExtractReviewStatus(itemRawDataCreatedDate.RawData);
                        var statusString = "Draft";
                        if (status == 1)
                        {
                            statusString = "Active";
                        } else if (status == 2)
                        {
                            statusString = "Closed";
                        }
                        referencedReviewArtifacts.Add(new ReferencedReviewArtifact
                        {
                            itemId = reviewId,
                            status = statusString,
                            createdDate = itemRawDataCreatedDate.CreatedDateTime
                        });
                    }
                }
                result.reviewArtifacts = referencedReviewArtifacts;
            }
            return result;
        }

        internal static class ReviewRawDataHelper
        {
            public static IEnumerable<int> ExtractReviewReviewers(string rawData)
            {
                List<int> reviewers = new List<int>();
                if (!string.IsNullOrWhiteSpace(rawData))
                {
                    var matches = Regex.Matches(rawData, "<UserId[^>]*>(.+?)</UserId\\s*>", RegexOptions.IgnoreCase);
                    foreach (Match match in matches)
                    {
                        if (match.Groups.Count > 1)
                        {
                            reviewers.Add(Convert.ToInt32(match.Groups[1].Value, new CultureInfo("en-CA", true)));
                        }
                    }
                }
                return reviewers;
            }

            public static int ExtractReviewStatus(string rawData)
            {
                int status = 0;
                if (!string.IsNullOrWhiteSpace(rawData))
                {
                    var matches = Regex.Matches(rawData, "<Status[^>]*>(.+?)</Status\\s*>", RegexOptions.IgnoreCase);
                    if (matches.Count > 0 && matches[0].Groups.Count > 1)
                    {
                        if (string.Compare(matches[0].Groups[1].Value, "Active", StringComparison.OrdinalIgnoreCase) == 0)
                        {
                            status = 1;
                        }
                        else if (string.Compare(matches[0].Groups[1].Value, "Closed", StringComparison.OrdinalIgnoreCase) == 0)
                        {
                            status = 2;
                        }
                    }
                }
                return status;
            }

            public static DateTime? ExtractReviewEndDate(string rawData)
            {
                DateTime? endDate = null;
                if (!string.IsNullOrWhiteSpace(rawData))
                {
                    var match = Regex.Match(rawData, "<EndDate[^>]*>(.+?)</EndDate\\s*>", RegexOptions.IgnoreCase);
                    if (match.Success)
                    {
                        string dateStr = match.Groups[1].Value;
                        DateTime date;
                        var successfulParse = DateTime.TryParse(dateStr, out date);

                        if (successfulParse)
                        {
                            endDate = date;
                        }
                    }
                }
                return endDate;
            }
        }
    }
}