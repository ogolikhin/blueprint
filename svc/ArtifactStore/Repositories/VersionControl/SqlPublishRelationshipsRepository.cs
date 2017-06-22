﻿using System.Data;
﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ArtifactStore.Helpers;
using ArtifactStore.Models;
using ArtifactStore.Services.VersionControl;
using Dapper;
using ServiceLibrary.Repositories;
using ServiceLibrary.Models.VersionControl;

namespace ArtifactStore.Repositories.VersionControl
{
    public class SqlPublishRelationshipsRepository : SqlPublishRepository, IPublishRepository
    {
        protected class DraftAndLatestLink : BaseVersionData
        {
            public LinkType Type { get; set; }

            public int Item1Id { get; set; }
            public int Item2Id { get; set; }

            public int Artifact1Id { get; set; }
            public int Artifact2Id { get; set; }

            public int DraftProject1Id { get; set; }
            public int? LatestProject1Id { get; set; }
            public int DraftProject2Id { get; set; }
            public int? LatestProject2Id { get; set; }

            public bool DraftSuspect { get; set; }
            public bool? LatestSuspect { get; set; }

            public double DraftOrderIndex { get; set; }
            public double? LatestOrderIndex { get; set; }
        }

        public async Task Execute(int revisionId, PublishParameters parameters, PublishEnvironment environment, IDbTransaction transaction = null)
        {
            //await Task.Run(() =>
            //{
                var artifactIds = parameters.ArtifactIds.ToHashSet();
                var draftAndLatestLinks = await GetDraftAndLatestLinks(artifactIds, parameters.UserId, transaction);

                if (!draftAndLatestLinks.Any())
                {
                    return;
                }

                var deleteLinkVersionsIds = new HashSet<int>();
                var closeLinkVersionIds = new HashSet<int>();
                var markAsLatestLinkVersionIds = new HashSet<int>();

                foreach (var link in await FilterLinksThatCannotBePublished(draftAndLatestLinks, artifactIds))
                {
                    if (link.DraftDeleted)
                    {
                        deleteLinkVersionsIds.Add(link.DraftVersionId);

                        if (link.LatestVersionId.HasValue)
                        {
                            closeLinkVersionIds.Add(link.LatestVersionId.Value);

                            MarkArtifactsAsAffectedIfRequired(link, artifactIds, environment);

                            if (link.Type == LinkType.Reuse)
                            {
                                DeleteReuseMapping(link.Item1Id, link.Item2Id, environment.RevisionId, transaction);
                            }

                            RegisterLinkModification(environment, link);
                        }
                    }
                    else
                    {
                        if (IsChanged(link))
                        {
                            markAsLatestLinkVersionIds.Add(link.DraftVersionId);

                            if (link.LatestVersionId.HasValue)
                            {
                                closeLinkVersionIds.Add(link.LatestVersionId.Value);
                            }

                            MarkArtifactsAsAffectedIfRequired(link, artifactIds, environment);
                            RegisterLinkModification(environment, link);
                        }
                        else
                        {
                            // For unchanged link - delete the draft
                            deleteLinkVersionsIds.Add(link.DraftVersionId);
                        }
                    }
                }

                DeleteLinkVersions(deleteLinkVersionsIds, transaction);
                CloseLinkVersions(closeLinkVersionIds, environment.RevisionId, transaction);
                MarkAsLatest(markAsLatestLinkVersionIds, environment.RevisionId, transaction);
            //});
        }

        private async void MarkAsLatest(HashSet<int> markAsLatestLinkVersionIds, int revisionId, IDbTransaction transaction)
        {
            if (markAsLatestLinkVersionIds.Count == 0)
            {
                return;
            }

            var prm = new DynamicParameters();
            prm.Add("@revisionId", revisionId);
            prm.Add("@versionIds", SqlConnectionWrapper.ToDataTable(markAsLatestLinkVersionIds));
            //int updatedRowsCount;
            if (transaction == null)
            {
                //updatedRowsCount = 
                await ConnectionWrapper.ExecuteAsync("MarkAsLatestLinkVersions", prm, commandType: CommandType.StoredProcedure);
            }
            else
            {
                //updatedRowsCount = 
                await transaction.Connection.ExecuteAsync("MarkAsLatestLinkVersions", prm, commandType: CommandType.StoredProcedure);
            }
            //Log.Assert(updatedRowsCount == markAsLatestLinkVersionIds.Count, "Publish: Some links not marked as Latest");
        }

        private async void CloseLinkVersions(HashSet<int> closeLinkVersionIds, int revisionId, IDbTransaction transaction)
        {
            if (closeLinkVersionIds.Count == 0)
            {
                return;
            }

            var prm = new DynamicParameters();
            prm.Add("@revisionId", revisionId);
            prm.Add("@versionIds", SqlConnectionWrapper.ToDataTable(closeLinkVersionIds));
            //int updatedRowsCount;
            if (transaction == null)
            {
                //updatedRowsCount = 
                    await ConnectionWrapper.ExecuteAsync("CloseLinkVersions", prm, commandType: CommandType.StoredProcedure);
            }
            else
            {
                //updatedRowsCount = 
                    await transaction.Connection.ExecuteAsync("CloseLinkVersions", prm, commandType: CommandType.StoredProcedure);
            }
            
            //Log.Assert(updatedRowsCount == closeLinkVersionIds.Count, "Publish: Some links are not closed");
        }

        private async void DeleteLinkVersions(HashSet<int> deleteLinkVersionsIds, IDbTransaction transaction)
        {
            if (deleteLinkVersionsIds.Count == 0)
            {
                return;
            }
            var prm = new DynamicParameters();
            prm.Add("@versionIds", SqlConnectionWrapper.ToDataTable(deleteLinkVersionsIds));
            if (transaction == null)
            {
                await
                    ConnectionWrapper.ExecuteAsync("RemoveLinkVersions", prm, commandType: CommandType.StoredProcedure);
            }
            else
            {
                await
                    transaction.Connection.ExecuteAsync("RemoveLinkVersions", prm, commandType: CommandType.StoredProcedure);
            }

        }

        // Was in AReuse in Raptor solution
        private async void DeleteReuseMapping(int artifactIdA, int artifactIdB, int revisionId, IDbTransaction transaction)
        {
            int artifactId1 = Math.Min(artifactIdA, artifactIdB);
            int artifactId2 = Math.Max(artifactIdA, artifactIdB);
            var prm = new DynamicParameters();
            prm.Add("@revisionId", revisionId);
            prm.Add("@artifactId1", artifactId1);
            prm.Add("@artifactId2", artifactId2);
            string sqlString = @"
UPDATE dbo.ReusedItems
SET EndRevision = @revisionId - 1
WHERE Artifact1Id = @artifactId1
	AND Artifact2Id = @artifactId2
	AND EndRevision = 2147483647";

            if (transaction == null)
            {
                await ConnectionWrapper.ExecuteAsync(sqlString, prm, commandType: CommandType.Text);
            }
            else
            {

                await transaction.Connection.ExecuteAsync(sqlString, prm, commandType: CommandType.Text);
            }

        }

        private async Task<ICollection<DraftAndLatestLink>> GetDraftAndLatestLinks(HashSet<int> artifactIds, int userId, IDbTransaction transaction = null)
        {
            var prm = new DynamicParameters();
            prm.Add("@userId", userId);
            prm.Add("@artifactIds", SqlConnectionWrapper.ToDataTable(artifactIds));

            if (transaction == null)
            {
                return (await ConnectionWrapper.QueryAsync<DraftAndLatestLink>(
                    "GetDraftAndLatestLinks", prm, commandType: CommandType.StoredProcedure)).ToList();
            }
            return (await transaction.Connection.QueryAsync<DraftAndLatestLink>(
                "GetDraftAndLatestLinks", prm, commandType: CommandType.StoredProcedure)).ToList();
        }

        // Was in ItemsRepo in Raptor solution
        private async Task<ISet<int>> GetLiveItemsOnly(IEnumerable<int> artifactIds)
        {
            var prm = new DynamicParameters();
            prm.Add("@itemIds", SqlConnectionWrapper.ToDataTable(artifactIds));
            return (await ConnectionWrapper.QueryAsync<int>(
                "GetLiveItems", prm, commandType: CommandType.StoredProcedure)).ToHashSet();
        }

        private void MarkArtifactsAsAffectedIfRequired(DraftAndLatestLink link, HashSet<int> artifactIds, PublishEnvironment env)
        {
            // For manual traces and reuse links we adding history for live artifacts on both ends
            if (link.Type == LinkType.Manual || link.Type == LinkType.Reuse)
            {
                if (!env.IsArtifactDeleted(link.Artifact1Id))
                {
                    env.AddAffectedArtifact(link.Artifact1Id);
                }

                if (!env.IsArtifactDeleted(link.Artifact2Id))
                {
                    env.AddAffectedArtifact(link.Artifact2Id);
                }
            }
            else
            {
                if (artifactIds.Contains(link.Artifact1Id))
                {
                    env.AddAffectedArtifact(link.Artifact1Id);
                }
            }
        }

        private async Task<ICollection<DraftAndLatestLink>> FilterLinksThatCannotBePublished(
            ICollection<DraftAndLatestLink> links,
            ISet<int> artifactIds)
        {
            var linkDependency = new Dictionary<DraftAndLatestLink, int>();
            var itemsToVerify = new HashSet<int>();

            #region Collect links with dependencies
            foreach (var link in links)
            {
                /*
                 * Need to check new Manual/Reuse links between difSferent artifacts
                 */
                if (link.DraftDeleted == false && link.LatestVersionId.HasValue == false
                    && link.Artifact1Id != link.Artifact2Id
                    && (link.Type == LinkType.Manual || link.Type == LinkType.Reuse))
                {
                    var isArtifact1InPublishScope = artifactIds.Contains(link.Artifact1Id);
                    var isArtifact2InPublishScope = artifactIds.Contains(link.Artifact2Id);

                    if (isArtifact1InPublishScope && isArtifact2InPublishScope)
                        continue; // everything will be published

                    int dependentItemId = isArtifact1InPublishScope
                        ? link.Item2Id
                        : link.Item1Id;

                    linkDependency.Add(link, dependentItemId);
                    itemsToVerify.Add(dependentItemId);
                }
            }
            #endregion

            var publishedItems = await GetLiveItemsOnly(itemsToVerify);//_itemsRepo.GetLiveItemsOnly(itemsToVerify);

            var filteredLinks = new List<DraftAndLatestLink>();
            foreach (var link in links)
            {
                int dependentItemId;
                if (linkDependency.TryGetValue(link, out dependentItemId) && !publishedItems.Contains(dependentItemId))
                {
                    continue; // don't publish this link
                }

                filteredLinks.Add(link);
            }
            return filteredLinks;
        }

        private bool IsArtifactInPublishScope(PublishEnvironment env, int artifactId)
        {
            return env.ArtifactStates.ContainsKey(artifactId) && !env.IsArtifactDeleted(artifactId);
        }

        private void RegisterLinkModification(PublishEnvironment env, DraftAndLatestLink link)
        {
            // Parent/child links changes should be covered in ItemsRepo for sub-artifacts and ignored for artifacts
            if (link.Type == LinkType.ParentChild)
            {
                return;
            }

            var sensitivityCollector = env.SensitivityCollector;

            if (link.Type != LinkType.Manual && link.Type != LinkType.Reuse)
            {
                if (link.Type == LinkType.DocumentReference
                    && link.Artifact1Id == link.Item1Id && IsArtifactInPublishScope(env, link.Item1Id)) // document reference from artifact
                {
                    sensitivityCollector.RegisterArtifactModification(link.Item1Id, ItemTypeReuseTemplateSetting.DocumentReferences);
                }

                if (link.Type == LinkType.ActorInheritsFrom
                    && link.Artifact1Id == link.Item1Id && IsArtifactInPublishScope(env, link.Item1Id)) // actor inheritance from artifact
                {
                    sensitivityCollector.RegisterArtifactModification(link.Item1Id, ItemTypeReuseTemplateSetting.BaseActor);
                }

                if (link.Artifact1Id != link.Item1Id // Sub-Artifact
                    && IsArtifactInPublishScope(env, link.Artifact1Id))
                {
                    sensitivityCollector.RegisterArtifactModification(link.Artifact1Id, ItemTypeReuseTemplateSetting.Subartifacts);
                }

                return;
            }

            if (link.Type == LinkType.Manual)
            {
                if (!env.IsArtifactDeleted(link.Artifact1Id))
                {
                    var modification = link.Artifact1Id == link.Item1Id
                        ? ItemTypeReuseTemplateSetting.Relationships
                        : ItemTypeReuseTemplateSetting.Subartifacts;
                    sensitivityCollector.RegisterArtifactModification(link.Artifact1Id, modification);
                }

                if (!env.IsArtifactDeleted(link.Artifact2Id))
                {
                    var modification = link.Artifact2Id == link.Item2Id
                        ? ItemTypeReuseTemplateSetting.Relationships
                        : ItemTypeReuseTemplateSetting.Subartifacts;
                    sensitivityCollector.RegisterArtifactModification(link.Artifact2Id, modification);
                }
            }
        }

        private bool IsChanged(DraftAndLatestLink link)
        {
            return link.LatestVersionId.HasValue == false // new link
                || link.DraftProject1Id != link.LatestProject1Id
                || link.DraftProject2Id != link.LatestProject2Id
                || link.DraftSuspect != link.LatestSuspect
                || link.DraftOrderIndex != link.LatestOrderIndex
                || link.DraftDeleted;
        }
    }
}