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
    internal class ItemDetails
    {
        internal int HolderId;
        internal string Name;
        internal int PrimitiveItemTypePredefined;
        internal string Prefix;
        internal int ItemTypeId;
        internal int ProjectId;
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

        private async Task<IEnumerable<ItemDetails>> GetItemsDetailsWithProjectInfo(int userId, List<int> itemIds, bool addDrafts= true, int revisionId = int.MaxValue)
        {
            var parameters = new DynamicParameters();
            parameters.Add("@userId", userId);
            parameters.Add("@itemIds", DapperHelper.GetIntCollectionTableValueParameter(itemIds));
            parameters.Add("@addDrafts", addDrafts);
            parameters.Add("@revisionId", revisionId);
            return await ConnectionWrapper.QueryAsync<ItemDetails>("GetItemsDetailsWithProjectInfo", parameters, commandType: CommandType.StoredProcedure);
        }

        private void PopulateRelationshipInfos(List<Relationship> relationships, Dictionary<int, ItemDetails> itemDetailsDictionary, Dictionary<int, ItemDetails> projectItemDetailsDictionary)
        {
            foreach (var manualTrace in relationships)
            {
                ItemDetails item;
                ItemDetails project;
                itemDetailsDictionary.TryGetValue(manualTrace.ItemId, out item);
                if (item != null)
                {
                    projectItemDetailsDictionary.TryGetValue(item.ProjectId, out project);
                    if (project != null)
                    {
                        manualTrace.ProjectId = project.HolderId;
                        manualTrace.ProjectName = project.Name;
                    }
                    manualTrace.ItemName = item.Name;
                    manualTrace.ItemTypePrefix = item.Prefix;
                }

                if (manualTrace.ItemId != manualTrace.ArtifactId) //Not sub-artifacts
                {
                    ItemDetails artifact;
                    itemDetailsDictionary.TryGetValue(manualTrace.ArtifactId, out artifact);
                    manualTrace.ArtifactName = artifact.Name;
                    manualTrace.ArtifactTypePrefix = artifact.Prefix;
                }
                else
                {
                    manualTrace.ArtifactName = manualTrace.ItemName;
                    manualTrace.ArtifactTypePrefix = manualTrace.ItemTypePrefix;
                }
            }

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
                    ItemId = otherLink.DestinationItemId,
                    TraceDirection = TraceDirection.To,
                    Suspect = otherLink.IsSuspect,
                    TraceType = otherLink.LinkType
                });
            }
            var distinctItemIds = result.Select(a => a.SourceArtifactId)
                           .Union(result.Select(a => a.SourceItemId))
                           .Union(result.Select(a => a.DestinationArtifactId))
                           .Union(result.Select(a => a.DestinationItemId)).Distinct().ToList();
            var itemDetails = await GetItemsDetailsWithProjectInfo(userId, distinctItemIds, true, int.MaxValue);
            var itemDetailsDictionary = itemDetails.ToDictionary(a => a.HolderId);
            var distinctProjectIds = itemDetails.ToList().Select(a => a.ProjectId).Distinct().ToList();
            var projectItemDetailsDictionary = (await GetItemsDetailsWithProjectInfo(userId, distinctProjectIds, true, int.MaxValue)).ToDictionary(a=>a.HolderId);

            PopulateRelationshipInfos(manualTraceRelationships, itemDetailsDictionary, projectItemDetailsDictionary);
            PopulateRelationshipInfos(otherTraceRelationships, itemDetailsDictionary, projectItemDetailsDictionary);
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
                    ItemId = fromManualLink.DestinationItemId,
                    TraceDirection = TraceDirection.To,
                    Suspect = fromManualLink.IsSuspect,
                    TraceType = LinkType.Manual
                });
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
                    result.Add(new Relationship
                    {
                        ArtifactId = toManualLink.SourceArtifactId,
                        ItemId = toManualLink.SourceItemId,
                        TraceDirection = TraceDirection.From,
                        Suspect = toManualLink.IsSuspect,
                        TraceType = LinkType.Manual
                    });
                }
            }
            return result;
        }
    }
}