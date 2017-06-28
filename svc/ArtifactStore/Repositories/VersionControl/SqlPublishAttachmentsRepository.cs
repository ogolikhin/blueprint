using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using ArtifactStore.Helpers;
using Dapper;
using ServiceLibrary.Models;
using ServiceLibrary.Models.Enums;
using ServiceLibrary.Models.VersionControl;
using ServiceLibrary.Repositories;

namespace ArtifactStore.Repositories.VersionControl
{
    public class SqlPublishAttachmentsRepository : SqlPublishRepository, IPublishRepository
    {
        protected class DraftAndLatestAttachment : SingleArtifactData
        {
            public int AttachmentId { get; set; }

            public bool Changed { get; set; }
        }

        protected override string MarkAsLatestStoredProcedureName { get; } = "MarkAsLatestAttachmentVersions";
        protected override string DeleteVersionsStoredProcedureName { get; } = "RemoveAttachmentVersions";
        protected override string CloseVersionsStoredProcedureName { get; } = "";
        protected override string GetDraftAndLatestStoredProcedureName { get; } = "";

        public async Task Execute(int revisionId, PublishParameters parameters, PublishEnvironment environment, IDbTransaction transaction = null)
        {
            //await Task.Run(() => { });
            var artifactIds = parameters.ArtifactIds.ToHashSet();
            var items = await GetDraftAndLatestAttachments(artifactIds, parameters.UserId, transaction);

            if (items.Count == 0)
            {
                return;
            }

            var deleteVersionsIds = new HashSet<int>();
            var closeVersionIds = new HashSet<int>();
            var closeButKeepLatestVersionIds = new HashSet<int>();
            var markAsLatestVersionIds = new HashSet<int>();

            foreach (var item in items)
            {
                if (item.DraftDeleted)
                {
                    deleteVersionsIds.Add(item.DraftVersionId);

                    if (item.LatestVersionId.HasValue)
                    {
                        closeButKeepLatestVersionIds.Add(item.LatestVersionId.Value);
                        RegisterAttachmentModification(environment, item);
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
                        environment.AddAffectedArtifact(item.ArtifactId);
                        RegisterAttachmentModification(environment, item);
                    }
                    else
                    {
                        deleteVersionsIds.Add(item.DraftVersionId);
                    }
                }
            }

            await CloseAttachmentVersions(closeButKeepLatestVersionIds, environment.RevisionId, keepLatest: true, transaction:transaction);
            await CloseAttachmentVersions(closeVersionIds, environment.RevisionId, keepLatest: false, transaction: transaction);
            await MarkAsLatest(markAsLatestVersionIds, environment.RevisionId, transaction);
            await DeleteVersions(deleteVersionsIds, transaction);
        }
        private async Task CloseAttachmentVersions(HashSet<int> closeVersionIds, int revisionId, bool keepLatest, IDbTransaction transaction)
        {
            if (closeVersionIds.Count == 0)
            {
                return;
            }
            var prm = new DynamicParameters();
            prm.Add("@revisionId", revisionId);
            prm.Add("@versionIds", SqlConnectionWrapper.ToDataTable(closeVersionIds));
            prm.Add("@keepLatest", keepLatest);

            if (transaction == null)
            {
                await ConnectionWrapper.ExecuteAsync("CloseAttachmentVersions", prm, commandType: CommandType.StoredProcedure);
            }
             else {
                await transaction.Connection.ExecuteAsync("CloseAttachmentVersions", prm, transaction, commandType: CommandType.StoredProcedure);
            }
            //Log.Assert(updatedRowsCount == closeVersionIds.Count, "Publish: Some attachment versions are not closed");
        }
        private async Task<ICollection<DraftAndLatestAttachment>> GetDraftAndLatestAttachments(ISet<int> artifactIds, int userId, IDbTransaction transaction)
        {
            var prm = new DynamicParameters();
            prm.Add("@userId", userId);
            prm.Add("@artifactIds", SqlConnectionWrapper.ToDataTable(artifactIds));

            if (transaction == null)
            {
                return (await ConnectionWrapper.QueryAsync<DraftAndLatestAttachment>(
                    "GetDraftAndLatestAttachmentVersions", prm, commandType: CommandType.StoredProcedure)).ToList();
            }
            return (await transaction.Connection.QueryAsync<DraftAndLatestAttachment>(
                "GetDraftAndLatestAttachmentVersions", prm, transaction, commandType: CommandType.StoredProcedure)).ToList();
        }

        private void RegisterAttachmentModification(PublishEnvironment env, DraftAndLatestAttachment attachment)
        {
            ReuseSensitivityCollector sensitivityCollector = env.SensitivityCollector;

            var affectedTemplateSetting = IsSubArtifactChange(attachment)
                        ? ItemTypeReuseTemplateSetting.Subartifacts
                        : (env.GetArtifactBaseType(attachment.ArtifactId) == ItemTypePredefined.Document
                            //for document artifact we have only document content as an attachment
                            ? ItemTypeReuseTemplateSetting.DocumentFile
                            : ItemTypeReuseTemplateSetting.Attachments);

            sensitivityCollector.RegisterArtifactModification(attachment.ArtifactId,
                affectedTemplateSetting);
        }

        private bool IsSubArtifactChange(SingleArtifactData attachment)
        {
            return attachment.ArtifactId != attachment.ItemId;
        }

        private bool IsChanged(DraftAndLatestAttachment item)
        {
            return item.Changed;
        }
    }
}