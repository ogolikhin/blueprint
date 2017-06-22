using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using ArtifactStore.Helpers;
using ServiceLibrary.Helpers;

namespace ArtifactStore.Repositories.VersionControl
{
    public class ArtifactsHierarchyValidator
    {
        private readonly IDictionary<int, ICollection<SqlDraftAndLatestItem>> _checkParents = new Dictionary<int, ICollection<SqlDraftAndLatestItem>>();
        private readonly IDictionary<SqlDraftAndLatestItem, int> _checkOrderIndex = new Dictionary<SqlDraftAndLatestItem, int>();
        private readonly SqlPublishRepository _publishRepository;

        public ArtifactsHierarchyValidator(SqlPublishRepository publishRepository)
        {
            _publishRepository = publishRepository;
        }

        public void ScheduleReparentAndReorderArtifactsCheck(SqlDraftAndLatestItem item, ISet<int> artifactIds)
        {
            if (!IsArtifact(item))
                return;

            // need to verify parent
            if (item.DraftParentId != item.DraftProjectId
                && !artifactIds.Contains(item.DraftParentId)
                && (IsNew(item) || IsParentChanged(item))
               )
            {
                ICollection<SqlDraftAndLatestItem> dependencies;
                if (!_checkParents.TryGetValue(item.DraftParentId, out dependencies))
                {
                    dependencies = new List<SqlDraftAndLatestItem>();
                    _checkParents.Add(item.DraftParentId, dependencies);
                }
                dependencies.Add(item);
            }

            if (item.DraftOrderIndex != -1.0
                 && (IsNew(item) || IsOrderIndexChanged(item))
                )
            {
                _checkOrderIndex.Add(item, item.DraftParentId);
            }
        }

        public async Task CheckAndFix(PublishEnvironment env, IDbTransaction transaction)
        {
            if (_checkParents.Count != 0)
            {
                var parentsToCheck = _checkParents.Keys;
                var notLiveArtifacts = new List<int>();

                var deletedArtifacts = env.DeletedArtifactIds;
                if (deletedArtifacts != null && deletedArtifacts.Count != 0)
                {
                    notLiveArtifacts.AddRange(parentsToCheck.Intersect(deletedArtifacts));
                }

                var mayBeAlive = parentsToCheck.Except(notLiveArtifacts).ToHashSet();
                mayBeAlive.ExceptWith(await _publishRepository.GetLiveItemsOnly(mayBeAlive, transaction));

                notLiveArtifacts.AddRange(mayBeAlive);

                foreach (var parentId in notLiveArtifacts)
                {
                    ICollection<SqlDraftAndLatestItem> items;
                    if (_checkParents.TryGetValue(parentId, out items))
                    {
                        double? lastOrderIndex = null;
                        foreach (var item in items)
                        {
                            if (lastOrderIndex == null)
                            {
                                double? maxOrderIndex = await _publishRepository.GetMaxChildOrderIndex(item.DraftProjectId, transaction);

                                lastOrderIndex = !maxOrderIndex.HasValue || maxOrderIndex < 0d
                                    ? 0d
                                    : maxOrderIndex;
                            }

                            lastOrderIndex += 10.0;

                            if (env.GetArtifactBaseType(item.ArtifactId).IsRegularArtifactType())
                            {
                                await _publishRepository.SetParentAndOrderIndex(item.DraftVersionId, item.DraftProjectId, lastOrderIndex.Value);
                            }
                        }
                    }
                }
            }
        }

        private bool IsOrderIndexChanged(SqlDraftAndLatestItem item)
        {
            return item.LatestVersionId.HasValue &&
                item.LatestOrderIndex != item.DraftOrderIndex;
        }

        private bool IsParentChanged(SqlDraftAndLatestItem item)
        {
            return item.LatestVersionId.HasValue &&
                item.LatestParentId != item.DraftParentId;
        }

        private bool IsNew(SqlDraftAndLatestItem item)
        {
            return !item.LatestVersionId.HasValue;
        }

        private bool IsArtifact(SqlDraftAndLatestItem item)
        {
            return item.ArtifactId == item.ItemId;
        }
    }
}