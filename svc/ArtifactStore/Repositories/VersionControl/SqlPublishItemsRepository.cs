using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using ArtifactStore.Models.Reuse;
using ArtifactStore.Models.VersionControl;
using ServiceLibrary.Models.Enums;
using ServiceLibrary.Models.VersionControl;

namespace ArtifactStore.Repositories.VersionControl
{
    public class SqlPublishItemsRepository : SqlPublishRepository, IPublishRepository
    {
        protected override string MarkAsLatestStoredProcedureName { get; } = "MarkAsLatestItemVersions";
        protected override string DeleteVersionsStoredProcedureName { get; } = "RemoveItemVersions";
        protected override string CloseVersionsStoredProcedureName { get; } = "CloseItemVersions";
        protected override string GetDraftAndLatestStoredProcedureName { get; } = "GetDraftAndLatestItemVersions";

        public async Task Execute(int revisionId, PublishParameters parameters, PublishEnvironment environment, IDbTransaction transaction = null)
        {
            var items = await GetDraftAndLatest<SqlDraftAndLatestItem>(parameters.UserId, parameters.AffectedArtifactIds, transaction);

            if (items.Count == 0)
            {
                return;
            }

            var hierarchyValidator = new ArtifactsHierarchyValidator(this);

            var deleteVersionsIds = new HashSet<int>();
            var closeVersionIds = new HashSet<int>();
            var markAsLatestVersionIds = new HashSet<int>();

            foreach (var item in items)
            {
                if (item.DraftDeleted)
                {
                    deleteVersionsIds.Add(item.DraftVersionId);

                    if (item.LatestVersionId.HasValue)
                    {
                        closeVersionIds.Add(item.LatestVersionId.Value);
                        RegisterItemModification(environment.SensitivityCollector, item);
                    }

                    environment.AddAffectedArtifact(item.ArtifactId);
                }
                else
                {
                    if (IsChanged(item))
                    {
                        markAsLatestVersionIds.Add(item.DraftVersionId);

                        if (item.LatestVersionId.HasValue)
                        {
                            closeVersionIds.Add(item.LatestVersionId.Value);
                        }

                        hierarchyValidator.ScheduleReparentAndReorderArtifactsCheck(item, parameters.AffectedArtifactIds);

                        environment.AddAffectedArtifact(item.ArtifactId);

                        RegisterItemModification(environment.SensitivityCollector, item);
                    }
                    else
                    {
                        deleteVersionsIds.Add(item.DraftVersionId);
                    }
                }
            }

            await hierarchyValidator.CheckAndFix(environment, transaction);

            await MarkAsLatest(markAsLatestVersionIds, environment.RevisionId, transaction);
            await DeleteVersions(deleteVersionsIds, transaction);
            await CloseVersions(closeVersionIds, environment.RevisionId, transaction);
        }

        private void RegisterItemModification(ReuseSensitivityCollector sensitivityCollector, SqlDraftAndLatestItem item)
        {
            if (IsSubArtifactChange(item))
            {
                sensitivityCollector.RegisterArtifactModification(item.ArtifactId,
                    ItemTypeReuseTemplateSetting.Subartifacts);
            }
        }

        private bool IsSubArtifactChange(SingleArtifactData item)
        {
            return item.ArtifactId != item.ItemId;
        }

        private bool IsChanged(SqlDraftAndLatestItem item)
        {
            return item.LatestVersionId.HasValue == false // new item
                || item.DraftItemTypeId != item.LatestItemTypeId
                || item.DraftParentId != item.LatestParentId
                || item.DraftOrderIndex != item.LatestOrderIndex
                || item.DraftDeleted
                || item.DraftName != item.LatestName;
        }
    }

    

    public class SqlDraftAndLatestItem : SingleArtifactData
    {
        public string DraftName { get; set; }
        public string LatestName { get; set; }

        public int DraftParentId { get; set; }
        public int? LatestParentId { get; set; }

        public int DraftProjectId { get; set; }
        public int? LatestProjectId { get; set; }

        public double DraftOrderIndex { get; set; }
        public double? LatestOrderIndex { get; set; }

        public int DraftItemTypeId { get; set; }
        public int? LatestItemTypeId { get; set; }
    }

    public abstract class SingleArtifactData : BaseVersionData
    {
        public int ItemId { get; set; }
        public int ArtifactId { get; set; }
    }

    public abstract class BaseVersionData
    {
        public int DraftVersionId { get; set; }
        public int? LatestVersionId { get; set; }

        public bool DraftDeleted { get; set; }
    }

}